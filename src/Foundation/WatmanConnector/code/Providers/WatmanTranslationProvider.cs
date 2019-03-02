using System;
using System.Linq;
using System.Text.RegularExpressions;
using Hackathon.SDN.Foundation.TranslationService.Providers;
using Sitecore.Globalization;

namespace Hackathon.SDN.Foundation.WatmanConnector.Providers {

    public class WatmanTranslationProvider : ITranslationProvider {

        public string Translate(string text, Language sourceLanguage, Language targetLanguage) {
            var words = StripHtml(text).Split(new [] {' ', '.', ',', '!', '?'}, StringSplitOptions.RemoveEmptyEntries);

            return $"{string.Join(" ", words.Select(x => "NaN"))} Watman!";
        }

        public bool CanTranslate(Language sourceLanguage, Language targetLanguage) {
            return true;
        }

        private static string StripHtml(string input) {
            return Regex.Replace(input, "<.*?>", string.Empty);
        }

    }

}
