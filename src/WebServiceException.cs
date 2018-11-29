using System;

namespace Salaros.Vtiger.WebService
{
    public sealed class WebServiceException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebServiceException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="code">The code.</param>
        public WebServiceException(string message, string code = "UNKOWN", Exception ex = null) 
            : base(message, ex)
        {
            Code = code;
        }

        /// <summary>Gets the error code.</summary>
        /// <value>The error code.</value>
        public string Code { get; private set; }
    }
}
