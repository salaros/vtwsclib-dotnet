using Moq;
using Newtonsoft.Json;
using Salaros.vTiger.WebService;
using Salaros.vTiger.WebService.Tests;
using System;
using System.Net;
using System.Net.Http;
using Xunit;

namespace vtwsclib.tests
{
    public class LoginTests
    {
        protected Client crmClient;

        public LoginTests()
        {
            crmClient = new Client(new Uri("http://vtiger.local/"));
        }

        public HttpClient GetHttpClient<TResponse>(TResponse fakeResponse)
            where TResponse : class
        {
            var fakeHttpMessageHandler = new Mock<FakeHttpMessageHandler> { CallBase = true };
            fakeHttpMessageHandler
                .Setup(f => f.Send(It.IsAny<HttpRequestMessage>()))
                .Returns(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(fakeResponse))
                });
            return new HttpClient(fakeHttpMessageHandler.Object);
        }

        [Fact]
        public void ChallangePasses()
        {
            // Arrange
            var dateNow = DateTimeOffset.UtcNow;
            crmClient.HttpClient = GetHttpClient(
                new
                {
                    success = true,
                    result = new
                    {
                        token = "fakeToken00000",
                        serverTime = dateNow.ToUnixTimeSeconds(),
                        expireTime = dateNow.AddMinutes(5).ToUnixTimeSeconds()
                    }
                }
            );


            // Act
            var challangePassed = crmClient.PassChallenge("admin");

            // Assert
            Assert.True(challangePassed);
        }

        [Fact]
        public void UserCanLogin()
        {
            // Arrange
            crmClient.HttpClient = GetHttpClient(
                new
                {
                    success = true,
                    result = new
                    {
                        sessionNam = "fake000session000name",
                        userId = "19x1",
                        version = "0.22",
                        vtigerVersion = "7.1.0",
                        timezone = 0
                    }
                }
            );

            // Faking passed challange
            crmClient.ServiceInfo.Token = "fakeToken00000";
            crmClient.ServiceInfo.TokenExpiration = DateTimeOffset.UtcNow.AddMinutes(5);

            // Act
            var loginResult = crmClient.LoginAfterChallange("admin", "2-G+B8AvgtQ:598A");

            // Assert
            Assert.True(loginResult);
        }
    }
}
