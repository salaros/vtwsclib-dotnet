using System;

namespace Salaros.vTiger.WebService
{
    public class WebServiceInfo
    {
        /// <summary>Gets or sets the token expiration.</summary>
        /// <value>The token expiration.</value>
        internal DateTimeOffset TokenExpiration { get; set; }

        /// <summary>Gets or sets the token.</summary>
        /// <value>The token.</value>
        internal string Token { get; set; }

        /// <summary>
        /// Gets vTiger CRM and WebServices API version
        /// </summary>
        /// <value>
        /// vTiger CRM and WebServices API version
        /// </value>
        public Version ApiVersion { get; internal set; } = new Version("0.0");

        /// <summary>
        /// Gets the version of vTiger CRM (or its fork) currently being used.
        /// </summary>
        /// <value>
        /// The v tiger version.
        /// </value>
        public Version CrmVersion { get; internal set; } = new Version("0.0");
    }
}
