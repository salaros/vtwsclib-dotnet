using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Salaros.Vtiger.WebService
{
    public class Entities
    {
        /// <summary>
        /// The parent <see cref="WebServiceClient"/> client object
        /// </summary>
        protected WebServiceClient parentClient;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Entities"/> class.
        /// </summary>
        /// <param name="parentClient">The parent client.</param>
        public Entities(WebServiceClient parentClient)
        {
            this.parentClient = parentClient;
        }

        #endregion Constructor

        #region Find / retrieve ONE

        /// <summary>
        /// Retrieves an entity by identifier asynchronously.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="numericId">The entity identifier.</param>
        /// <param name="select">The select.</param>
        /// <param name="jsonSettings">The JSON serialization settings.</param>
        /// <returns></returns>
        public async Task<TEntity> FindOneByIdAsync<TEntity>(string moduleName, long numericId, IList<string> select = null, JsonSerializerSettings jsonSettings = null)
            where TEntity : class
        {
            var entityTypedId = await parentClient.Modules.GetTypedIdAsync(moduleName, numericId, jsonSettings);
            return await FindOneByIdAsync<TEntity>(moduleName, entityTypedId, select, jsonSettings);
        }

        /// <summary>
        /// Retrieves an entity by identifier asynchronously.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="moduleName">The name of the module / entity type.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="select">The list of fields to select (defaults to SQL-like '*' - all the fields).</param>
        /// <param name="jsonSettings">The JSON serialization settings.</param>
        /// <returns>
        /// The entity asynchronously retrieved by identifier
        /// </returns>
        public async Task<TEntity> FindOneByIdAsync<TEntity>(string moduleName, string entityId, IList<string> select = null, JsonSerializerSettings jsonSettings = null)
            where TEntity : class
        {
            var record = await parentClient.InvokeOperationAsync<TEntity>(
                "retrieve",
                new Dictionary<string, string> { { "id", entityId } },
                HttpMethod.Get,
                jsonSettings
            );
            if (null == record)
                return null;

            if (select?.Any() ?? false || typeof(TEntity) != typeof(JToken))
                return record;

            var recordJtoken = record as JToken;
            foreach (var field in recordJtoken.Values())
                field.Remove();

            return recordJtoken as TEntity;  
        }

        /// <summary>
        /// Retrieves an entity by identifier.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="numericId">The entity identifier.</param>
        /// <param name="select">The select.</param>
        /// <param name="jsonSettings">The JSON serialization settings.</param>
        /// <returns></returns>
        public TEntity FindOneById<TEntity>(string moduleName, long numericId, IList<string> select = null, JsonSerializerSettings jsonSettings = null)
            where TEntity : class
        {
            var findTask = FindOneByIdAsync<TEntity>(moduleName, numericId, select, jsonSettings);
            findTask.Wait();
            return findTask.Result;
        }

        /// <summary>
        /// Retrieves an entity by identifier.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="select">The select.</param>
        /// <param name="jsonSettings">The JSON serialization settings.</param>
        /// <returns></returns>
        public TEntity FindOneById<TEntity>(string moduleName, string entityId, IList<string> select = null, JsonSerializerSettings jsonSettings = null)
            where TEntity : class
        {
            var findTask = FindOneByIdAsync<TEntity>(moduleName, entityId, select, jsonSettings);
            findTask.Wait();
            return findTask.Result;
        }

        /// <summary>
        /// Retrieve the entity asynchronously matching a list of constraints.
        /// </summary>
        /// <typeparam name="TEntity">The name of the module / entity type.</typeparam>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="params">Data used to find a matching entry.</param>
        /// <param name="select">The list of fields to select (defaults to SQL-like '*' - all the fields).</param>
        /// <param name="jsonSettings">The JSON serialization settings.</param>
        /// <returns>
        /// The matching record
        /// </returns>
        public async Task<TEntity> FindOneAsync<TEntity>(
            string moduleName,
            IDictionary<string, string> @params,
            IList<string> select = null,
            JsonSerializerSettings jsonSettings = null
        )
            where TEntity : class
        {
            var entityID = GetId(moduleName, @params);
            return (string.IsNullOrWhiteSpace(entityID))
                ? null
                : await FindOneByIdAsync<TEntity>(moduleName, entityID, select, jsonSettings);
        }

        /// <summary>
        /// Retrieve the entity matching a list of constraints.
        /// </summary>
        /// <typeparam name="TEntity">The name of the module / entity type.</typeparam>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="params">Data used to find a matching entry.</param>
        /// <param name="select">The list of fields to select (defaults to SQL-like '*' - all the fields).</param>
        /// <param name="jsonSettings">The JSON serialization settings.</param>
        /// <returns>
        /// The matching record
        /// </returns>
        public TEntity FindOne<TEntity>(string moduleName, IDictionary<string, string> @params, IList<string> select = null, JsonSerializerSettings jsonSettings = null)
            where TEntity : class
        {
            var findTask = FindOneAsync<TEntity>(moduleName, @params, select, jsonSettings);
            findTask.Wait();
            return findTask.Result;
        }

        #endregion Find / retrieve ONE

        #region Find / retrieve MANY

        /// <summary>
        /// Retrieves multiple records asynchronously using module name and a set of constraints.
        /// </summary>
        /// <typeparam name="TEntity">The name of the module / entity type.</typeparam>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="params">Data used to find matching entries.</param>
        /// <param name="select">The list of fields to select (defaults to SQL-like '*' - all the fields).</param>
        /// <param name="limit">The limit the list of entries to N records (acts like LIMIT in SQL).</param>
        /// <param name="offset">The offset for the query.</param>
        /// <param name="jsonSettings">The JSON serialization settings.</param>
        /// <returns>
        /// The array containing matching entries or false if nothing was found
        /// </returns>
        public async Task<TEntity[]> FindManyAsync<
            TEntity>(string moduleName,
            IDictionary<string, string> @params, 
            IList<string> select = null,
            long limit = 0,
            long offset = 0,
            JsonSerializerSettings jsonSettings = null
        )
            where TEntity : class
        {
            // Builds the query
            var query = GetQueryString(moduleName, @params, select, limit, offset, jsonSettings);

            // Run the query
            var records = await parentClient.ExecuteQueryAsync<TEntity[]>(query, jsonSettings);
            return (records?.Any() ?? false)
                ? records
                : null;
        }

        /// <summary>
        /// Retrieves multiple records using module name and a set of constraints.
        /// </summary>
        /// <typeparam name="TEntity">The name of the module / entity type.</typeparam>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="params">Data used to find matching entries.</param>
        /// <param name="select">The list of fields to select (defaults to SQL-like '*' - all the fields).</param>
        /// <param name="limit">The limit the list of entries to N records (acts like LIMIT in SQL).</param>
        /// <param name="offset">The offset for the query.</param>
        /// <param name="jsonSettings">The JSON serialization settings.</param>
        /// <returns>
        /// The array containing matching entries or false if nothing was found
        /// </returns>
        public TEntity[] FindMany<TEntity>(
            string moduleName, 
            IDictionary<string, string> @params,
            IList<string> select = null,
            long limit = 0,
            long offset = 0,
            JsonSerializerSettings jsonSettings = null
        )
            where TEntity : class
        {
            var findManyTask = FindManyAsync<TEntity>(moduleName, @params, select, limit, offset, jsonSettings);
            findManyTask.Wait();
            return findManyTask.Result;
        }

        public object FindOneById<T>(object uPGRADES, string upgradeId)
        {
            throw new NotImplementedException();
        }

        #endregion Find / retrieve MANY

        #region Create

        /// <summary>
        /// Creates an entity asynchronously for the giving module / entity.
        /// </summary>
        /// <typeparam name="TEntity">Name of the module / entity type for which the entry has to be created.</typeparam>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="params">The data for the new entity.</param>
        /// <param name="jsonSettings">The JSON serialization settings.</param>
        /// <returns>
        /// A newly created entity
        /// </returns>
        public async Task<TEntity> CreateOneAsync<TEntity>(string moduleName, IDictionary<string, string> @params, JsonSerializerSettings jsonSettings = null)
            where TEntity : class
        {
            // Assign record to logged in user if not user has been specified
            if (!@params.ContainsKey("assigned_user_id"))
                @params["assigned_user_id"] = parentClient.Session.CurrentUser.Id;

            var entityData = new Dictionary<string, string>
            {
                { "elementType", moduleName },
                { "element", JsonConvert.SerializeObject(@params, jsonSettings ?? new JsonSerializerSettings()) }
            };

            return await parentClient.InvokeOperationAsync<TEntity>("create", entityData, HttpMethod.Post, jsonSettings);
        }

        /// <summary>
        /// Creates an entity for the giving module / entity.
        /// </summary>
        /// <typeparam name="TEntity">Name of the module / entity type for which the entry has to be created.</typeparam>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="params">The data for the new entity.</param>
        /// <param name="jsonSettings">The JSON serialization settings.</param>
        /// <returns>
        /// A newly created entity
        /// </returns>
        public TEntity CreateOne<TEntity>(string moduleName, IDictionary<string, string> @params, JsonSerializerSettings jsonSettings = null)
            where TEntity : class
        {
            var createTask = CreateOneAsync<TEntity>(moduleName, @params, jsonSettings);
            createTask.Wait();
            return createTask.Result;
        }

        /// <summary>
        /// Creates an entity asynchronously for the giving module / entity.
        /// </summary>
        /// <typeparam name="TEntity">Name of the module / entity type for which the entry has to be created.</typeparam>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="entity">The data for the new entity.</param>
        /// <param name="jsonSettings">The JSON serialization settings.</param>
        /// <returns>
        /// A newly created entity
        /// </returns>
        public async Task<TEntity> CreateOneAsync<TEntity>(string moduleName, TEntity entity, JsonSerializerSettings jsonSettings = null)
            where TEntity : class
        {
            var entityJObj = JObject.FromObject(entity, JsonSerializer.Create(jsonSettings));
            var @params = entityJObj.ToObject<Dictionary<string, object>>().ToDictionary(i => i.Key, i => i.Value?.ToString());
            return await CreateOneAsync<TEntity>(moduleName, @params, jsonSettings);
        }

        /// <summary>
        /// Creates an entity for the giving module / entity.
        /// </summary>
        /// <typeparam name="TEntity">Name of the module / entity type for which the entry has to be created.</typeparam>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="entity">The data for the new entity.</param>
        /// <param name="jsonSettings">The JSON serialization settings.</param>
        /// <returns>
        /// A newly created entity
        /// </returns>
        public TEntity CreateOne<TEntity>(string moduleName, TEntity entity, JsonSerializerSettings jsonSettings = null)
            where TEntity : class
        {
            var createTask = CreateOneAsync(moduleName, entity, jsonSettings);
            createTask.Wait();
            return createTask.Result;
        }

        #endregion Create

        #region Update typed ID

        /// <summary>
        /// Updates the given entity asynchronously.
        /// </summary>
        /// <typeparam name="TEntity">The name of the module / entity type.</typeparam>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="params">The new entity data.</param>
        /// <param name="jsonSettings">The JSON serialization settings.</param>
        /// <returns>
        /// Updated entity
        /// </returns>
        /// <exception cref="WebServiceException">The list of constrains must contain a valid ID
        /// or
        /// Such entity doesn't exist, so it cannot be updated</exception>
        public async Task<TEntity> UpdateOneAsync<TEntity>(
            string moduleName,
            string entityId,
            IDictionary<string, string> @params,
            JsonSerializerSettings jsonSettings = null
        )
            where TEntity : class
        {
            // Fail if no ID was supplied
            if (string.IsNullOrWhiteSpace(entityId))
                throw new WebServiceException("The list of constrains must contain a valid ID");

            // Check if the entity exists + retrieve its data so it can be used below
            var entityData = await FindOneByIdAsync<JObject>(moduleName, entityId, null, jsonSettings);
            if (null == entityData) {
                throw new WebServiceException("Such entity doesn't exist, so it cannot be updated");
            }

            // The new data overrides the existing one needed to provide
            // mandatory field values to WS 'update' operation
            entityData.Merge(
                new JObject(@params),
                new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace }
            );

            var requestData = new Dictionary<string, string>
            {
                { "elementType", moduleName },
                { "element", JsonConvert.SerializeObject(entityData) }
            };
            return await parentClient.InvokeOperationAsync<TEntity>("update", requestData, HttpMethod.Post, jsonSettings);
        }

        /// <summary>
        /// Updates the given entity.
        /// </summary>
        /// <typeparam name="TEntity">The name of the module / entity type.</typeparam>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="params">The new entity data.</param>
        /// <param name="jsonSettings">The JSON serialization settings.</param>
        /// <returns>
        /// Updated entity
        /// </returns>
        /// <exception cref="WebServiceException">The list of constrains must contain a valid ID
        /// or
        /// Such entity doesn't exist, so it cannot be updated</exception>
        public TEntity UpdateOne<TEntity>(
            string moduleName,
            string entityId,
            IDictionary<string, string> @params,
            JsonSerializerSettings jsonSettings = null
        )
            where TEntity : class
        {
            var updateTask = UpdateOneAsync<TEntity>(moduleName, entityId, @params, jsonSettings);
            updateTask.Wait();
            return updateTask.Result;
        }

        /// <summary>
        /// Updates the given entity asynchronously.
        /// </summary>
        /// <typeparam name="TEntity">The name of the module / entity type.</typeparam>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="jsonSettings">The JSON serialization settings.</param>
        /// <returns>
        /// Updated entity
        /// </returns>
        /// <exception cref="WebServiceException">The list of constrains must contain a valid ID
        /// or
        /// Such entity doesn't exist, so it cannot be updated</exception>
        public async Task<TEntity> UpdateOneAsync<TEntity>(
            string moduleName,
            string entityId,
            TEntity entity,
            JsonSerializerSettings jsonSettings = null
        )
            where TEntity : class
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Fail if no ID was supplied
            if (string.IsNullOrWhiteSpace(entityId))
                throw new WebServiceException("The list of constrains must contain a valid ID");

            var entityData = JObject.FromObject(entity, JsonSerializer.Create(jsonSettings))
                                    .ToObject<Dictionary<string, object>>()
                                    .ToDictionary(i => i.Key, i => i.Value?.ToString());
            var requestData = new Dictionary<string, string>
            {
                { "elementType", moduleName },
                { "element", JsonConvert.SerializeObject(entityData) }
            };
            return await parentClient.InvokeOperationAsync<TEntity>("update", requestData, HttpMethod.Post, jsonSettings);
        }

        /// <summary>
        /// Updates the given entity.
        /// </summary>
        /// <typeparam name="TEntity">The name of the module / entity type.</typeparam>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="jsonSettings">The JSON serialization settings.</param>
        /// <returns>
        /// Updated entity
        /// </returns>
        /// <exception cref="WebServiceException">The list of constrains must contain a valid ID
        /// or
        /// Such entity doesn't exist, so it cannot be updated</exception>
        public TEntity UpdateOne<TEntity>(
            string moduleName,
            string entityId,
            TEntity entity,
            JsonSerializerSettings jsonSettings = null
        )
            where TEntity : class
        {
            var updateTask = UpdateOneAsync(moduleName, entityId, entity, jsonSettings);
            updateTask.Wait();
            return updateTask.Result;
        }

        #endregion Update typed ID

        #region Update numeric ID

        /// <summary>
        /// Updates the given entity.
        /// </summary>
        /// <typeparam name="TEntity">The name of the module / entity type.</typeparam>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="params">The new entity data.</param>
        /// <param name="jsonSettings">The JSON serialization settings.</param>
        /// <returns>
        /// Updated entity
        /// </returns>
        /// <exception cref="WebServiceException">The list of constrains must contain a valid ID
        /// or
        /// Such entity doesn't exist, so it cannot be updated</exception>
        public async Task<TEntity> UpdateOneAsync<TEntity>(
            string moduleName,
            long numericId,
            IDictionary<string, string> @params,
            JsonSerializerSettings jsonSettings = null
        )
            where TEntity : class
        {
            var entityTypedId = await parentClient.Modules.GetTypedIdAsync(moduleName, numericId, jsonSettings);
            return await UpdateOneAsync<TEntity>(moduleName, entityTypedId, @params, jsonSettings);
        }

        /// <summary>
        /// Updates the given entity.
        /// </summary>
        /// <typeparam name="TEntity">The name of the module / entity type.</typeparam>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="params">The new entity data.</param>
        /// <param name="jsonSettings">The JSON serialization settings.</param>
        /// <returns>
        /// Updated entity
        /// </returns>
        /// <exception cref="WebServiceException">The list of constrains must contain a valid ID
        /// or
        /// Such entity doesn't exist, so it cannot be updated</exception>
        public object UpdateOne<TEntity>(
            string moduleName,
            long numericId,
            IDictionary<string, string> @params,
            JsonSerializerSettings jsonSettings = null
        )
            where TEntity : class
        {
            var updateTask = UpdateOneAsync(moduleName, numericId, @params, jsonSettings);
            updateTask.Wait();
            return updateTask.Result;
        }

        /// <summary>
        /// Updates the given entity asynchronously.
        /// </summary>
        /// <typeparam name="TEntity">The name of the module / entity type.</typeparam>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="jsonSettings">The JSON serialization settings.</param>
        /// <returns>
        /// Updated entity
        /// </returns>
        /// <exception cref="WebServiceException">The list of constrains must contain a valid ID
        /// or
        /// Such entity doesn't exist, so it cannot be updated</exception>
        public async Task<TEntity> UpdateOneAsync<TEntity>(
            string moduleName,
            long numericId,
            TEntity entity,
            JsonSerializerSettings jsonSettings = null
        )
            where TEntity : class
        {
            var entityTypedId = await parentClient.Modules.GetTypedIdAsync(moduleName, numericId, jsonSettings);
            return await UpdateOneAsync(moduleName, entityTypedId, entity, jsonSettings);
        }

        /// <summary>
        /// Updates the given entity.
        /// </summary>
        /// <typeparam name="TEntity">The name of the module / entity type.</typeparam>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="jsonSettings">The JSON serialization settings.</param>
        /// <returns>
        /// Updated entity
        /// </returns>
        /// <exception cref="WebServiceException">The list of constrains must contain a valid ID
        /// or
        /// Such entity doesn't exist, so it cannot be updated</exception>
        public object UpdateOne<TEntity>(
            string moduleName,
            long numericId,
            TEntity entity,
            JsonSerializerSettings jsonSettings = null
        )
            where TEntity : class
        {
            var updateTask = UpdateOneAsync(moduleName, numericId, entity, jsonSettings);
            updateTask.Wait();
            return updateTask.Result;
        }

        #endregion Update numeric ID

        #region Delete

        /// <summary>
        /// Deletes the given entity asynchronously.
        /// </summary>
        /// <param name="moduleName">The name of the module / entity type.</param>
        /// <param name="numericId">The identifier of the entity to delete.</param>
        /// <param name="jsonSettings">The JSON serialization settings.</param>
        /// <returns></returns>
        public async Task<bool> DeleteOneAsync(string moduleName, long numericId, JsonSerializerSettings jsonSettings = null)
        {
            var entityTypedId = await parentClient.Modules.GetTypedIdAsync(moduleName, numericId, jsonSettings);
            return await DeleteOneAsync(moduleName, entityTypedId, jsonSettings);
        }

        /// <summary>
        /// Deletes the given entity asynchronously.
        /// </summary>
        /// <param name="moduleName">The name of the module / entity type.</param>
        /// <param name="entityId">The identifier of the entity to delete.</param>
        /// <param name="jsonSettings">The JSON serialization settings.</param>
        /// <returns></returns>
        public async Task<bool> DeleteOneAsync(string moduleName, string entityId, JsonSerializerSettings jsonSettings = null)
        {
            var removalResult = await parentClient.InvokeOperationAsync<JToken>(
                "delete",
                new Dictionary<string, string> { { "id", entityId } },
                HttpMethod.Post,
                jsonSettings
            );
            return null != removalResult;
        }

        /// <summary>
        /// Deletes the given entity.
        /// </summary>
        /// <param name="moduleName">The name of the module / entity type.</param>
        /// <param name="entityId">The identifier of the entity to delete.</param>
        /// <param name="jsonSettings">The JSON serialization settings.</param>
        /// <returns></returns>
        public JToken DeleteOne(string moduleName, string entityId, JsonSerializerSettings jsonSettings = null)
        {
            var deleteTask = DeleteOneAsync(moduleName, entityId, jsonSettings);
            deleteTask.Wait();
            return deleteTask.Result;
        }

        /// <summary>
        /// Deletes the given entity.
        /// </summary>
        /// <param name="moduleName">The name of the module / entity type.</param>
        /// <param name="numericId">The identifier of the entity to delete.</param>
        /// <param name="jsonSettings">The JSON serialization settings.</param>
        /// <returns></returns>
        public JToken DeleteOne(string moduleName, long numericId, JsonSerializerSettings jsonSettings = null)
        {
            var deleteTask = DeleteOneAsync(moduleName, numericId, jsonSettings);
            deleteTask.Wait();
            return deleteTask.Result;
        }

        #endregion Delete

        #region Sync

        /// <summary>
        /// Sync will asynchronously return a sync result object, containing details of changes after modifiedTime.
        /// </summary>
        /// <param name="modifiedTime">The date of the first change.</param>
        /// <param name="moduleName">The name of the module / entity type.</param>
        /// <param name="jsonSettings">The JSON serialization settings.</param>
        /// <returns>
        /// The list of modified and newly created entities
        /// </returns>
        public async Task<JToken> SyncAsync(DateTime modifiedTime, string moduleName = null, JsonSerializerSettings jsonSettings = null)
        {
            var unixTime = (modifiedTime == null || modifiedTime <= DateTime.MinValue)
                ? DateTime.Today.Date
                : modifiedTime;

            var requestData = new Dictionary<string, string> { { "modifiedTime", unixTime.ToString() } };
            if (string.IsNullOrWhiteSpace(moduleName))
                requestData["elementType"] = moduleName;

            // TODO create a class structure for sync result object
            return await parentClient.InvokeOperationAsync<JToken>("sync", requestData, HttpMethod.Get, jsonSettings);
        }

        /// <summary>
        /// Sync will return a sync result object, containing details of changes after modifiedTime.
        /// </summary>
        /// <param name="modifiedTime">The date of the first change.</param>
        /// <param name="moduleName">The name of the module / entity type.</param>
        /// <param name="jsonSettings">The JSON serialization settings.</param>
        /// <returns>
        /// The list of modified and newly created entities
        /// </returns>
        public JToken Sync(DateTime modifiedTime, string moduleName = null, JsonSerializerSettings jsonSettings = null)
        {
            var syncTask = SyncAsync(modifiedTime, moduleName);
            syncTask.Wait();
            return syncTask.Result; // TODO create a class structure for sync result object
        }

        #endregion Sync

        #region Helpers

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="params">The parameters.</param>
        /// <returns></returns>
        public string GetId(string moduleName, IDictionary<string, string> @params)
        {
            var query = GetQueryString(moduleName, @params, new[] { "id" }, 1);
            var records = parentClient.ExecuteQuery<CrmEntity[]>(query);
            return records?.FirstOrDefault()?.TypedId;
        }

        /// <summary>
        /// Retrieve a numeric ID of the entity matching a list of constraints.
        /// </summary>
        /// <param name="moduleName">The name of the module / entity type.</param>
        /// <param name="params">Data used to find a matching entry.</param>
        /// <returns>
        /// The numeric ID
        /// </returns>
        public long GetNumericId(string moduleName, IDictionary<string, string> @params)
        {
            var entityID = GetId(moduleName, @params);
            return long.TryParse(entityID?.Split('x')?.LastOrDefault() ?? string.Empty, out long parsedId)
                ? parsedId
                : -1;
        }

        /// <summary>
        /// Builds the query using the supplied parameters.
        /// </summary>
        /// <param name="moduleName">The name of the module / entity type.</param>
        /// <param name="params">Data used to find matching entries.</param>
        /// <param name="select">The list of fields to select (defaults to SQL-like '*' - all the fields).</param>
        /// <param name="limit">The limit the list of entries to N records (acts like LIMIT in SQL).</param>
        /// <param name="offset">The offset.</param>
        /// <param name="jsonSettings">The JSON serialization settings.</param>
        /// <returns>
        /// The query build out of the supplied parameters
        /// </returns>
        private static string GetQueryString(
            string moduleName,
            IDictionary<string, string> @params,
            IList<string> select = null,
            long limit = 0,
            long offset = 0,
            JsonSerializerSettings jsonSettings = null
        ){
            var query = (select?.Any() ?? false) ? string.Join(", ", select) : "*";
            query = $"SELECT {query} FROM {moduleName}";

            if (@params?.Any() ?? false)
            {
                var likes = new List<string>();
                foreach (var criteria in @params)
                    likes.Add($"{criteria.Key} LIKE '{criteria.Value}'");

                query = $"{query} WHERE {string.Join(" AND ", likes)}";
            }

            if (limit <= 0)
                return query;

            return (offset > 0)
                ? $"{query} LIMIT {offset}, {limit};"
                : $"{query} LIMIT {limit};";
        }

        #endregion
    }
}
