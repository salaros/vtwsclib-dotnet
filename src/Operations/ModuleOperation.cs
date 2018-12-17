using System;

namespace Salaros.vTiger.WebService
{
    public class ModuleOperation
    {
        protected string moduleName;

        protected Client client;

        internal ModuleOperation(Client client, string moduleName)
            : this(moduleName)
        {
            this.client = client;
        }

        public ModuleOperation(string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName))
                throw new ArgumentException("Must be a non-empty string.", nameof(moduleName));

            this.moduleName = moduleName;
        }

        public ModuleInfo Describe() => client.InvokeOperation("describe").Execute<ModuleInfo>();

        public Operation InvoveOperation(string operationName)
        {
            var operation = client.InvokeOperation(operationName);
            operation.SetData("elementType", moduleName);
            return operation;
        }
    }
}