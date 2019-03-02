﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Hackathon.SDN.Foundation.TranslationService.Exceptions;
using Hackathon.SDN.Foundation.TranslationService.Providers;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.SecurityModel;
using Sitecore.StringExtensions;

namespace Hackathon.SDN.Foundation.TranslationService.Services {

    public class TranslationService : ITranslationService {

        private readonly Database _masterDb;

        private readonly IEnumerable<Language> _allAvailableLanguages;

        private readonly ITranslationProvider _translationProvider;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="translationProvider"></param>
        public TranslationService(ITranslationProvider translationProvider) {
            _masterDb = Factory.GetDatabase("master");
            _allAvailableLanguages = _masterDb.GetLanguages();
            _translationProvider = translationProvider;
        }

        /// <summary>
        /// Translate the item
        /// </summary>
        /// <param name="sourceItem">The source item</param>
        /// <param name="targetLanguage">The target language</param>
        /// <param name="includeRelatedItems">returns if the related items should be translated also</param>
        /// <param name="includeSubItems">returns if the sub items should be translated also</param>
        /// <returns></returns>
        public string TranslateItem(Item sourceItem, Language targetLanguage, bool includeRelatedItems, bool includeSubItems) {

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

            TranslateItem(sourceItem, targetItem, includeSubItems, includeRelatedItems);

            return Translate.Text("TheItem") + " \"" + sourceItem.DisplayName + "\" " +
                   Translate.Text("GotSuccessfullyTranslatedToLanguage") + " " + targetLanguage.CultureInfo.DisplayName;
        }

        #region Translate curren item

        private void TranslateItem(Item sourceItem, Item targetItem, bool includeSubItems, bool includeRelatedItems) {

            PrepareTargetItem(targetItem);

            TranslateItemTextFields(sourceItem, targetItem);

            // Check if sub items should be translated too
            if (includeSubItems && sourceItem.Children.Any()) {
                foreach (Item childSourceItem in sourceItem.Children) {
                    var targetChildItem = GetTargetItemInLanguage(childSourceItem.ID, targetItem.Language);

                    TranslateItem(childSourceItem, targetChildItem, true, includeRelatedItems);
                }
            }

            if (includeRelatedItems) {
                TranslateRelatedItems(sourceItem, targetItem);
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
                throw new ItemHasAlreadyTargetLanguageException();
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
                    if (IsStandardTempalteField(itemField)) {
                        continue; // Skip Sitecore standard fields
                    }

                    if (itemField.Value.IsNullOrEmpty()) {
                        continue; // skip empty fields
                    }

                    string translation;
                    if (FieldTypeManager.GetField(itemField) is TextField) {
                        translation = _translationProvider.GetTranslatedContent(itemField.Value, sourceItem.Language.CultureInfo.TwoLetterISOLanguageName, targetItem.Language.CultureInfo.TwoLetterISOLanguageName);
                    } else if (FieldTypeManager.GetField(itemField) is HtmlField) {
                        translation = GetTranslatedHtmlContent(itemField.Value, sourceItem.Language.CultureInfo.TwoLetterISOLanguageName, targetItem.Language.CultureInfo.TwoLetterISOLanguageName);
                    } else {
                        continue;
                    }

                    if (!translation.IsNullOrEmpty()) {

                        targetItem.Fields[itemField.Name].Value = translation;
                    }
                }

            }
        }

        /// <summary>
        /// Returns if the current field is a field of the Sitecore standard template
        /// </summary>
        /// <param name="field">The current field</param>
        /// <returns>If the field is part of the Sitecore standard template</returns>
        public static bool IsStandardTempalteField(Field field) {
            var template = Sitecore.Data.Managers.TemplateManager.GetTemplate(Settings.DefaultBaseTemplate, field.Database);
            Sitecore.Diagnostics.Assert.IsNotNull(template, "template");

            return template.ContainsField(field.ID);
        }

        /// <summary>
        /// Returns translated html content
        /// </summary>
        /// <param name="sourceText">The source text</param>
        /// <param name="sourceLanguage">The source language</param>
        /// <param name="targetLanguage">The target language</param>
        /// <returns>The translated content</returns>
        private string GetTranslatedHtmlContent(string sourceText, string sourceLanguage, string targetLanguage) {
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
                        var translation = _translationProvider.GetTranslatedContent(text, sourceLanguage, targetLanguage);
                        translationString += translation;
                        i += 4;
                    }
                }
                if (toBeTranslatedList[i].StartsWith("<")) {
                    translationString += toBeTranslatedList[i];
                } else {
                    var translation = _translationProvider.GetTranslatedContent(toBeTranslatedList[i], sourceLanguage, targetLanguage);
                    translationString += HttpUtility.HtmlEncode(translation);
                }
            }

            return translationString;
        }

        #endregion

        #region TranslateRelatedItems

        /// <summary>
        /// Translate the related items
        /// </summary>
        /// <param name="sourceItem">The source item</param>
        /// <param name="targetItem">The target item</param>
        private void TranslateRelatedItems(Item sourceItem, Item targetItem) {
            // TODO: Implement this feature
        }

        #endregion
    }
}
