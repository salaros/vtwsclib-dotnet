using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Salaros.Vtiger.VTWSCLib
{
    public class Session
    {
        /// <summary>
        /// The HTTP client
        /// </summary>
        protected HttpClient httpClient;

        /// <summary>
        /// The parent <see cref="WSClient"/> client object
        /// </summary>
        protected WSClient parentClient;

        /// <summary>
        /// Session name and token
        /// </summary>
        protected string name, serviceToken;

        /// <summary>
        /// The service expiration date
        /// </summary>
        protected long serviceExpireTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="Session"/> class.
        /// </summary>
        /// <param name="parentClient">The parent client.</param>
        public Session(WSClient parentClient)
        {
            this.parentClient = parentClient;
        }

        /// <summary>
        /// Gets a challenge token from the server and stores for future requests
        /// </summary>
        /// <param name="userName">VTiger user name</param>
        /// <returns>Returns false in case of failure</returns>
        private async Task<bool> PassChallenge(string userName)
        {
            var challengeData = new Dictionary<string, string> {
                { "operation", "getchallenge" },
                { "username", userName }
            };

            var result = await SendHttpRequest<JToken>(challengeData, WebRequestMethods.Http.Get);
            if (null == result)
                return false;

            serviceExpireTime = DateTimeOffset.FromUnixTimeSeconds(result?.Value<long>("expireTime") ?? 0).Ticks;
            serviceToken = result?.Value<string>("token");

            return string.IsNullOrWhiteSpace(serviceToken) && serviceExpireTime > 0;
        }

        /// <summary>
        /// Sends HTTP request to VTiger web service API endpoint
        /// </summary>
        /// <typeparam name="T">The type of the result. Use <see cref="JToken"/> if you are not sure on type to use.</typeparam>
        /// <param name="requestData">HTTP request data.</param>
        /// <param name="method">HTTP request method (GET, POST etc), use <see cref="WebRequestMethods.Http"/> verbs</param>
        /// <returns>Returns request result object (null in case of failure)</returns>
        internal async Task<T> SendHttpRequest<T>(Dictionary<string, string> requestData, string method = WebRequestMethods.Http.Post)
            where T : class
        {
            if (!requestData.TryGetValue("operation", out string operation) || string.IsNullOrWhiteSpace(operation))
                throw new ArgumentException($"Please specify a valid {nameof(operation)}", nameof(requestData));

            // Perform re-login if required (e.g. service token has expired)
            // Please note: the only time login is not called is when API challenge occurs 
            if (!"getchallenge".Equals(operation) && DateTime.UtcNow.Ticks > serviceExpireTime)
                await Login();

            // Inject session name into data sent to vTiger 
            requestData["sessionName"] = name;

            var jsonRaw = string.Empty;
            var requestUrl = $"?operation={operation}";
            try
            {
                httpClient = new HttpClient
                {
                    BaseAddress = parentClient.WebServiceUrl,
                    Timeout = TimeSpan.FromSeconds((parentClient.RequestTimeout <= 0) ? 30 : parentClient.RequestTimeout),
                };

                switch (method)
                {
                    case WebRequestMethods.Http.Get:
                        var query = string.Join("&", requestData.ToList()
                                .Select(i => $"{i.Key}={Uri.EscapeUriString(i.Value ?? string.Empty)}"));
                        requestUrl = $"?{query}";
                        jsonRaw = await httpClient.GetStringAsync(requestUrl);
                        break;

                    case WebRequestMethods.Http.Post:
                        var result = await httpClient.PostAsync(requestUrl, new FormUrlEncodedContent(requestData.ToList()));
                        jsonRaw = await result.Content.ReadAsStringAsync();
                        break;

                    default:
                        throw new WSException($"Unsupported request type {method}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new WSException(
                    $"Failed to execute {method} request on the following URL: '{new Uri(httpClient.BaseAddress, requestUrl)}'. {ex.Message}",
                    "FAILED_SENDING_REQUEST",
                    ex
                );
            }

            // Parse the response
            switch (JObject.Parse(jsonRaw))
            {
                // Parse error object
                case var errorObj when bool.TryParse(errorObj["error"]?.ToString(), out bool isSuccess) && isSuccess:
                    var error = errorObj["error"];
                    throw new WSException(error?.Value<string>("message"), error?.Value<string>("code"));
                
                // Parse success / result object
                case var successObj when bool.TryParse(successObj["success"]?.ToString(), out bool isSuccess) && isSuccess:
                    var result = successObj["result"];
                    return (typeof(JToken) == typeof(T))
                        ? result as T               // Cast to JToken
                        : result.ToObject<T>();     // Convert to the actual object

                default:
                    throw new WSException($"Failed to parse the following vTiger response: '{jsonRaw}'");
            }
        }

        /// <summary>
        /// Login to the server using username and VTiger access key token
        /// </summary>
        /// <param name="userName">vTiger user name</param>
        /// <param name="accessKey">VTiger access key token (located on the user profile/settings page)</param>
        /// <returns>Returns true if login operation has been successful</returns>
        internal async Task<bool> Login(string userName = null, string accessKey = null)
        {
            userName = userName ?? CurrentUser?.UserName;
            accessKey = accessKey ?? CurrentUser?.SecretKey;

            // Pass the challenge before logging in
            if (await PassChallenge(userName))
                return false;

            // Concatenate service token and access key
            // and compute MD5 of that string as per vTiger specs
            var saltedAccessKey = MD5.Create()
                .ComputeHash(Encoding.ASCII.GetBytes($"{serviceToken}{accessKey}"))
                .Select(b => b.ToString("X2"))
                .Aggregate((s, c) => s + c)
                .ToLowerInvariant();

            var result = await SendHttpRequest<JToken>(
                new Dictionary<string, string> {
                    { "operation", "login" },
                    { "username", userName },
                    { "accessKey", saltedAccessKey }
                }
            ) ?? throw new WSException("Failed to log in");

            if (null == result)
                return false;

            // Setting session name, so it can be used for subsequent calls
            name = result.Value<string>("sessionName");

            // Backing up logged in user credentials
            CurrentUser = new UserInfo
            {
                Id = result?.Value<string>("userId"),
                UserName = userName,
                SecretKey = accessKey,
            };

            // vTiger CRM and WebServices API version
            ApiVersion = new Version(result.Value<string>("version"));
            CrmVersion = new Version(result.Value<string>("vtigerVersion"));

            return !string.IsNullOrWhiteSpace(CurrentUser?.Id);
        }

        /// <summary>
        /// Allows you to login using user name and password instead of access key (works on some VTige forks)
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">vTiger password, DO NOT use access key from user profile/settings page</param>
        /// <returns>Returns true if login operation has been successful</returns>
        internal async Task<bool> LoginPassword(string userName = null, string password = null)
        {
            userName = userName ?? CurrentUser?.UserName;
            password = password ?? CurrentUser?.SecretKey;

            // Pass the challenge before logging in
            if (await PassChallenge(userName))
                return false;

            var result = await SendHttpRequest<JToken>(
                new Dictionary<string, string> {
                    { "operation", "login_pwd" },
                    { "username", userName },
                    { "password", password },
                }
            ) ?? throw new WSException("Failed to log in");

            // Extract access key from user + password login
            var accessKey = result?.FirstOrDefault()?.Value<string>() ?? result?.Value<string>("accesskey");
            return await Login(userName, accessKey);
        }

        /// <summary>
        /// Gets vTiger CRM and WebServices API version
        /// </summary>
        /// <value>
        /// vTiger CRM and WebServices API version
        /// </value>
        public Version ApiVersion { get; private set; } = new Version("0.0");

        /// <summary>
        /// Gets the version of vTiger CRM (or its fork) currently being used.
        /// </summary>
        /// <value>
        /// The v tiger version.
        /// </value>
        public Version CrmVersion { get; private set; } = new Version("0.0");

        /// <summary>
        /// Gets an array containing the basic information about current API user
        /// </summary>
        /// <value>
        /// Basic information about current API user
        /// </value>
        public UserInfo CurrentUser { get; private set; }
    }
}
