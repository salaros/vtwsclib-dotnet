using System.Linq;
using Newtonsoft.Json;
using Xunit;

namespace Salaros.vTiger.WebService.Tests
{
    public class QueryResultTests
    {
        [Fact]
        public void CountWorks()
        {
            // Arrange
            var crmClient = FakeClientFactory.GetOneLoggedIn(
                new {success = true, result = new[] {new {count = 11}}}
            );

            // Act
            var count = crmClient
                .UseModule("Contacts")
                .QueryEntities()
                .OrWhere("email", ".com", ExpressionType.Contains)
                .Count();

            // Assert
            Assert.Equal(11, count);
        }

        [Fact]
        public void SelectWorks()
        {
            // Arrange
            var crmClient = FakeClientFactory.GetOneLoggedIn(
                new
                {
                    success = true,
                    result = new[]
                    {
                        new
                        {
                            email = "foo@acme.com.cn",
                            id = "4x1234"
                        },
                        new
                        {
                            email = "bar @example.org",
                            id = "4x1235"
                        },
                        new
                        {
                            email = "hello@test.com",
                            id = "4x1236"
                        }
                    }
                }
            );

            // Act
            var contacts = crmClient
                .UseModule("Contacts")
                .QueryEntities()
                .OrWhere("email", ".com", ExpressionType.Contains)
                .Select<Contact>("email");

            // Assert
            Assert.True(contacts.Any());
        }

        private class Contact
        {
            [JsonProperty("id")] public string ID { get; set; }

            [JsonProperty("email")] public string Email { get; set; }
        }
    }
}
