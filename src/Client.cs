using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace Salaros.vTiger.WebService
{
    public class Client
    {
        protected string sessionName;

        protected HttpClient httpClient;

        /// <summary>Initializes a new instance of the <see cref="Client"/> class.</summary>
        /// <param name="options">The options.</param>
        public Client(ClientOptions options)
            : this(options, new HttpClient())
        {}

        /// <summary>Initializes a new instance of the <see cref="Client"/> class.</summary>
        /// <param name="uri">The URI.</param>
        /// <param name="httpClient">The HTTP client.</param>
        public Client(Uri uri, HttpClient httpClient = null) : this(new ClientOptions { BaseUrl = uri }, httpClient ?? new HttpClient())
        {}

        /// <summary>Initializes a new instance of the <see cref="Client"/> class.</summary>
        /// <param name="uri">The URI.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="accessKey">The access key.</param>
        /// <param name="httpClient">The HTTP client.</param>
        public Client(string uri, string userName, string accessKey, HttpClient httpClient = null)
            : this(new ClientOptions
                {
                    BaseUrl = new Uri(uri),
                    Credentials = new ClientCredentials(userName, accessKey)
                },
                httpClient ?? new HttpClient())
        {}

        /// <summary>
        ///   <para>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// </para>
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="httpClient">The HTTP client.</param>
        /// <exception cref="ArgumentNullException">options
        /// or
        /// HttpClient</exception>
        public Client(ClientOptions options, HttpClient httpClient = null)
        {
            if (options?.BaseUrl is null)
                throw new ArgumentNullException(nameof(options));

            Options = options;

            // Set HTTP client up
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            // ReSharper disable once PossibleNullReferenceException
            this.httpClient.BaseAddress = Options.FullUrl;
            this.httpClient.Timeout = TimeSpan.FromSeconds(Options.RequestTimeout);

            ServiceInfo = new WebServiceInfo();
        }

        /// <summary>Gets the service information.</summary>
        /// <value>The service information.</value>
        public WebServiceInfo ServiceInfo { get; internal set; }

        /// <summary>Gets the options.</summary>
        /// <value>The options.</value>
        public ClientOptions Options { get; }

        /// <summary>Gets or sets the HTTP client.</summary>
        /// <value>The HTTP client.</value>
        internal HttpClient HttpClient
        {
            get => httpClient;
            set
            {
                httpClient = value;
                httpClient.BaseAddress = Options?.BaseUrl;
            }
        }

        /// <summary>Gets the current user.</summary>
        /// <value>The current user.</value>
        public ClientUser CurrentUser { get; internal set; }

        /// <summary>Uses the module.</summary>
        /// <param name="moduleName">Name of the module.</param>
        /// <returns></returns>
        public ModuleOperation UseModule(string moduleName) => new ModuleOperation(this, moduleName);

        /// <summary>Invokes the operation.</summary>
        /// <param name="operationName">Name of the operation.</param>
        /// <returns></returns>
        public Operation InvokeOperation(string operationName) => new Operation(this, operationName);

        /// <summary>Retrieves the specified identifier typed.</summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="idTyped">The identifier typed.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Must be a non-empty string. - idTyped</exception>
        public Operation Retrieve<TResult>(string idTyped)
        {
            if (string.IsNullOrWhiteSpace(idTyped))
                throw new ArgumentException("Must be a non-empty string.", nameof(idTyped));

            return InvokeOperation("retrieve")
                .WithData("id", idTyped);
        }

        /// <summary>Passes the challenge.</summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Must be a non-empty string. - userName</exception>
        internal bool PassChallenge(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("Must be a non-empty string.", nameof(userName));

            var responseToken = InvokeOperation("getchallenge")
                .WithData("username", userName)
                .Send();

            ServiceInfo.TokenExpiration = DateTimeOffset.FromUnixTimeSeconds(responseToken?.Value<long>("expireTime") ?? 0);
            ServiceInfo.Token = responseToken?.Value<string>("token");

            return !string.IsNullOrWhiteSpace(ServiceInfo.Token) && ServiceInfo.TokenExpiration.Ticks > 0;
        }

        /// <summary>Logins this instance.</summary>
        /// <exception cref="NotImplementedException"></exception>
        internal bool Login()
        {
            return Login(Options.Credentials);
        }

        /// <summary>Logins the specified user name.</summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="accessKey">The access key.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Must be a non-empty string. - userName</exception>
        public bool Login(string userName, string accessKey)
        {
            Options.Credentials = new ClientCredentials(userName, accessKey);
            return Login(Options.Credentials);
        }

        /// <summary>Logins the specified credentials.</summary>
        /// <param name="credentials">The credentials.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">credentials</exception>
        public bool Login(ClientCredentials credentials)
        {
            if (credentials is null)
                throw new ArgumentNullException(nameof(credentials));

            if (string.IsNullOrWhiteSpace(credentials.UserName))
                throw new ArgumentException("Must be a non-empty string.", nameof(credentials.UserName));

            // Pass the challenge before logging in
            if (!PassChallenge(credentials.UserName))
                return false;

            return LoginAfterChallenge(credentials);
        }

        /// <summary>Logins the after challenge.</summary>
        /// <param name="credentials">The credentials.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Must be a non-empty string. - UserName
        /// or
        /// Must be a non-empty string. - AccessKey</exception>
        /// <exception cref="WebServiceException">Failed to log in</exception>
        internal bool LoginAfterChallenge(ClientCredentials credentials)
        {
            if (string.IsNullOrWhiteSpace(credentials.UserName))
                throw new ArgumentException("Must be a non-empty string.", nameof(credentials.UserName));

            if (string.IsNullOrWhiteSpace(credentials.AccessKey))
                throw new ArgumentException("Must be a non-empty string.", nameof(credentials.AccessKey));

            // Concatenate service token and access key
            // and compute MD5 of that string as per vTiger specs
            var saltedAccessKey = MD5.Create()
                .ComputeHash(Encoding.ASCII.GetBytes($"{ServiceInfo.Token}{credentials.AccessKey}"))
                .Select(b => b.ToString("X2"))
                .Aggregate((s, c) => s + c)
                .ToLowerInvariant();

            var loginToken = InvokeOperation("login")
                .WithData(new Dictionary<string, string>
                {
                    { "username", credentials.UserName },
                    { "accessKey", saltedAccessKey }
                })
                .SendAsPost() ?? throw new WebServiceException("Failed to log in");

            // Setting session name, so it can be used for subsequent calls
            sessionName = loginToken.Value<string>("sessionName");

            // Backing up logged in user credentials
            CurrentUser = new ClientUser(
                loginToken?.Value<string>("userId"),
                credentials.UserName,
                credentials.AccessKey
            );

            // vTiger CRM and WebServices API version
            ServiceInfo.ApiVersion = new Version(loginToken.Value<string>("version"));
            ServiceInfo.CrmVersion = new Version(loginToken.Value<string>("vtigerVersion"));

            return !string.IsNullOrWhiteSpace(CurrentUser?.Id);
        }

        /// <summary>Sends HTTP request to CRM.</summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="requestData">The request data.</param>
        /// <param name="method">The method.</param>
        /// <param name="jsonSettings">The json settings.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Please specify a valid {nameof(operation)} - requestData</exception>
        /// <exception cref="WebServiceException">
        /// Server has replied with the following status code: {result.StatusCode}
        /// or
        /// Unsupported request type {method}
        /// or
        /// Failed to execute {method} request on the following URL: 'CRM URL HERE' - FAILED_SENDING_REQUEST
        /// or
        /// UNKNOWN_ERROR
        /// or
        /// or
        /// Failed to parse the following responses: {result} - PARSING_JSON_RESPONSE
        /// or
        /// Failed to parse the following vTiger response: 'RAW JSON HERE'
        /// </exception>
        /// <exception cref="Uri"></exception>
        /// <exception cref="InvalidOperationException">Server replied with an empty string, which is a clear sign of an error!</exception>
        internal TResult SendRequest<TResult>(IDictionary<string, string> requestData, HttpMethod method = null, JsonSerializerSettings jsonSettings = null)
            where TResult : class
        {
            if (!requestData.TryGetValue("operation", out string operation) || string.IsNullOrWhiteSpace(operation))
                throw new ArgumentException($"Please specify a valid {nameof(operation)}", nameof(requestData));

            if (null == method)
                method = HttpMethod.Post;

            // Perform re-login if required (e.g. service token has expired)
            // Please note: the only time login is not called is when API challenge occurs
            if (!"getchallenge".Equals(operation) && DateTime.UtcNow.Ticks > ServiceInfo.TokenExpiration.Ticks)
                Login(Options.Credentials);

            // Inject session name into data sent to vTiger
            requestData["sessionName"] = sessionName;

            string jsonRaw;
            var requestUrl = $"?operation={operation}";
            try
            {
                switch (method)
                {
                    case var get when HttpMethod.Get.Equals(get):
                        var query = string.Join("&", requestData.ToList()
                                .Select(i => $"{i.Key}={Uri.EscapeUriString(i.Value ?? string.Empty)}"));
                        requestUrl = $"?{query}";
                        jsonRaw = httpClient.GetStringAsync(requestUrl)?.Result;
                        break;

                    case var post when HttpMethod.Post.Equals(post):
                        var result = httpClient.PostAsync(requestUrl, new FormUrlEncodedContent(requestData.ToList()))?.Result;
                        if ((int)result.StatusCode > 399)
                            throw new WebServiceException($"Server has replied with the following status code: {result.StatusCode}");

                        jsonRaw = result.Content.ReadAsStringAsync()?.Result;
                        break;

                    default:
                        throw new WebServiceException($"Unsupported request type {method}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new WebServiceException(
                    $"Failed to execute {method} request on the following URL: '{new Uri(httpClient.BaseAddress, requestUrl)}'. {ex.Message}",
                    "FAILED_SENDING_REQUEST",
                    ex
                );
            }

            if (string.IsNullOrWhiteSpace(jsonRaw))
                throw new InvalidOperationException("Server replied with an empty string, which is a clear sign of an error!");

            JObject responseObject = null;

            try
            {
                responseObject = JObject.Parse(jsonRaw);
            }
            catch (JsonReaderException ex)
            {
                throw new WebServiceException(jsonRaw, "UNKNOWN_ERROR", ex);
            }

            // Parse the raw JSON response
            switch (responseObject)
            {
                // Parse error object
                case var errorObj when errorObj.TryGetValue("error", StringComparison.OrdinalIgnoreCase, out JToken errorToken):
                    throw new WebServiceException(errorToken?.Value<string>("message"), errorToken?.Value<string>("code"));

                // Parse success / result object
                case var successObj when bool.TryParse(successObj?["success"]?.ToString(), out bool isSuccess) && isSuccess:
                    var result = successObj?["result"];

                    try
                    {
                        switch (typeof(TResult))
                        {
                            // Cast to JToken
                            case Type jtoken when jtoken == typeof(JToken):
                                return result as TResult;

                            case Type jobject when jobject == typeof(JObject):
                                return result.ToObject<TResult>(JsonSerializer.Create(jsonSettings ?? new JsonSerializerSettings()));

                            // Try to re-parse JSON to the given type
                            default:
                                return JsonConvert.DeserializeObject<TResult>(result?.ToString(), jsonSettings ?? new JsonSerializerSettings());
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new WebServiceException($"Failed to parse the following responses: {result}", "PARSING_JSON_RESPONSE", ex);
                    }

                default:
                    throw new WebServiceException($"Failed to parse the following vTiger response: '{jsonRaw}'");
            }
        }
    }
}
