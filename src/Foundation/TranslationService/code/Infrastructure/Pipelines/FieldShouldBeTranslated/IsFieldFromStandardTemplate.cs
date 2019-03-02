using Sitecore.Configuration;

namespace Hackathon.SDN.Foundation.TranslationService.Infrastructure.Pipelines.FieldShouldBeTranslated {

    public class IsFieldFromStandardTemplate {

        public void Process(FieldShouldBeTranslatedPipelineArgs args) {
            var standardTemplate = Sitecore.Data.Managers.TemplateManager.GetTemplate(Settings.DefaultBaseTemplate, args.Field.Database);

            if (standardTemplate.ContainsField(args.Field.ID)) {
                args.ShouldBeTranslated = false;
            }
        }

    }

}