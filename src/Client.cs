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
        protected HttpClient httpClient;

        protected string sessionName;

        public Client(Uri baseUrl)
        {
            BaseUrl = baseUrl;
            ServiceInfo = new WebServiceInfo();
        }

        public WebServiceInfo ServiceInfo { get; }

        public ClientUser CurrentUser { get; private set; }

        public Uri BaseUrl { get; }

        internal Uri FullUrl => new Uri(BaseUrl, ServiceInfo.WebservicePath);

        public HttpClient HttpClient
        {
            get
            {
                return httpClient ?? (httpClient = new HttpClient
                {
                    BaseAddress = FullUrl,
                    Timeout = TimeSpan.FromSeconds(RequestTimeout)
                });
            }
            set
            {
                httpClient = value;
                httpClient.BaseAddress = FullUrl;
                httpClient.Timeout = TimeSpan.FromSeconds(RequestTimeout);
            }
        }

        public int RequestTimeout { get; set; } = 30;

        public ModuleOperation UseModule(string moduleName) => new ModuleOperation(this, moduleName);

        public Operation InvokeOperation(string operationName) => new Operation(this, operationName);

        public Operation Retrieve<TResult>(string idTyped)
        {
            if (string.IsNullOrWhiteSpace(idTyped))
                throw new ArgumentException("Must be a non-empty string.", nameof(idTyped));

            return InvokeOperation("retrieve")
                .SetData("id", idTyped);
        }

        internal bool PassChallenge(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("Must be a non-empty string.", nameof(userName));

            var responseToken = InvokeOperation("getchallenge")
                .SetData("username", userName)
                .Execute<JToken>(HttpMethod.Get);

            ServiceInfo.TokenExpiration = DateTimeOffset.FromUnixTimeSeconds(responseToken?.Value<long>("expireTime") ?? 0);
            ServiceInfo.Token = responseToken?.Value<string>("token");

            return !string.IsNullOrWhiteSpace(ServiceInfo.Token) && ServiceInfo.TokenExpiration.Ticks > 0;
        }

        public bool Login(string userName, string accessKey)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("Must be a non-empty string.", nameof(userName));

            // Pass the challenge before logging in
            if (!PassChallenge(userName))
                return false;

            return LoginAfterChallange(userName, accessKey);
        }

        internal bool LoginAfterChallange(string userName, string accessKey)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("Must be a non-empty string.", nameof(userName));

            if (string.IsNullOrWhiteSpace(accessKey))
                throw new ArgumentException("Must be a non-empty string.", nameof(accessKey));

            // Concatenate service token and access key
            // and compute MD5 of that string as per vTiger specs
            var saltedAccessKey = MD5.Create()
                .ComputeHash(Encoding.ASCII.GetBytes($"{ServiceInfo.Token}{accessKey}"))
                .Select(b => b.ToString("X2"))
                .Aggregate((s, c) => s + c)
                .ToLowerInvariant();

            var loginToken = InvokeOperation("login")
                .SetData(new Dictionary<string, string>
                {
                    { "username", userName },
                    { "accessKey", saltedAccessKey }
                })
                .Execute<JToken>() ?? throw new WebServiceException("Failed to log in");

            // Setting session name, so it can be used for subsequent calls
            sessionName = loginToken.Value<string>("sessionName");

            // Backing up logged in user credentials
            CurrentUser = new ClientUser
            {
                Id = loginToken?.Value<string>("userId"),
                UserName = userName,
                AccessKey = accessKey,
            };

            // vTiger CRM and WebServices API version
            ServiceInfo.ApiVersion = new Version(loginToken.Value<string>("version"));
            ServiceInfo.CrmVersion = new Version(loginToken.Value<string>("vtigerVersion"));

            return !string.IsNullOrWhiteSpace(CurrentUser?.Id);
        }

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
                Login(CurrentUser?.UserName, CurrentUser?.AccessKey);

            // Inject session name into data sent to vTiger 
            requestData["sessionName"] = sessionName;

            var jsonRaw = string.Empty;
            var requestUrl = $"?operation={operation}";
            try
            {
                switch (method?.Method)
                {
                    case "GET":
                        var query = string.Join("&", requestData.ToList()
                                .Select(i => $"{i.Key}={Uri.EscapeUriString(i.Value ?? string.Empty)}"));
                        requestUrl = $"?{query}";
                        jsonRaw = HttpClient.GetStringAsync(requestUrl)?.Result;
                        break;

                    case "POST":
                        var result = HttpClient.PostAsync(requestUrl, new FormUrlEncodedContent(requestData.ToList()))?.Result;
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
                    $"Failed to execute {method} request on the following URL: '{new Uri(HttpClient.BaseAddress, requestUrl)}'. {ex.Message}",
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
                        throw new WebServiceException($"Failed to parse the following resposnse: {result}", "PARSING_JSON_RESPONSE", ex);
                    }

                default:
                    throw new WebServiceException($"Failed to parse the following vTiger response: '{jsonRaw}'");
            }
        }
    }
}
