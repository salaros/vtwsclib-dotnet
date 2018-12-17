using System;

namespace Salaros.vTiger.WebService
{
    public class ModuleOperation
    {
        protected string moduleName;

        protected Client client;

        internal ModuleOperation(Client client, string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName))
                throw new ArgumentException("Must be a non-empty string.", nameof(moduleName));

            this.client = client;
            this.moduleName = moduleName;
        }

        public ModuleInfo Describe() => client?
            .InvokeOperation("describe")
            .Send<ModuleInfo>();

        public Operation InvoveOperation(string operationName)
        {
            var operation = client?.InvokeOperation(operationName);
            operation.WithData("elementType", moduleName);
            return operation;
        }

        public QueryOperation QueryEntities()
        {
            var operation = new QueryOperation(client, moduleName);
            return operation;
        }
    }
}