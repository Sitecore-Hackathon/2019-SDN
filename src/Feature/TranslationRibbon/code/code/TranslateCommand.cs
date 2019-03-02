using System;
using System.Linq;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Shell.Framework.Commands;
using TranslationRibbon.Models;
using TranslationService.Interface;

namespace TranslationRibbon {
    public class TranslateCommand : Command {

        private readonly ITranslationService _translationService;


        public TranslateCommand(ITranslationService translationService) {
            _translationService = translationService;
        }

        public override void Execute(CommandContext context) {
            try {
                var targetLanguage = GetTargetLanguage(context);

                var sourceItem = GetSourceItem(context);

                var result = _translationService.TranslateItem(sourceItem, targetLanguage, false, false);

                Alert(result);
            } catch (Exception ex) {
                Alert("Error while translating the item");
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
