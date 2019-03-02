using Sitecore.Data.Fields;
using Sitecore.Pipelines;

namespace Hackathon.SDN.Foundation.TranslationService.Infrastructure.Pipelines.FieldShouldBeTranslated {

    public class FieldShouldBeTranslatedPipelineArgs : PipelineArgs {

        public FieldShouldBeTranslatedPipelineArgs(Field field) {
            Field = field;
        }

        public Field Field { get; set; }

        public bool ShouldBeTranslated { get; set; } = true;

    }

}