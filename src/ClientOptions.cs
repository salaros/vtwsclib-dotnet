using System;

namespace Salaros.vTiger.WebService
{
    public class ClientOptions
    {
        /// <summary>Initializes a new instance of the <see cref="ClientOptions"/> class.</summary>
        public ClientOptions() { }

        /// <summary>Initializes a new instance of the <see cref="ClientOptions"/> class.</summary>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="accessKey">The access key.</param>
        /// <exception cref="ArgumentNullException">baseUrl</exception>
        public ClientOptions(Uri baseUrl, string userName, string accessKey)
        {
            BaseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
            Credentials = new ClientCredentials(userName, accessKey);
        }

        /// <summary>Initializes a new instance of the <see cref="ClientOptions"/> class.</summary>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="accessKey">The access key.</param>
        public ClientOptions(string baseUrl, string userName, string accessKey)
            : this(new Uri(baseUrl), userName, accessKey)
        {}

        public ClientCredentials Credentials { get; set; }

        public Uri BaseUrl { get; set; }

        public string WebservicePath { get; set; } = "webservice.php";

        internal Uri FullUrl => new Uri(BaseUrl, WebservicePath);

        public int RequestTimeout { get; set; } = 30;
    }
}
