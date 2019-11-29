using System;
using Xunit;

namespace Salaros.vTiger.WebService.Tests
{
    public class LoginTests
    {
        [Fact]
        public void ChallengePasses()
        {
            // Arrange
            var dateNow = DateTimeOffset.UtcNow;
            var crmClient = FakeClientFactory.GetOne(new
            {
                success = true,
                result = new
                {
                    token = "fakeToken00000",
                    serverTime = dateNow.ToUnixTimeSeconds(),
                    expireTime = dateNow.AddMinutes(5).ToUnixTimeSeconds()
                }
            });

            // Act
            var challengePassed = crmClient.PassChallenge("admin");

            // Assert
            Assert.True(challengePassed);
        }

        [Fact]
        public void UserCanLogin()
        {
            // Arrange
            var crmClient = FakeClientFactory.GetOneAfterChallenge(new
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
            });
            var credentials = new ClientCredentials("admin", "2-G+B8AvgtQ:598A");

            // Act
            var loginResult = crmClient.LoginAfterChallenge(credentials);

            // Assert
            Assert.True(loginResult);
        }
    }
}
