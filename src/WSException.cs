using System;

namespace Salaros.Vtiger.VTWSCLib
{
    public sealed class WSException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WSException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="code">The code.</param>
        public WSException(string message, string code = "UNKOWN", Exception ex = null) 
            : base(message, ex)
        {
            Code = code;
        }

        /// <summary>Gets the error code.</summary>
        /// <value>The error code.</value>
        public string Code { get; private set; }
    }
}
