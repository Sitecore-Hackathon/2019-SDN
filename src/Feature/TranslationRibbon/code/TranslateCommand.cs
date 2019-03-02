﻿using System;
using System.Linq;
using System.Text;
using Hackathon.SDN.Feature.TranslationRibbon.Exceptions;
using Hackathon.SDN.Foundation.TranslationService.Exceptions;
using Hackathon.SDN.Foundation.TranslationService.Factories;
using Hackathon.SDN.Foundation.TranslationService.Models;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Shell.Framework.Commands;

namespace Hackathon.SDN.Feature.TranslationRibbon {
    // ReSharper disable once UnusedMember.Global
    public class TranslateCommand : Command {
        public override void Execute(CommandContext context) {
            try {
                var translationService = TranslationServiceFactory.Create();

                var targetLanguage = GetTargetLanguage(context);
                var includeSubItems = GetIncludeSubItems(context);

                var sourceItem = GetSourceItem(context);

                var translationResult = translationService.TranslateItem(sourceItem, targetLanguage, includeSubItems);

                var resultOutput = GetResultOutput(translationResult);

                Alert(resultOutput);
            } catch (LanguageNotDifferentException) {
                Alert(Translate.Text("TranslationRibbon_SameLanguageInfo"));
            } catch (ItemHasAlreadyTargetLanguageException) {
                Alert(Translate.Text("TranslationRibbon_TargetLanguageAlreadyExistsInfo"));
            } catch (Exception) {
                Alert(Translate.Text("TranslationRibbon_GeneralErrorInfo"));
                throw;
            }
        }

        private Language GetTargetLanguage(CommandContext context) {
            var languageCode = context.Parameters["language"];
            if (string.IsNullOrEmpty(languageCode)) throw new LanguageMissingException();

            Language.TryParse(languageCode, out Language language);

            if (language == null) throw new LanguageIsInvalidException();

            return language;
        }

        private bool GetIncludeSubItems(CommandContext context) {
            var subItemsIncluded = context.Parameters["include_sub_items"];
            if (subItemsIncluded.Equals("1")) return true;

            return false;
        }

        private Item GetSourceItem(CommandContext context) {
            if (context.Items == null || context.Items.Any() == false) throw new NoContextItemFoundException();

            return context.Items.First();
        }

        private void Alert(string message) {
            if (Context.ClientPage != null && Context.ClientPage.ClientResponse != null)
                Context.ClientPage.ClientResponse.Alert(message);
        }

        private string GetResultOutput(TranslationResult result) {
            var sb = new StringBuilder();
            sb.AppendLine(Translate.Text("TranslationRibbon_ResultDialog_Headline"));
            sb.AppendLine(Translate.Text("TranslationRibbon_ResultDialog_Success") + result.SuccessfullyTranslated);
            sb.AppendLine(Translate.Text("TranslationRibbon_ResultDialog_Skipped") + result.Skipped);
            sb.AppendLine(Translate.Text("TranslationRibbon_ResultDialog_Errors") + result.OccuredErrors);

            foreach (ID itemId in result.ItemsWithErrors) {
                sb.AppendLine(Translate.Text("TranslationRibbon_ResultDialog_Errors_Description") + itemId);
            }

            return sb.ToString();
        }
    }
}