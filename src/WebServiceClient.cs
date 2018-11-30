using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Salaros.Vtiger.WebService
{
    public class WebServiceClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebServiceClient" /> class.
        /// </summary>
        /// <param name="crmUrl">The URL of the remote CRM server.</param>
        /// <param name="userName">User name.</param>
        /// <param name="secret">Access key token (shown on user's profile page) or password, depends on <paramref name="authMode" />.</param>
        /// <param name="authMode">The authentication mode, defaults to <paramref name="userName" /> + access key</param>
        /// <param name="relativeUrl">The relative URL.</param>
        /// <param name="requestTimeout">Optional request timeout in seconds.</param>
        /// <exception cref="WebServiceException">Unknown login mode: <paramref name="authMode" />
        /// or
        /// Failed to log into vTiger CRM (User: <paramref name="userName" />, URL: <paramref name="vtigerUrl" />)</exception>
        public WebServiceClient(Uri crmUrl, string userName, string secret, AuthMode authMode = AuthMode.AccessKey, string relativeUrl = "webservice.php", int requestTimeout = 0)
        {
            Modules = new Modules(this);
            Entities = new Entities(this);
            Session = new Session(this);

            RequestTimeout = requestTimeout;
            WebServiceUrl = new Uri(crmUrl, relativeUrl);

            if (!Login(userName, secret, authMode))
                throw new WebServiceException($"Failed to log into vTiger CRM (User: '{userName}', URL: {crmUrl})");
        }

        /// <summary>
        /// Logins the asynchronous.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="secret">The secret.</param>
        /// <param name="authMode">The authentication mode.</param>
        /// <returns></returns>
        /// <exception cref="WebServiceException">Unknown login mode: '{authMode}</exception>
        internal async Task<bool> LoginAsync(string userName, string secret, AuthMode authMode = AuthMode.AccessKey)
        {
            switch (authMode)
            {
                case AuthMode.AccessKey:
                    return await Session.Login(userName, secret);

                case AuthMode.Password:
                    return await Session.LoginPassword(userName, secret);

                default:
                    throw new WebServiceException($"Unknown login mode: '{authMode}'");
            }
        }

        /// <summary>
        /// Logins the specified user name.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="secret">The secret.</param>
        /// <param name="authMode">The authentication mode.</param>
        /// <returns></returns>
        internal bool Login(string userName, string secret, AuthMode authMode = AuthMode.AccessKey)
        {
            var loginTask = LoginAsync(userName, secret, authMode);
            loginTask.Wait();
            return loginTask?.Result ?? false;
        }

        /// <summary>
        /// Executes the query asynchronous.
        /// </summary>
        /// <typeparam name="TRes">The type of the resource.</typeparam>
        /// <param name="query">The query string / expression.</param>
        /// <param name="jsonSettings">The JSON serialization settings.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">query</exception>
        /// <exception cref="ArgumentException">query</exception>
        public async Task<TRes> ExecuteQueryAsync<TRes>(string query, JsonSerializerSettings jsonSettings)
            where TRes : class
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException(nameof(query));

            return await InvokeOperationAsync<TRes>(
                "query", 
                new Dictionary<string, string> { { "query", query } }, 
                HttpMethod.Get,
                jsonSettings
            );
        }

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <typeparam name="TRes">The type of the resource.</typeparam>
        /// <param name="query">The query string / expression.</param>
        /// <param name="jsonSettings">The JSON serialization settings.</param>
        /// <returns></returns>
        public TRes ExecuteQuery<TRes>(string query, JsonSerializerSettings jsonSettings = null)
            where TRes : class
        {
            var queryTask = ExecuteQueryAsync<TRes>(query, jsonSettings);
            queryTask.Wait();
            return queryTask?.Result;
        }

        /// <summary>
        /// Invokes the operation.
        /// </summary>
        /// <typeparam name="TRes">The type of the resource.</typeparam>
        /// <param name="operation">The operation.</param>
        /// <param name="operationData">The dictionary.</param>
        /// <param name="method">The method.</param>
        /// <param name="jsonSettings">The JSON serialization settings.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<TRes> InvokeOperationAsync<TRes>(string operation, Dictionary<string, string> operationData, HttpMethod method, JsonSerializerSettings jsonSettings = null)
            where TRes : class
        {
            operationData["operation"] = operation;
            return await Session.SendHttpRequestAsync<TRes>(operationData, method, jsonSettings);
        }

        /// <summary>
        /// Invokes the operation.
        /// </summary>
        /// <typeparam name="TRes">The type of the resource.</typeparam>
        /// <param name="operation">The operation.</param>
        /// <param name="operationData">The operation data.</param>
        /// <param name="method">The method.</param>
        /// <param name="jsonSettings">The JSON serialization settings.</param>
        /// <returns></returns>
        public TRes InvokeOperation<TRes>(string operation, Dictionary<string, string> operationData, HttpMethod method, JsonSerializerSettings jsonSettings = null)
            where TRes : class
        {
            var invokeTask = InvokeOperationAsync<TRes>(operation, operationData, method, jsonSettings);
            invokeTask.Wait();
            return invokeTask?.Result;
        }

        public Uri WebServiceUrl { get; internal set; }

        /// <summary>
        /// Gets the session.
        /// </summary>
        /// <value>
        /// The session.
        /// </value>
        internal Session Session { get; }

        /// <summary>
        /// Gets modules.
        /// </summary>
        /// <value>
        /// The modules.
        /// </value>
        public Modules Modules { get; }

        /// <summary>
        /// Gets entities.
        /// </summary>
        /// <value>
        /// The entities.
        /// </value>
        public Entities Entities { get; }

        /// <summary>
        /// Gets request timeout in seconds.
        /// </summary>
        /// <value>
        /// Request timeout in seconds.
        /// </value>
        internal int RequestTimeout { get; }

        /// <summary>
        /// Authentication mode
        /// </summary>
        public enum AuthMode
        {
            AccessKey = 0x0,
            Password = 0x1
        }
    }
}
