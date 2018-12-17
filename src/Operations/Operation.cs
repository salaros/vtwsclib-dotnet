using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Salaros.vTiger.WebService
{
    public class Operation : OperationBase
    {
        internal Operation(Client client, string operationName)
            : base(client, operationName)
        {
        }

        public Operation WithData(string key, string value)
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

        public Operation WithData(Dictionary<string, string> operationData)
        {
            if (operationData == null)
                throw new ArgumentNullException(nameof(operationData));

            this.operationData = new[] { this.operationData, operationData }
                                    .SelectMany(dict => dict)
                                    .ToLookup(pair => pair.Key, pair => pair.Value)
                                    .ToDictionary(group => group.Key, group => group.First());
            return this;
        }

        public Operation WithData<TEntity>(TEntity entity, JsonSerializerSettings jsonSettings = null)
            where TEntity : class
        {
            var entityData = JObject.FromObject(entity, JsonSerializer.Create(jsonSettings ?? new JsonSerializerSettings()))
                                    .ToObject<Dictionary<string, object>>()
                                    .ToDictionary(i => i.Key, i => i.Value?.ToString());
            return WithData(entityData as Dictionary<string, string>);
        }

        public virtual TResult Send<TResult>(JsonSerializerSettings jsonSettings = null)
            where TResult : class
        {
            return client?.SendRequest<TResult>(operationData, HttpMethod.Get, jsonSettings);
        }

        public virtual JToken Send(JsonSerializerSettings jsonSettings = null)
        {
            return Send<JToken>(jsonSettings);
        }

        public virtual TResult SendAsPost<TResult>(JsonSerializerSettings jsonSettings = null)
            where TResult : class
        {
            return client?.SendRequest<TResult>(operationData, HttpMethod.Post, jsonSettings);
        }

        public virtual JToken SendAsPost(JsonSerializerSettings jsonSettings = null)
        {
            return SendAsPost<JToken>(jsonSettings);
        }
    }
}