using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Salaros.vTiger.WebService
{
    public class Operation
    {
        protected string operationName;

        protected IDictionary<string, string> operationData;

        protected IDictionary<IEnumerable<Condition>, string> operationConditions;

        protected Client client;

        internal Operation(Client client, string operationName)
        {
            this.client = client;

            if (string.IsNullOrWhiteSpace(operationName))
                throw new ArgumentException("Must be a non-empty string.", nameof(operationName));

            this.operationName = operationName;

            operationConditions = new Dictionary<IEnumerable<Condition>, string>();
            operationData = new Dictionary<string, string>()
            {
                { "operation", operationName }
            };
        }

        public Operation Where<TResult>(IEnumerable<Condition> conditions)
        {
            operationConditions.Add(conditions, string.Empty);
            return this;
        }

        public Operation AndWhere<TResult>(IEnumerable<Condition> conditions)
        {
            operationConditions.Add(conditions, "AND");
            return this;
        }

        public Operation OrWhere(IEnumerable<Condition> conditions)
        {
            operationConditions.Add(conditions, "OR");
            return this;
        }

        public Operation SetData(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Must be a non-empty string.", nameof(key));

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Must be a non-empty string.", nameof(value));

            if (operationData.ContainsKey(key))
                operationData[key] = value;
            else
                operationData.Add(key, value);

            return this;
        }

        public Operation SetData(Dictionary<string, string> operationData)
        {
            if (operationData == null)
                throw new ArgumentNullException(nameof(operationData));

            this.operationData = new[] { this.operationData, operationData }
                                    .SelectMany(dict => dict)
                                    .ToLookup(pair => pair.Key, pair => pair.Value)
                                    .ToDictionary(group => group.Key, group => group.First());
            return this;
        }

        public Operation SetData<TEntity>(TEntity entity, JsonSerializerSettings jsonSettings = null)
            where TEntity : class
        {
            var entityData = JObject.FromObject(entity, JsonSerializer.Create(jsonSettings ?? new JsonSerializerSettings()))
                                    .ToObject<Dictionary<string, object>>()
                                    .ToDictionary(i => i.Key, i => i.Value?.ToString());
            return SetData(entityData as Dictionary<string, string>);
        }

        public TResult Execute<TResult>(HttpMethod method = null, JsonSerializerSettings jsonSettings = null)
            where TResult : class
        {
            return client.SendRequest<TResult>(operationData, method, jsonSettings);
        }
    }
}