namespace Hackathon.SDN.Foundation.TranslationService.Interface {
    public interface ITranslationProviderService {

        /// <summary>
        /// Get the translated content
        /// </summary>
        /// <param name="content"></param>
        /// <param name="sourceLanguageCode"></param>
        /// <param name="targetLanguageCode"></param>
        /// <returns></returns>
        string GetTranslatedContent(string content, string sourceLanguageCode, string targetLanguageCode);
    }
}
