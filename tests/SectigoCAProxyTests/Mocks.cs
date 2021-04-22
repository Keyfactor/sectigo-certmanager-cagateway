using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SectigoCAProxyTests
{
    public static class Mocks
    {
        public static Mock<HttpMessageHandler> GetMessageHandler(string responseContent)
        {
            return ConfigureHandler(responseContent);
        }

        public static Mock<HttpMessageHandler> GetMessageHandler(string responseContent, HttpStatusCode statusCode)
        {
            return ConfigureHandler(responseContent, statusCode);
        }

        private static Mock<HttpMessageHandler> ConfigureHandler(string responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var response = new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(responseContent)
            };
            var handlerMock = new Mock<HttpMessageHandler>();
                handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);
            return handlerMock;
        }

        public static string GetFileFromResourceFolder(string fileName)
        {
            return File.ReadAllText($"../../resources/{fileName}");
        }
    }
}
