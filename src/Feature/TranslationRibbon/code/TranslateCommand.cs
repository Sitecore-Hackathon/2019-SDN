using System;
using System.Linq;
using System.Text;
using System.Threading;
using Hackathon.SDN.Feature.TranslationRibbon.Exceptions;
using Hackathon.SDN.Foundation.TranslationService.Exceptions;
using Hackathon.SDN.Foundation.TranslationService.Factories;
using Hackathon.SDN.Foundation.TranslationService.Models;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Jobs;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Shell.Applications.Dialogs.ProgressBoxes;

namespace Hackathon.SDN.Feature.TranslationRibbon {

    // ReSharper disable once UnusedMember.Global
    public class TranslateCommand : Command {

        public string Result { get; set; }

        public override void Execute(CommandContext context) {
            try {
                var targetLanguage = GetTargetLanguage(context);
                var includeSubItems = GetIncludeSubItems(context);
                var sourceItem = GetSourceItem(context);

                var obj = new[]
                {
                    (object) sourceItem,
                    targetLanguage,
                    includeSubItems
                };

                ProgressBox.Execute("TranslateItem", "TranslationRibbon_TranslateItem", TranslateItemAsyc, obj);

                var job = JobManager.GetJobs().FirstOrDefault(j => j.Name.Equals("TranslateItem"));
                while (job.Status.State != JobState.Finished) {
                    Thread.Sleep(100); // wait until job is finished
                }

                Alert(Result);

            } catch (Exception) {
                Result = Translate.Text("TranslationRibbon_GeneralErrorInfo");
            }
        }

        public void TranslateItemAsyc(params object[] parameters) {
            try {
                var translationService = TranslationServiceFactory.Create();

                var translationResult = translationService.TranslateItem((Item)parameters[0], (Language)parameters[1], (bool)parameters[2], Context.Job);

                Result = GetResultOutput(translationResult);
            } catch (LanguageNotDifferentException) {
                Result = Translate.Text("TranslationRibbon_SameLanguageInfo");
            } catch (ItemHasAlreadyTargetLanguageException) {
                Result = Translate.Text("TranslationRibbon_TargetLanguageAlreadyExistsInfo");
            } catch (Exception) {
                Result = Translate.Text("TranslationRibbon_GeneralErrorInfo");
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