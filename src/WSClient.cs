using System;
using System.Threading.Tasks;

namespace Salaros.Vtiger.VTWSCLib
{
    public class WSClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WSClient" /> class.
        /// </summary>
        /// <param name="crmUrl">The URL of the remote CRM server.</param>
        /// <param name="userName">User name.</param>
        /// <param name="secret">Access key token (shown on user's profile page) or password, depends on <paramref name="authMode" />.</param>
        /// <param name="authMode">The authentication mode, defaults to <paramref name="userName" /> + access key</param>
        /// <param name="relativeUrl">The relative URL.</param>
        /// <param name="requestTimeout">Optional request timeout in seconds.</param>
        /// <exception cref="WSException">Unknown login mode: <paramref name="authMode" />
        /// or
        /// Failed to log into vTiger CRM (User: <paramref name="userName" />, URL: <paramref name="vtigerUrl" />)</exception>
        public WSClient(Uri crmUrl, string userName, string secret, AuthMode authMode = AuthMode.AccessKey, string relativeUrl = "webservice.php", int requestTimeout = 0)
        {
            Modules = new Modules(this);
            Entities = new Entities(this);
            Session = new Session(this);

            RequestTimeout = requestTimeout;

            WebServiceUrl = new Uri(crmUrl, relativeUrl);

            Task<bool> loginTask;
            switch (authMode) {
                case AuthMode.AccessKey:
                    loginTask = Session.Login(userName, secret);
                    break;

                case AuthMode.Password:
                    loginTask = Session.LoginPassword(userName, secret);
                    break;

                default:
                    throw new WSException($"Unknown login mode: '{authMode}'");
            }

            loginTask.Wait();
            if (!loginTask.Result)
                throw new WSException($"Failed to log into vTiger CRM (User: '{userName}', URL: {crmUrl})");
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
