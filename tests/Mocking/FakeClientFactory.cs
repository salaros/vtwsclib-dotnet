using System;

namespace Salaros.vTiger.WebService.Tests
{
    internal static class FakeClientFactory
    {
        public static Client GetOne()
        {
            return new Client(new Uri("http://vtiger.local/"));
        }

        public static Client GetOne<TResponse>(TResponse response) where TResponse : class
        {
            var httpClient = FakeHttpMessageHandler.GetHttpClient(response);
            return new Client(new Uri("http://vtiger.local/"), httpClient);
        }

        public static Client GetOneAfterChallenge<TResponse>(TResponse response) where TResponse : class
        {
            var client = GetOne(response);
            client.ServiceInfo = new WebServiceInfo()
            {
                Token = "000000000000000000001",
                CrmVersion = new Version(7, 1),
                ApiVersion = new Version(0, 2, 2),
                TokenExpiration = DateTimeOffset.UtcNow.AddMinutes(5)
            };
            return client;
        }

        public static Client GetOneLoggedIn<TResponse>(TResponse response) where TResponse : class
        {
            var client = GetOneAfterChallenge(response);
            client.CurrentUser = new ClientUser("19x1", "admin", "$3çr3t");
            return client;
        }
    }
}
