using System;
using System.Linq;
using Hackathon.SDN.Feature.TranslationRibbon.Models;
using Hackathon.SDN.Foundation.TranslationService.Exceptions;
using Hackathon.SDN.Foundation.TranslationService.Factories;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Shell.Framework.Commands;

namespace Hackathon.SDN.Feature.TranslationRibbon
{
    // ReSharper disable once UnusedMember.Global
    public class TranslateCommand : Command
    {
        public override void Execute(CommandContext context)
        {
            try
            {
                var translationService = TranslationServiceFactory.Create();

                var targetLanguage = GetTargetLanguage(context);
                var includeSubItems = GetIncludeSubItems(context);

                var sourceItem = GetSourceItem(context);

                var result = translationService.TranslateItem(sourceItem, targetLanguage, includeSubItems);

                Alert(result);
            }
			catch (LanguageNotDifferentException)
            {
                Alert(Translate.Text("TranslationRibbon_SameLanguageInfo"));
            }
            catch (ItemHasAlreadyTargetLanguageException)
            {
                Alert(Translate.Text("TranslationRibbon_TargetLanguageAlreadyExistsInfo"));
            }
            catch (Exception)
            {
                Alert(Translate.Text("TranslationRibbon_GeneralErrorInfo"));
				throw;
            }
            }
        }

        private Language GetTargetLanguage(CommandContext context)
        {
            var languageCode = context.Parameters["language"];
            if (string.IsNullOrEmpty(languageCode)) throw new Exception("language parameter ist empty");

            Language language;
            Language.TryParse(languageCode, out language);

            if (language == null) throw new LanguageIsInvalidException();

            return language;
        }

        private bool GetIncludeSubItems(CommandContext context) {
            var subItemsIncluded = context.Parameters["include_sub_items"];
            if (subItemsIncluded.Equals("1"))
            {
                return true;
            }

        private Item GetSourceItem(CommandContext context)
        {
            if (context.Items == null || context.Items.Any() == false) throw new NoContextItemFoundException();

            return false;
        }


            return context.Items.First();
        }

        private void Alert(string message)
        {
            if (Context.ClientPage != null && Context.ClientPage.ClientResponse != null)
                Context.ClientPage.ClientResponse.Alert(message);
        }
    }
}