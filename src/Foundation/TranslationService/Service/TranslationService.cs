using System.Collections.Generic;
using System.Linq;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.SecurityModel;
using Sitecore.StringExtensions;
using TranslationService.Interface;
using TranslationService.Models;

namespace TranslationService.Service {

    public class TranslationService : ITranslationService {

        private readonly Database _masterDb;

        private IEnumerable<Language> _allAvailableLanguages;

        private readonly ITranslationProviderService _translationProviderService;

        public TranslationService(ITranslationProviderService translationProviderService) {
            _masterDb = Factory.GetDatabase("master");
            _allAvailableLanguages = _masterDb.GetLanguages();
            _translationProviderService = translationProviderService;
        }


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

            TranslateCurrentItem(sourceItem, targetItem);

            if (includeRelatedItems) {
                TranslateRelatedItems(sourceItem, targetItem);
            }

            if (includeSubItems) {
                TranslateSubItems(sourceItem, targetItem);
            }

            return string.Empty; // TODO
        }

        #region TranslateItem

        private void TranslateCurrentItem(Item sourceItem, Item targetItem) {

            PrepareTargetItem(targetItem);

            TranslateItemFields(sourceItem, targetItem);

        }

        #endregion

        #region Translate related items

        private void TranslateRelatedItems(Item sourceItem, Item targetItem) {
            // Not yet implemented
        }

        #endregion

        #region Translate sub items

        private void TranslateSubItems(Item sourceItem, Item targetItem) {
            // Not yet implemented
        }

        #endregion

        #region Target item helper methods

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
        /// <param name="sourceItem"></param>
        /// <param name="targetItem"></param>
        private void TranslateItemFields(Item sourceItem, Item targetItem) {

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
                        translation = _translationProviderService.GetTranslatedContent(itemField.Value, sourceItem.Language.CultureInfo.TwoLetterISOLanguageName, targetItem.Language.CultureInfo.TwoLetterISOLanguageName);
                    } else if (FieldTypeManager.GetField(itemField) is HtmlField) {
                        translation = _translationProviderService.GetTranslatedContent(itemField.Value, sourceItem.Language.CultureInfo.TwoLetterISOLanguageName, targetItem.Language.CultureInfo.TwoLetterISOLanguageName);
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

        #endregion
    }
}
