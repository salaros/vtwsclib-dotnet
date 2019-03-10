﻿using System;
using Xunit;

namespace Salaros.vTiger.WebService.Tests
{
    public class QueryTests
    {
        protected Client crmClient;

        public QueryTests()
        {
            crmClient = new Client(new Uri("http://vtiger.local/"));
        }

        [Fact]
        public void ComplexQueryGenerates()
        {
            // Arrange
            var queryOperation = crmClient
                .UseModule("Leads")
                .QueryEntities()
                .Select("id", "firstname", "lastname", "company")
                .WhereIn("leadstatus", new[] { "Cold", "Contacted", "Hot", "Warm" })
                .OrWhere("company", "Ltd", ExpressionType.Contains)
                .OrderByDesc("lead_no")
                .Take(10)
                .Skip(2);

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