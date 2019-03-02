using Hackathon.SDN.Foundation.TranslationService.Providers;
using Sitecore.Configuration;

namespace Hackathon.SDN.Foundation.TranslationService.Factories {

    public static class TranslationProviderFactory {

        public static ITranslationProvider Create() => Factory.CreateObject("contentTranslator/translationProvider", true) as ITranslationProvider;

    }

}