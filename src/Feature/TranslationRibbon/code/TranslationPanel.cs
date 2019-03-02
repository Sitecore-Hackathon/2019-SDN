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

    // ReSharper disable once UnusedMember.Global
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
            
            #region Build HTML

            // Language Selector HTML
            sb.Append("<div style=\"display: inline-block; padding-left: 6px;\">");
            sb.Append($"<p style=\"padding-top: 6px; padding-bottom: 6px;\">{Translate.Text("SelectTargetLanguage")}</p>");
            sb.Append("<select id=\"translation-languages\" style=\"width: 100%;\">");

            foreach (var language in _allAvailableLanguages) {
                var isDisabled = SourceLanguageIsTargetLanguage(currentContextLanguage, language) ||
                                 !_translationProvider.CanTranslate(currentContextLanguage, language);

                AddOption(sb, language, isDisabled);
            }

            sb.Append("</select></div>");

            // Button HTML
            sb.Append("<div class=\"scRibbonToolbarLargeComboButton\">");
            sb.Append("<a id=\"deepl-button\" href=\"#\" class=\"scRibbonToolbarLargeComboButtonTop\" title=\"" + Translate.Text("StartTranslation") + "\" onclick=\"(function() { let lang = document.getElementById(\'translation-languages\').value;return scForm.invoke(\'Command:TranslateContent(language=\'+ lang + \', include_sub_items=0)\', event);})();\">");
            sb.Append("<img src=\"/temp/iconcache/office/24x24/earth_location.png\" class=\"scRibbonToolbarLargeButtonIcon\" border=\"0\" />");
            sb.Append("</a>");
            sb.Append("<a id=\"deepl-button\" href=\"#\" class=\"scRibbonToolbarLargeComboButtonBottom\" title=\"Start Translation\" onclick=\"javascript:return scContent.showMenu(this,event,'translations_menu')\">");
            sb.Append($"<span class=\"header\">{Translate.Text("StartTranslation")}</span><img src=\"/sitecore/shell/themes/standard/Images/ribbondropdown.gif\" class=\"scRibbonToolbarLargeComboButtonGlyph\" alt=\"\" border=\"0\" />");
            sb.Append("<table id=\"translations_menu\" class=\"scMenu\" style=\"display:none;\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");
            sb.Append("<tbody>");
            sb.Append("<tr id=\"menu_id_1\" onmouseover=\"javascript:return scForm.rollOver(this,event)\" onfocus=\"javascript:return scForm.rollOver(this,event)\" onmouseout=\"javascript:return scForm.rollOver(this,event)\" onblur=\"javascript:return scForm.rollOver(this,event)\" onclick=\"(function() { let lang = document.getElementById(\'translation-languages\').value;return scForm.invoke(\'Command:TranslateContent(language=\' + lang + \', include_sub_items=1)\', event);})();\">");
            sb.Append("<td class=\"scMenuItemIcon\"><img src=\"/temp/iconcache/office/24x24/earth_location.png\" width=\"16\" height=\"16\" align=\"middle\" class=\"\" alt=\"\" border=\"0\"></td>");
            sb.Append($"<td class=\"scMenuItemCaption\">{Translate.Text("TranslateItemIncludingSubItems")}</td>");
            sb.Append("<td class=\"scMenuItemHotkey\"><img src=\"/sitecore/images/blank.gif\" width=\"1\" height=\"1\" class=\"scSpacer\" alt=\"\" border=\"0\"></td>");
            sb.Append("</tr>");
            sb.Append("</tbody>");
            sb.Append("</table>");
            sb.Append("</a>");
            sb.Append("</div>");
            
            #endregion

            var htmlOutput = sb.ToString();

            output.Write(htmlOutput);
        }

        private static void AddOption(StringBuilder sb, Language language, bool isDisabled) {
            sb.Append($"<option value=\"{language.Name.ToLower()}\" {(isDisabled ? "disabled" : string.Empty)}>{language.CultureInfo.DisplayName}</option>");
        }

        private static bool SourceLanguageIsTargetLanguage(Language sourceLanguage, Language targetLanguage) {
            return targetLanguage.CultureInfo.IetfLanguageTag.Equals(sourceLanguage.CultureInfo.IetfLanguageTag);
        }

    }
}

