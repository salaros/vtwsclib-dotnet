using System;
using System.Collections.Generic;

namespace Salaros.vTiger.WebService
{
    public abstract class OperationBase
    {
        protected string operationName;

        protected IDictionary<string, string> operationData;

        protected Client client;

        internal OperationBase(Client client, string operationName)
        {
            this.client = client;

            if (string.IsNullOrWhiteSpace(operationName))
                throw new ArgumentException("Must be a non-empty string.", nameof(operationName));

            this.operationName = operationName;
            operationData = new Dictionary<string, string>()
            {
                { "operation", operationName }
            };
        }
    }
}
