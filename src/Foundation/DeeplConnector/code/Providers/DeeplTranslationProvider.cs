using System.Linq;
using Hackathon.SDN.Foundation.DeeplConnector.Models;
using Hackathon.SDN.Foundation.TranslationService.Exceptions;
using Hackathon.SDN.Foundation.TranslationService.Providers;
using Newtonsoft.Json;
using Sitecore.Globalization;

namespace Hackathon.SDN.Foundation.DeeplConnector.Providers {

    public class DeeplTranslationProvider : ITranslationProvider {

        private readonly DeeplClient _client;

        public DeeplTranslationProvider(DeeplClient client) {
            _client = client;
        }

        public string Translate(string text, Language sourceLanguage, Language targetLanguage) {
            var response = _client.SendTranslationRequest(text, sourceLanguage.CultureInfo.TwoLetterISOLanguageName, targetLanguage.CultureInfo.TwoLetterISOLanguageName);
            var deeplResponse = JsonConvert.DeserializeObject<DeeplResponse>(response);
            var translation = deeplResponse.Translations?.FirstOrDefault();

            if (translation == null) {
                throw new TranslationFailureException();
            }

            return translation.Text;
        }

        public bool CanTranslate(Language sourceLanguage, Language targetLanguage) {
            return SupportedLanguages.Source.Contains(sourceLanguage.CultureInfo.TwoLetterISOLanguageName) &&
                   SupportedLanguages.Target.Contains(targetLanguage.CultureInfo.TwoLetterISOLanguageName);
        }

    }

}
