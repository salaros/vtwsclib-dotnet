using System;

namespace Salaros.vTiger.WebService
{
    public class ModuleOperation
    {
        protected string moduleName;

        protected Client client;

        /// <summary>Initializes a new instance of the <see cref="ModuleOperation"/> class.</summary>
        /// <param name="client">The client.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <exception cref="ArgumentException">Must be a non-empty string. - moduleName</exception>
        internal ModuleOperation(Client client, string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName))
                throw new ArgumentException("Must be a non-empty string.", nameof(moduleName));

            this.client = client;
            this.moduleName = moduleName;
        }

        /// <summary>Describes this instance.</summary>
        /// <returns></returns>
        public ModuleInfo Describe() => client?
            .InvokeOperation("describe")
            .Send<ModuleInfo>();

        /// <summary>Invokes the operation.</summary>
        /// <param name="operationName">Name of the operation.</param>
        /// <returns></returns>
        public Operation InvokeOperation(string operationName)
        {
            var operation = client?.InvokeOperation(operationName);
            operation?.WithData("elementType", moduleName);
            return operation;
        }

        /// <summary>Queries the entities.</summary>
        /// <returns></returns>
        public QueryOperation QueryEntities()
        {
            var operation = new QueryOperation(client, moduleName);
            return operation;
        }
    }
}