using System;
using Xunit;

namespace Salaros.vTiger.WebService.Tests
{
    public class QueryTests
    {
        protected Client crmClient;

        /// <summary>Initializes a new instance of the <see cref="QueryTests"/> class.</summary>
        public QueryTests()
        {
            crmClient = FakeClientFactory.GetOne();
        }

        [Fact]
        public void ComplexQueryIsGenerated()
        {
            // Arrange
            var queryOperation = crmClient
                .UseModule("Leads")
                .QueryEntities()
                .WhereIn("leadstatus", new[] { "Cold", "Contacted", "Hot", "Warm" })
                .OrWhere("company", "Ltd", ExpressionType.Contains)
                .OrderByDesc("lead_no")
                .Take(10)
                .Skip(2)
                .SelectQuery("id", "firstname", "lastname", "company");

            // Act
            var queryString = queryOperation.CompileQuery();

            // Assert
            Assert.Equal(
                "SELECT id, firstname, lastname, company " +
                "FROM Leads " +
                "WHERE leadstatus IN ('Cold', 'Contacted', 'Hot', 'Warm') OR company LIKE '%Ltd%' " +
                "ORDER BY lead_no DESC " +
                "LIMIT 2, 10;",
                queryString
            );
        }
    }
}
