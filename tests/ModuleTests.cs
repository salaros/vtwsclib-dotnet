using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;

namespace Salaros.vTiger.WebService.Tests
{
    public class ModuleTests
    {
        [Fact]
        public void GetModules()
        {
            // Arrange
            var crmClient = FakeClientFactory.GetOneAfterChallenge(new
            {
                success = true,
                result = new
                {
                    types = new[]
                    {
                        "module_name"
                    },
                    information = new
                    {
                        module_name = new
                        {
                            isEntity = true,
                            label = "string",
                            singular = "string"
                        }
                    }
                }
            });

            // Act
            var moduleNames = crmClient.GetModules();

            // Assert
            Assert.Equal(new[] {"module_name"}, moduleNames);
        }


        [Fact]
        public void GetModule()
        {
            // Arrange
            var responseObj = JsonConvert.DeserializeObject(Properties.Resources.MODULE_DESCRIBE_JSON);
            var crmClient = FakeClientFactory.GetOneAfterChallenge(responseObj);

            // Act
            var moduleInfo = crmClient.UseModule("test").Describe();

            // Assert
            Assert.NotNull(moduleInfo);
            Assert.Equal("Contacts", moduleInfo.Name);
            Assert.Equal(49, moduleInfo.Fields?.Count ?? 0);
        }
    }
}