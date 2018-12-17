using System;

namespace Salaros.vTiger.WebService
{
    public class WebServiceInfo
    {
        internal DateTimeOffset TokenExpiration { get; set; }

        internal string Token { get; set; }

        public string WebservicePath { get; set; } = "webservice.php";

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
