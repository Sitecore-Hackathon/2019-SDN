using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Hackathon.SDN.Foundation.TranslationService.Exceptions;
using Hackathon.SDN.Foundation.TranslationService.Infrastructure.Pipelines.FieldShouldBeTranslated;
using Hackathon.SDN.Foundation.TranslationService.Models;
using Hackathon.SDN.Foundation.TranslationService.Providers;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Pipelines;
using Sitecore.SecurityModel;
using Sitecore.StringExtensions;

namespace Hackathon.SDN.Foundation.TranslationService.Services {

    // ReSharper disable once UnusedMember.Global
    public class TranslationService : ITranslationService {

        private readonly Database _masterDb;

        private readonly IEnumerable<Language> _allAvailableLanguages;

        public ITranslationProvider TranslationProvider { get; }

        public TranslationService(ITranslationProvider translationProvider) {
            _masterDb = Factory.GetDatabase("master");
            _allAvailableLanguages = _masterDb.GetLanguages();
            TranslationProvider = translationProvider;
        }


        /// <summary>
        /// Translate the item
        /// </summary>
        /// <param name="sourceItem">The source item</param>
        /// <param name="targetLanguage">The target language</param>
        /// <param name="includeSubItems">returns if the sub items should be translated also</param>
        /// <returns></returns>
        /// <exception cref="ItemIsNullException">Throw when item is null</exception>
        /// <exception cref="LanguageMissingException">Throw when target language is not set</exception>
        /// <exception cref="LanguageNotDifferentException">Throw when target language is the same as source language</exception>
        public TranslationResult TranslateItem(Item sourceItem, Language targetLanguage, bool includeSubItems) {

            // Check input
            if (sourceItem == null) {
                throw new ItemIsNullException();
            }

            if (targetLanguage == null) {
                throw new LanguageMissingException();
            }

            if (sourceItem.Language.CultureInfo.IetfLanguageTag.Equals(targetLanguage.CultureInfo.IetfLanguageTag)) {
                throw new LanguageNotDifferentException();
            }

            var targetItem = GetTargetItemInLanguage(sourceItem.ID, targetLanguage);

            var result = new TranslationResult();

            TranslateItemRecursive(sourceItem, targetItem, includeSubItems, result);

            return result;
        }

        #region Translate current item

        /// <summary>
        /// Translate the item
        /// </summary>
        /// <param name="sourceItem"></param>
        /// <param name="targetItem"></param>
        /// <param name="includeSubItems"></param>
        /// <param name="result">The translation result</param>
        private void TranslateItemRecursive(Item sourceItem, Item targetItem, bool includeSubItems, TranslationResult result) {
            try {
                PrepareTargetItem(targetItem);

                TranslateItemTextFields(sourceItem, targetItem);

                result.SuccessfullyTranslated++;
            } catch (ItemHasAlreadyTargetLanguageException) {
                result.Skipped++;
            } catch (ItemIsNullException) {
                result.OccuredErrors++;
                result.ItemsWithErrors.Add(sourceItem.ID);
            } catch (TranslationFailureException) {
                result.OccuredErrors++;
                result.ItemsWithErrors.Add(sourceItem.ID);
            }

            // Check if sub items should be translated too
            if (includeSubItems && sourceItem.Children.Any()) {
                foreach (Item childSourceItem in sourceItem.Children) {
                    Item targetChildItem;
                    try {
                        targetChildItem = GetTargetItemInLanguage(childSourceItem.ID, targetItem.Language);
                    } catch (ItemIsNullException) {
                        result.OccuredErrors++;
                        result.ItemsWithErrors.Add(childSourceItem.ID);
                        continue;
                    }
                    TranslateItemRecursive(childSourceItem, targetChildItem, true, result);
                }
            }
        }

        #endregion

        #region Target item helper methods

        /// <summary>
        /// Returns the item in target language
        /// </summary>
        /// <param name="itemId">The item Id</param>
        /// <param name="targetLanguage">The target language</param>
        /// <returns>The item in target language</returns>
        private Item GetTargetItemInLanguage(ID itemId, Language targetLanguage) {
            if (_allAvailableLanguages.Any(x => x.CultureInfo.IetfLanguageTag.Equals(targetLanguage.CultureInfo.IetfLanguageTag)) == false) {
                throw new LanguageDidNotExistException();
            }

            var item = _masterDb.GetItem(itemId, targetLanguage);
            if (item == null) {
                throw new ItemIsNullException();
            }

            return item;
        }

        private void PrepareTargetItem(Item item) {
            if (item.Versions.Count > 0) {
                throw new ItemHasAlreadyTargetLanguageException();
            }

            var workflow = item.Database.WorkflowProvider.GetWorkflow(item);
            if (workflow != null) {
                var workflowState = workflow.GetState(item);
                if (workflowState.FinalState) {
                    item.Versions.AddVersion();
                }
            } else {
                item.Versions.AddVersion();
            }
        }

        #endregion

        #region Translate item fields


        /// <summary>
        /// Translate the item fields
        /// </summary>
        /// <param name="sourceItem">The source item</param>
        /// <param name="targetItem">The target item</param>
        private void TranslateItemTextFields(Item sourceItem, Item targetItem) {

            using (new SecurityDisabler())
            using (new BulkUpdateContext())
            using (new EditContext(targetItem)) {

                foreach (Field itemField in sourceItem.Fields) {
                    if (!FieldShouldBeTranslated(itemField)) {
                        continue;
                    }

                    string translation;
                    if (FieldTypeManager.GetField(itemField) is TextField) {
                        translation = TranslationProvider.Translate(itemField.Value, sourceItem.Language, targetItem.Language);
                    } else if (FieldTypeManager.GetField(itemField) is HtmlField) {
                        translation = GetTranslatedHtmlContent(itemField.Value, sourceItem.Language, targetItem.Language);
                    } else {
                        continue;
                    }

                    if (!translation.IsNullOrEmpty()) {

                        targetItem.Fields[itemField.Name].Value = translation;
                    }
                }

            }
        }

        private static bool FieldShouldBeTranslated(Field field) {
            var args = new FieldShouldBeTranslatedPipelineArgs(field);

            CorePipeline.Run("fieldShouldBeTranslated", args, "contentTranslator");

            return args.ShouldBeTranslated;
        }

        /// <summary>
        /// Returns translated html content
        /// </summary>
        /// <param name="sourceText">The source text</param>
        /// <param name="sourceLanguage">The source language</param>
        /// <param name="targetLanguage">The target language</param>
        /// <returns>The translated content</returns>
        private string GetTranslatedHtmlContent(string sourceText, Language sourceLanguage, Language targetLanguage) {
            var htmlDecodedSourceText = HttpUtility.HtmlDecode(sourceText);
            if (htmlDecodedSourceText == null) {
                throw new Exception("htmlDecodedSourceText is null");
            }

            htmlDecodedSourceText = htmlDecodedSourceText.Replace("\n", "");

            var toBeTranslatedList = Regex.Split(htmlDecodedSourceText, @"(<.*?>)").Where(t => !t.Equals(string.Empty)).ToList();
            var translationString = string.Empty;

            for (var i = 0; i < toBeTranslatedList.Count; i++) {
                if (i + 4 <= toBeTranslatedList.Count) {
                    if (toBeTranslatedList[i + 1].StartsWith("<a") || toBeTranslatedList[i + 1].StartsWith("< a")) {
                        var text = toBeTranslatedList[i] + toBeTranslatedList[i + 1] + toBeTranslatedList[i + 2] + toBeTranslatedList[i + 3] + toBeTranslatedList[i + 4];
                        var translation = TranslationProvider.Translate(text, sourceLanguage, targetLanguage);
                        translationString += translation;
                        i += 4;
                    }
                }
                if (toBeTranslatedList[i].StartsWith("<")) {
                    translationString += toBeTranslatedList[i];
                } else {
                    var translation = TranslationProvider.Translate(toBeTranslatedList[i], sourceLanguage, targetLanguage);
                    translationString += HttpUtility.HtmlEncode(translation);
                }
            }

            return translationString;
        }

        #endregion
    }
}
