using System;
using System.Linq;
using System.Text;
using Hackathon.SDN.Feature.TranslationRibbon.Models;
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
using Sitecore.Web.UI.Sheer;

namespace Hackathon.SDN.Feature.TranslationRibbon {

    // ReSharper disable once UnusedMember.Global
    public class TranslateCommand : Command {

        public string Result { get; set; }

        public override void Execute(CommandContext context) {
            var targetLanguage = GetTargetLanguage(context);
            var includeSubItems = GetIncludeSubItems(context);
            var sourceItem = GetSourceItem(context);

            var obj = new[] {
                (object) sourceItem,
                targetLanguage,
                includeSubItems
            };

            ProgressBox.Execute("TranslateItem", "TranslateItem", TranslateItemAsyc, obj);

            var isJobDone = JobManager.GetJobs().FirstOrDefault(j => j.Name.Equals("TranslateItem") && j.Status.State == JobState.Running);
            if (isJobDone != null && !isJobDone.IsDone) {
                SheerResponse.Timer("CheckTranslationStatus", 100);
            } else {
                Alert(Result);
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
            if (string.IsNullOrEmpty(languageCode)) throw new Exception("language parameter ist empty");

            Language language;
            Language.TryParse(languageCode, out language);

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
            sb.AppendLine("Translation progress finished, result:");
            sb.AppendLine("Success: " + result.SuccessfullyTranslated);
            sb.AppendLine("Skipped: " + result.Skipped);
            sb.AppendLine("Errors: " + result.OccuredErrors);

            foreach (ID itemId in result.ItemsWithErrors) {
                sb.AppendLine("ID of item with failed translation: " + itemId);
            }

            return sb.ToString();
        }
    }
}