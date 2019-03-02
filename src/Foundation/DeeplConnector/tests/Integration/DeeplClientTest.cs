using System.Linq;
using AutoFixture.Xunit2;
using Hackathon.SDN.Foundation.DeeplConnector.Models;
using FluentAssertions;
using Hackathon.SDN.Foundation.DeeplConnector.Tests.Models;
using Newtonsoft.Json;
using Xunit;

namespace Hackathon.SDN.Foundation.DeeplConnector.Tests.Integration {

    public class DeeplClientTest {

        [Fact]
        public void SendTranslationRequestShouldSendSourceAndTargetLanguageInFormBody() {
            // Arrange
            var client = new DeeplClient("http://httpbin.org/anything", string.Empty);

            // Act
            var response = client.SendTranslationRequest("Hello World", "EN", "DE");

            // Assert
            var httpBinResponse = JsonConvert.DeserializeObject<HttpBinResponse>(response);

            httpBinResponse.Form.SourceLang.Should().Be("EN");
            httpBinResponse.Form.TargetLang.Should().Be("DE");
        }

        [Theory]
        [AutoData]
        public void SendTranslationRequestShouldSendAuthKeyInFormBody(string authKey) {
            // Arrange
            var client = new DeeplClient("http://httpbin.org/anything", authKey);

            // Act
            var response = client.SendTranslationRequest("Hello World", "EN", "DE");

            // Assert
            var httpBinResponse = JsonConvert.DeserializeObject<HttpBinResponse>(response);
            httpBinResponse.Form.AuthKey.Should().Be(authKey);
        }

        [Fact]
        public void SendTranslationRequestShouldSendTextUrlEncodedInFormBody() {
            // Arrange
            const string text = "Hello <strong>World</strong>";

            var client = new DeeplClient("http://httpbin.org/anything", string.Empty);

            // Act
            var response = client.SendTranslationRequest(text, "EN", "DE");

            // Assert
            var httpBinResponse = JsonConvert.DeserializeObject<HttpBinResponse>(response);

            httpBinResponse.Form.Text.Should().Be(text);
        }

        [Fact]
        public void SendTranslationRequestShouldAcceptJsonResponses() {
            // Arrange
            var client = new DeeplClient("http://httpbin.org/anything", string.Empty);

            // Act
            var response = client.SendTranslationRequest("Hello World", "EN", "DE");

            // Assert
            var httpBinResponse = JsonConvert.DeserializeObject<HttpBinResponse>(response);

            httpBinResponse.Headers.Accept.Should().Be("application/json");
        }

        [Fact]
        public void SendTranslationShouldTranslate() {
            // Arrange
            var client = new DeeplClient("https://api.deepl.com/v2/translate", "ece9269e-5399-2e8d-14e8-a5e70436555e");

            // Act
            var response = client.SendTranslationRequest("Hello World", "EN", "DE");

            // Assert
            var deeplResponse = JsonConvert.DeserializeObject<DeeplResponse>(response);

            deeplResponse.Translations.First().Text.Should().Be("Hallo Welt");
        }
        
    }

}
