using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Shell.Web.UI.WebControls;
using Sitecore.Web.UI.WebControls.Ribbons;

namespace Hackathon.SDN.Feature.TranslationRibbon {
    public class TranslationPanel : RibbonPanel {

        private readonly IEnumerable<Language> _allAvailableLanguages;

        private readonly Database _masterDb;

        public TranslationPanel() {
            _masterDb = Factory.GetDatabase("master");
            _allAvailableLanguages = _masterDb.GetLanguages();
        }

        public override void Render(HtmlTextWriter output, Ribbon ribbon, Item button, CommandContext context) {
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
                sb.Append($"<option value=\"{language.Name.ToLower()}\">{language.CultureInfo.DisplayName}</option>");
            }

            sb.Append("</select></div></div>");

            var htmlOutput = sb.ToString();

            output.Write(htmlOutput);
        }
    }
}