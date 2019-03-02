using System.Linq;
using Hackathon.SDN.Foundation.DeeplConnector.Models;
using Hackathon.SDN.Foundation.TranslationService.Exceptions;
using Hackathon.SDN.Foundation.TranslationService.Providers;
using Newtonsoft.Json;

namespace Hackathon.SDN.Foundation.DeeplConnector.Providers {

    public class DeeplTranslationProvider : ITranslationProvider {

        private readonly DeeplClient _client;

        public DeeplTranslationProvider(DeeplClient client) {
            _client = client;
        }

        public string GetTranslatedContent(string sourceText, string sourceLanguageCode, string targetLanguageCode) {
            var response = _client.SendTranslationRequest(sourceText, sourceLanguageCode, targetLanguageCode);
            var deeplResponse = JsonConvert.DeserializeObject<DeeplResponse>(response);
            var translation = deeplResponse.Translations?.FirstOrDefault();

            if (translation == null) {
                throw new TranslationFailureException();
            }

            return translation.Text;
        }

    }

}
