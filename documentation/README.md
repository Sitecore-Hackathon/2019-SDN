# Documentation

## Summary

**Category:** Best enhancement to the Sitecore Admin (XP) UI for Content Editors & Marketers

Sitecore offers two standard translation options: either the editor himself does the translation or he exports the content to be translated and sends it to a translation agency. Taking this concept one step further, it is also possible to translate the content through third-party providers such as "LanguageWire" or "LionBridge". Via connectors offered by these providers, the content is sent to the respective translation team, processed and then returned back to Sitecore.

While the manual, human-powered translation approach works really well, it has one major drawback: it is very expensive and takes a long time to get results. Machine learning and AI have improved significantly in recent years, so that high-quality translations can now be achieved automatically.

The aim of our extension is to leverage AI-based translation services to translate Sitecore content quickly and at low costs. Editors should be able to translate content via a simple button click. Initially, deepl.com will be used as a background service, however the module is easily extensible which allows the implementation of custom translation providers.

## Pre-requisites

In order to use the deepl translation provider, you'll need an API key. You can sign-up for a free 30-day trial [here](https://www.deepl.com/pro.html). 

## Installation

Provide detailed instructions on how to install the module, and include screenshots where necessary.

1. Use the Sitecore Installation wizard to install the [package](../sc.package/ContentTranslator.zip?raw=true)
2. Optional: Add your own deepl API key (see [Configuration](#configuration) section)
3. ???
4. Profit

## Usage

After installation, the content translator module is visible in the versions ribbon of the content editor:

![Placement in the versions tab](images/versions-tab.png?raw=true "Placement in the versions tab")

To use the model, navigate to the item you want to translate and open it. It's important to have to correct language version opened as this is the source for our translation.

If that's done, you can select the language you want to translate to over the dropdown field. Please keep in mind that you can only translate into languages that exist on your sitecore instance (`/sitecore/system/Languages`). If the installed translation provider doesn't support the language or it's the opened language, it's not possible to select it.

![Opened language selection](images/dropdown-open.png?raw=true "Opened language selection")

You can start the translation by clicking the "Start translation" button. In that way, it only translates the item itself. If you like to translate the item itself with all its children, you can click the little arrow. You're now able to click on "Translate item (including subitems)" to do that.

![Opened translation menu](images/opened-menu.png?raw=true "Opened translation menu")

Right after the click, our module starts translating the items by using the installed translation provider. In our example, we will translate this example item:

![Example item](images/example-item.png?raw=true "Example item")

If everything worked as expected, you should see an summary of the changed items:

![Summary message](images/success-message.png?raw=true "Summary message")

The translation is then created as new version for the selected language.

![Translated item](images/created-version.png?raw=true "Translated item")

## Configuration

### Adding the deepl API key
Make sure to add you API key for the deepl translation provider. You can either directly edit the `Project.ContentTranslator.config` file or patch it in using your own config:
```xml
<?xml version="1.0"?>
<configuration xmlns:patch="http://www.sitecore.net/xmlconfig/">
    <sitecore>
        <contentTranslator>
            <deeplClient>
                <param desc="authKey" patch:instead="*[@desc='authKey']">YOUR KEY HERE</param>
            </deeplClient>
        </contentTranslator>
    </sitecore>
</configuration>
```

### Swapping the translation provider
The translation provider to be used is defined in the `Project.ContentTranslator.config` file. If you want to use a different provider, you can either directly edit this config or patch it in using your own config file.

Per default, the deepl translation provider is used. As it requires some more advanced configuration and has some dependencies, you can easily reference the fully wired-up definition via `<translationProvider ref="contentTranslator/deeplTranslationProvider" />`.

To swap the provider, simply assign a different implementation to the `contentTranslator.translationProvider` config property. For example, to use the totally awesome Watman translation provider, which is also provided (pun not intended) with the module, you would adjust the config as following:
```xml
<?xml version="1.0"?>
<configuration xmlns:patch="http://www.sitecore.net/xmlconfig/">
    <sitecore>
        <contentTranslator>
            <translationProvider type="Hackathon.SDN.Foundation.WatmanConnector.Providers.WatmanTranslationProvider, Hackathon.SDN.Foundation.WatmanConnector" />
        </contentTranslator>
    </sitecore>
</configuration>
```

## Translation providers
The module ships with two pre-implemented translation providers.

### deepl.com
Deepl is an AI-based translation service. Their pro plan included API access which allows easy, fast and automated translation of texts between multiple languages. The deepl translation provider makes use of this API.

Currently, the following languages are supported: en, de, fr, es, pt, it, nl, pl and ru

![Demo of the Deepl Provider](images/deepl-provider.png?raw=true "Demo of the Deepl Provider")

### Watman
The Watman translation provider translates the input text according to the "Wat" algorithm outlined by Gary Bernhardt (Destroy all Software) in his infamous talk [Wat](https://www.destroyallsoftware.com/talks/wat). In detail, this means: all words are replaced with "NaN" and "Watman!" is appended to the translation.

![Demo of the Watman Provider](images/watman-provider.png?raw=true "Demo of the Watman Provider")

### Creating a custom provider
To add a custom provider, create a class implementing the following interface:
```csharp
namespace Hackathon.SDN.Foundation.TranslationService.Providers {

    public interface ITranslationProvider {
        
        string Translate(string text, Language sourceLanguage, Language targetLanguage);

        bool CanTranslate(Language sourceLanguage, Language targetLanguage);

    }

}
```

The `string Translate(string text, Language sourceLanguage, Language targetLanguage)` method is used to do the actual translation. The `text` parameter is the untranslated source content (i.e. the content in the `sourceLanguage`). The `targetLanguage` parameter contains the language the content should be translated to. The method should return the translated content.

The `bool CanTranslate(Language sourceLanguage, Language targetLanguage)` method is used to check whether the active provider can handle a translation from the source to the target language. Most service providers are capable of bi-directional translations, however this is not always the case so be careful while implementing the method to prevent errors.

**Note:** The language selection will always show all available languages on the Sitecore instance. If a service provider can not handle a translation, the non-supported languages will be disabled in the dropdown list:

![Example for disabled Languages](images/disabled-languages.png?raw=true "Example for disabled Languages")

See `WatmanTranslationProvider` for an easy sample implementation.

After you have implemented you provider, you need to swap it in. Please see [Configuration](#configuration) section for details.

## Determining whether a field should be translated or not and why is this headline so long
Not all fields should or must be translated. One such example are shared fields, e.g. key of a dictionary entry.

By default, the module will check the following cases:
* Whether the field contains some content or not. If it is empty, it does not need to be translated.
* Whether the field is rated by Sitecore to be translated or not. The check is done through the `ShouldBeTranslated` property of the `Field`.
* Whether the field is part of the Standard Template or not. If it is a standard field, it will not be translated.

All other text fields will be translated per default, though you can easily extend or modify the checks. To determine whether a field should be translated, the `fieldShouldBeTranslated` pipeline of the `contentTranslator` pipeline domain/group is used. All processors receive an instance of the `FieldShouldBeTranslatedPipelineArgs`. 

```csharp
public class FieldShouldBeTranslatedPipelineArgs : PipelineArgs {

    public Field Field { get; set; }

    public bool ShouldBeTranslated { get; set; } = true;

}
```

To disable translation of a field, simply set the `ShouldBeTranslated` property to false:
```csharp
namespace YourAssembly {

    public class YourCheckClass {

        public void Process(FieldShouldBeTranslatedPipelineArgs args) {
            var myCheck = false;

            if (!myCheck) {
                args.ShouldBeTranslated = false;
            }
        }

    }

}
```

To add a custom check, simple add a processor to the `fieldShouldBeTranslated` pipeline:
```xml
<?xml version="1.0"?>
<configuration xmlns:patch="http://www.sitecore.net/xmlconfig/">
    <sitecore>
        <pipelines>
            <group name="contentTranslator" groupName="contentTranslator">
                <pipelines>
                    <fieldShouldBeTranslated>
                        <processor type="YourAssembly.YourCheckClass, YourAssembly" />
                    </fieldShouldBeTranslated>
                </pipelines>
            </group>
        </pipelines>
    </sitecore>
</configuration>
```

**Please note** that the `ShouldBeTranslated` property has a default value of `true`, which means translation is enabled for all fields and needs to be explicitly disabled (although the three built-in processors already catch all common cases).

## Video

Please provide a video highlighing your Hackathon module submission and provide a link to the video. Either a [direct link](https://github.com/Sitecore-Hackathon/2019-SDN/blob/develop/documentation/TranslationModule.swf) to the video, upload it to this documentation folder or maybe upload it to Youtube...

[![Sitecore Hackathon Video Embedding Alt Text](https://img.youtube.com/vi/EpNhxW4pNKk/0.jpg)](https://github.com/Sitecore-Hackathon/2019-SDN/blob/develop/documentation/TranslationModule.swf)

## Further resources
* [deepl API documentation](https://www.deepl.com/de/api.html)
* [Wat algorithm documentation](https://www.destroyallsoftware.com/talks/wat)