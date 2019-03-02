using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using Hackathon.SDN.Foundation.TranslationService.Factories;
using Hackathon.SDN.Foundation.TranslationService.Providers;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Shell.Web.UI.WebControls;
using Sitecore.Web.UI.WebControls.Ribbons;

namespace Hackathon.SDN.Feature.TranslationRibbon {

    public class TranslationPanel : RibbonPanel {

        private readonly ITranslationProvider _translationProvider;

        private readonly IEnumerable<Language> _allAvailableLanguages;

        public TranslationPanel(ITranslationProvider translationProvider, Database database) {
            _translationProvider = translationProvider;
            _allAvailableLanguages = database.GetLanguages();
        }

        public TranslationPanel() : this(TranslationProviderFactory.Create(), Factory.GetDatabase("master")) { }

        public override void Render(HtmlTextWriter output, Ribbon ribbon, Item button, CommandContext context) {
            Language currentContextLanguage = null;
            if (context.Items != null && context.Items.Any()) {
                currentContextLanguage = context.Items.First().Language;
            }

            if (currentContextLanguage == null) {
                return;
            }

            var sb = new StringBuilder();

            sb.Append("<div>");
            sb.Append("<a id=\"deepl-button\" href=\"#\" class=\"scRibbonToolbarLargeButton\" title=\"Start Translation\" onclick=\"(function() { let lang = document.getElementById(\'translation-languages\').value;return scForm.invoke(\'Command:TranslateContent(language=\'+lang+\')\', event);})();\">");
            sb.Append("<img src=\"/temp/iconcache/office/24x24/earth_location.png\" class=\"scRibbonToolbarLargeButtonIcon\" border=\"0\" />");
            sb.Append($"<span class=\"header\">{Translate.Text("StartTranslation")}</span>");
            sb.Append("</a>");
            sb.Append("<div style=\"display: inline-block; padding-left: 6px;\">");
            sb.Append($"<p style=\"padding-top: 6px; padding-bottom: 6px;\">{Translate.Text("StartTranslation")}</p>");
            sb.Append("<select id=\"translation-languages\" style=\"width: 100%;\">");

            foreach (var language in _allAvailableLanguages) {
                var isDisabled = SourceLanguageIsTargetLanguage(currentContextLanguage, language) ||
                                 !_translationProvider.CanTranslate(currentContextLanguage, language);

                AddOption(sb, language, isDisabled);
            }

            sb.Append("</select></div></div>");

            var htmlOutput = sb.ToString();

            output.Write(htmlOutput);
        }

        private static void AddOption(StringBuilder sb, Language language, bool isDisabled) {
            sb.Append($"<option value=\"{language.Name.ToLower()}\" {(isDisabled ? "disabled" : string.Empty)}>{language.CultureInfo.Name}</option>");
        }

        private static bool SourceLanguageIsTargetLanguage(Language sourceLanguage, Language targetLanguage) {
            return targetLanguage.CultureInfo.IetfLanguageTag.Equals(sourceLanguage.CultureInfo.IetfLanguageTag);
        }

    }

}