using System;
using System.Linq;
using Hackathon.SDN.Feature.TranslationRibbon.Models;
using Hackathon.SDN.Foundation.TranslationService.Factories;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Shell.Framework.Commands;

namespace Hackathon.SDN.Feature.TranslationRibbon {

    public class TranslateCommand : Command {

        public override void Execute(CommandContext context) {
            try {
                var translationService = TranslationServiceFactory.Create();

                var targetLanguage = GetTargetLanguage(context);
                var includeSubItems = GetIncludeSubItems(context);

                var sourceItem = GetSourceItem(context);

                var result = translationService.TranslateItem(sourceItem, targetLanguage, false, includeSubItems);

                Alert(result);
            } catch (Exception ex) {
                Alert("Error while translating the item");
                throw;
                // TODO: Logging and handling of all possible exceptions
            }
        }

        private Language GetTargetLanguage(CommandContext context) {
            var languageCode = context.Parameters["language"];
            if (string.IsNullOrEmpty(languageCode)) {
                throw new Exception("language parameter ist empty");
            }

            Language language;
            Language.TryParse(languageCode, out language);

            if (language == null) {
                throw new LanguageIsInvalidException();
            }

            return language;
        }

        private bool GetIncludeSubItems(CommandContext context) {
            var subItemsIncluded = context.Parameters["include_sub_items"];
            if (subItemsIncluded.Equals("1"))
            {
                return true;
            }

            return false;
        }

        private Item GetSourceItem(CommandContext context) {
            if (context.Items == null || context.Items.Any() == false) {
                throw new NoContextItemFoundException();
            }

            return context.Items.First();
        }

        private void Alert(string message) {
            if (Context.ClientPage != null && Context.ClientPage.ClientResponse != null) {
                Context.ClientPage.ClientResponse.Alert(message);
            }
        }
    }
}
