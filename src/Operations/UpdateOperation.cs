using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Newtonsoft.Json;

namespace Salaros.vTiger.WebService
{
    public class UpdateOperation : Operation
    {
        /// <summary>Initializes a new instance of the <see cref="UpdateOperation"/> class.</summary>
        /// <param name="client">The client.</param>
        /// <param name="moduleName">Name of the module.</param>
        internal UpdateOperation(Client client, string moduleName)
            : base(client, "update")
        {
        }

        /// <summary>Updates the specified entity data.</summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="entityData">The entity data.</param>
        /// <param name="jsonSettings">The json settings.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">entityData</exception>
        /// <exception cref="ArgumentException">Value cannot be an empty collection. - entityData</exception>
        public TResult Update<TResult>(IDictionary<string, string> entityData, JsonSerializerSettings jsonSettings = null)
            where TResult : class
        {
            if (entityData == null)
                throw new ArgumentNullException(nameof(entityData));

            if (entityData.Count == 0)
                throw new ArgumentException("Value cannot be an empty collection.", nameof(entityData));

            WithData(entityData);
            return client.SendRequest<TResult>(operationData, HttpMethod.Post, jsonSettings);
        }

        /// <summary>Updates the specified entity.</summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="jsonSettings">The json settings.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">entity</exception>
        public TResult Update<TResult>(TResult entity, JsonSerializerSettings jsonSettings = null)
            where TResult : class
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var entityFields = entity
                .GetType()
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(entity)?.ToString());

            var entityProps = entity
                .GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(entity, null)?.ToString());

            var entityData = new[] { entityFields, entityProps }
                .SelectMany(dict => dict)
                .ToLookup(pair => pair.Key, pair => pair.Value)
                .ToDictionary(group => group.Key, group => group.First());

            return Update<TResult>(entityData, jsonSettings);
        }
    }
}
