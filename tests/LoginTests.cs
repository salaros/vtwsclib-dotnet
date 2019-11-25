using System;
using Xunit;

namespace Salaros.vTiger.WebService.Tests
{
    public class LoginTests
    {
        public ClientOptions ClientOptions => new ClientOptions
        {
            BaseUrl = new Uri("http://vtiger.local/")
        };

        [Fact]
        public void ChallangePasses()
        {
            // Arrange
            var dateNow = DateTimeOffset.UtcNow;
            var crmClient = new Client(ClientOptions, FakeHttpMessageHandler.GetHttpClient(
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
            ));

            // Act
            var challangePassed = crmClient.PassChallenge("admin");

            // Assert
            Assert.True(challangePassed);
        }

        [Fact]
        public void UserCanLogin()
        {
            // Arrange
            var options = ClientOptions;
            options.Credentials = new ClientCredentials("admin", "2-G+B8AvgtQ:598A");

            var crmClient = new Client(options, FakeHttpMessageHandler.GetHttpClient(
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
            ));

            // Faking passed challange
            crmClient.ServiceInfo.Token = "fakeToken00000";
            crmClient.ServiceInfo.TokenExpiration = DateTimeOffset.UtcNow.AddMinutes(5);

            // Act
            var loginResult = crmClient.LoginAfterChallenge(options.Credentials);

            // Assert
            Assert.True(loginResult);
        }
    }
}
