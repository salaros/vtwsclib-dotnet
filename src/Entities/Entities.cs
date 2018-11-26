using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Salaros.Vtiger.VTWSCLib
{
    public class Entities
    {
        /// <summary>
        /// The parent <see cref="WSClient"/> client object
        /// </summary>
        protected WSClient parentClient;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Entities"/> class.
        /// </summary>
        /// <param name="parentClient">The parent client.</param>
        public Entities(WSClient parentClient)
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
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="select">The select.</param>
        /// <returns></returns>
        public async Task<TEntity> FindOneByIdAsync<TEntity>(string moduleName, long entityId, IList<string> select = null)
            where TEntity : class
        {
            var entityTypedId = await parentClient.Modules.GetTypedIdAsync(moduleName, entityId);
            return await FindOneByIdAsync<TEntity>(moduleName, entityTypedId, select);
        }

        /// <summary>
        /// Retrieves an entity by identifier asynchronously.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="moduleName">The name of the module / entity type.</param>
        /// <param name="entityID">The ID of the entity to retrieve.</param>
        /// <param name="select">The list of fields to select (defaults to SQL-like '*' - all the fields).</param>
        /// <returns>
        /// The entity asynchronously retrieved by identifier
        /// </returns>
        public async Task<TEntity> FindOneByIdAsync<TEntity>(string moduleName, string entityId, IList<string> select = null)
            where TEntity : class
        {
            var record = await parentClient.InvokeOperationAsync<TEntity>(
                "retrieve",
                new Dictionary<string, string> { { "id", entityId } },
                HttpMethod.Get
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
        /// Retrieve the entity asynchronously matching a list of constraints.
        /// </summary>
        /// <typeparam name="TEntity">The name of the module / entity type.</typeparam>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="params">Data used to find a matching entry.</param>
        /// <param name="select">The list of fields to select (defaults to SQL-like '*' - all the fields).</param>
        /// <returns>
        /// The matching record
        /// </returns>
        public async Task<TEntity> FindOneAsync<TEntity>(string moduleName, IDictionary<string, string> @params, IList<string> select = null)
            where TEntity : class
        {
            var entityID = GetNumericId(moduleName, @params);
            return (entityID < 0)
                ? null
                : await FindOneByIdAsync<TEntity>(moduleName, entityID, select);
        }

        /// <summary>
        /// Retrieve the entity matching a list of constraints.
        /// </summary>
        /// <typeparam name="TEntity">The name of the module / entity type.</typeparam>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="params">Data used to find a matching entry.</param>
        /// <param name="select">The list of fields to select (defaults to SQL-like '*' - all the fields).</param>
        /// <returns>
        /// The matching record
        /// </returns>
        public TEntity FindOne<TEntity>(string moduleName, IDictionary<string, string> @params, IList<string> select = null)
            where TEntity : class
        {
            var findTask = FindOneAsync<TEntity>(moduleName, @params, select);
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
        /// <returns>
        /// The array containing matching entries or false if nothing was found
        /// </returns>
        /// <exception cref="WSException">You have to specify at least one search parameter (prop => value) in order to be able to retrieve entity(ies)</exception>
        public async Task<TEntity[]> FindManyAsync<TEntity>(string moduleName, IDictionary<string, string> @params, IList<string> select = null, int limit = 0)
            where TEntity : class
        {
            if (!(@params?.Any() ?? false))
            {
                throw new WSException(
                    "You have to specify at least one search parameter (prop => value) in order to be able to retrieve entity(ies)"
                );
            }

            // Builds the query
            var query = GetQueryString(moduleName, @params, select, limit);

            // Run the query
            var records = await parentClient.ExecuteQueryAsync<TEntity[]>(query);
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
        /// <returns>
        /// The array containing matching entries or false if nothing was found
        /// </returns>
        /// <exception cref="WSException">You have to specify at least one search parameter (prop => value) in order to be able to retrieve entity(ies)</exception>
        public TEntity[] FindMany<TEntity>(string moduleName, IDictionary<string, string> @params, IList<string> select = null, int limit = 0)
            where TEntity : class
        {
            var findManyTask = FindManyAsync<TEntity>(moduleName, @params, select, limit);
            findManyTask.Wait();
            return findManyTask.Result;
        }

        #endregion Find / retrieve MANY

        #region Create

        /// <summary>
        /// Creates an entity asynchronously for the giving module / entity.
        /// </summary>
        /// <typeparam name="TEntity">Name of the module / entity type for which the entry has to be created.</typeparam>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="params">The data for the new entity.</param>
        /// <returns>A newly created entity</returns>
        /// <exception cref="WSException">You have to specify at least one search parameter (prop => value) in order to be able to create an entity</exception>
        public async Task<TEntity> CreateOneAsync<TEntity>(string moduleName, IDictionary<string, string> @params)
            where TEntity : class
        {
            if (!(@params?.Any() ?? false))
            {
                throw new WSException(
                    "You have to specify at least one search parameter (prop => value) in order to be able to create an entity"
                );
            }

            // Assign record to logged in user if not user has been specified
            if (!@params.ContainsKey("assigned_user_id"))
                @params["assigned_user_id"] = parentClient.Session.CurrentUser.Id;

            var entityData = new Dictionary<string, string>
            {
                { "elementType", moduleName },
                { "element", JsonConvert.SerializeObject(@params) }
            };

            return await parentClient.InvokeOperationAsync<TEntity>("create", entityData, HttpMethod.Post);
        }

        /// <summary>
        /// Creates an entity for the giving module / entity.
        /// </summary>
        /// <typeparam name="TEntity">Name of the module / entity type for which the entry has to be created.</typeparam>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="params">The data for the new entity.</param>
        /// <returns>A newly created entity</returns>
        /// <exception cref="WSException">You have to specify at least one search parameter (prop => value) in order to be able to create an entity</exception>
        public TEntity CreateOne<TEntity>(string moduleName, IDictionary<string, string> @params)
            where TEntity : class
        {
            var createTask = CreateOneAsync<TEntity>(moduleName, @params);
            createTask.Wait();
            return createTask.Result;
        }

        #endregion Create

        #region Update

        /// <summary>
        /// Updates the given entity asynchronously.
        /// </summary>
        /// <typeparam name="TEntity">The name of the module / entity type.</typeparam>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="params">The new entity data.</param>
        /// <returns>Updated entity</returns>
        /// <exception cref="WSException">
        /// You have to specify at least one search parameter (prop => value) in order to be able to update the entity(ies)
        /// or
        /// The list of constrains must contain a valid ID
        /// or
        /// Such entity doesn't exist, so it cannot be updated
        /// </exception>
        public async Task<TEntity> UpdateOneAsync<TEntity>(string moduleName, string entityId, IDictionary<string, string> @params)
            where TEntity : class
        {
            if (!(@params?.Any() ?? false))
            {
                throw new WSException(
                    "You have to specify at least one search parameter (prop => value) in order to be able to update the entity(ies)"
                );
            }
            // Fail if no ID was supplied
            if (string.IsNullOrWhiteSpace(entityId))
                throw new WSException("The list of constrains must contain a valid ID");

            // Check if the entity exists + retrieve its data so it can be used below
            var entityData = await FindOneByIdAsync<JObject>(moduleName, entityId);
            if (null == entityData) {
                throw new WSException("Such entity doesn't exist, so it cannot be updated");
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
            return await parentClient.InvokeOperationAsync<TEntity>("update", requestData, HttpMethod.Get);
        }

        /// <summary>
        /// Updates the given entity.
        /// </summary>
        /// <typeparam name="TEntity">The name of the module / entity type.</typeparam>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="params">The new entity data.</param>
        /// <returns>Updated entity</returns>
        /// <exception cref="WSException">
        /// You have to specify at least one search parameter (prop => value) in order to be able to update the entity(ies)
        /// or
        /// The list of constrains must contain a valid ID
        /// or
        /// Such entity doesn't exist, so it cannot be updated
        /// </exception>
        public TEntity UpdateOne<TEntity>(string moduleName, string entityId, IDictionary<string, string> @params)
            where TEntity : class
        {
            var updateTask = UpdateOneAsync<TEntity>(moduleName, entityId, @params);
            updateTask.Wait();
            return updateTask.Result;
        }

        #endregion Update

        #region Delete

        /// <summary>
        /// Deletes the given entity asynchronously.
        /// </summary>
        /// <param name="moduleName">The name of the module / entity type.</param>
        /// <param name="entityId">The identifier of the entity to delete.</param>
        /// <returns></returns>
        public async Task<bool> DeleteOneAsync(string moduleName, long entityId)
        {
            var entityTypedId = await parentClient.Modules.GetTypedIdAsync(moduleName, entityId);
            return await DeleteOneAsync(moduleName, entityTypedId);
        }

        /// <summary>
        /// Deletes the given entity asynchronously.
        /// </summary>
        /// <param name="moduleName">The name of the module / entity type.</param>
        /// <param name="entityId">The identifier of the entity to delete.</param>
        /// <returns></returns>
        public async Task<bool> DeleteOneAsync(string moduleName, string entityId)
        {
            var removalResult = await parentClient.InvokeOperationAsync<JToken>(
                "delete",
                new Dictionary<string, string> { { "id", entityId } },
                HttpMethod.Post
            );
            return null != removalResult;
        }

        /// <summary>
        /// Deletes the given entity.
        /// </summary>
        /// <param name="moduleName">The name of the module / entity type.</param>
        /// <param name="entityId">The identifier of the entity to delete.</param>
        /// <returns></returns>
        public JToken DeleteOne(string moduleName, string entityId)
        {
            var deleteTask = DeleteOneAsync(moduleName, entityId);
            deleteTask.Wait();
            return deleteTask.Result;
        }

        /// <summary>
        /// Deletes the given entity.
        /// </summary>
        /// <param name="moduleName">The name of the module / entity type.</param>
        /// <param name="entityId">The identifier of the entity to delete.</param>
        /// <returns></returns>
        public JToken DeleteOne(string moduleName, long entityId)
        {
            var deleteTask = DeleteOneAsync(moduleName, entityId);
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
        /// <returns>The list of modified and newly created entities</returns>
        public async Task<JToken> SyncAsync(DateTime modifiedTime, string moduleName = null)
        {
            var unixTime = (modifiedTime == null || modifiedTime <= DateTime.MinValue)
                ? DateTime.Today.Date
                : modifiedTime;

            var requestData = new Dictionary<string, string> { { "modifiedTime", unixTime.ToString() } };
            if (string.IsNullOrWhiteSpace(moduleName))
                requestData["elementType"] = moduleName;

            // TODO create a class structure for sync result object
            return await parentClient.InvokeOperationAsync<JToken>("sync", requestData, HttpMethod.Get);
        }

        /// <summary>
        /// Sync will return a sync result object, containing details of changes after modifiedTime.
        /// </summary>
        /// <param name="modifiedTime">The date of the first change.</param>
        /// <param name="moduleName">The name of the module / entity type.</param>
        /// <returns>The list of modified and newly created entities</returns>
        public JToken Sync(DateTime modifiedTime, string moduleName = null)
        {
            var syncTask = SyncAsync(modifiedTime, moduleName);
            syncTask.Wait();
            return syncTask.Result; // TODO create a class structure for sync result object
        }

        #endregion Sync

        #region Helpers

        /// <summary>
        /// Retrieves the ID of the entity matching a list of constraints + prepends '<module_id>x' string to it.
        /// </summary>
        /// <param name="moduleName">The name of the module / entity type.</param>
        /// <param name="params">Data used to find a matching entry.</param>
        /// <returns>So-called typed ID: '{module}x{entity}')</returns>
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
        /// <returns>The numeric ID</returns>
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
        /// <returns>
        /// The query build out of the supplied parameters
        /// </returns>
        private static string GetQueryString(string moduleName, IDictionary<string, string> @params, IList<string> select = null, int limit = 0)
        {
            var query = (select?.Any() ?? false) ? "*" : string.Join(", ", select);
            query = $"SELECT {query} FROM $moduleName";

            if (@params?.Any() ?? false)
            {
                var likes = new List<string>();
                foreach (var criteria in @params)
                    likes.Add($"{criteria.Key} LIKE '{criteria.Value}'");

                query = $"{query} WHERE {string.Join(" AND ", likes)}";
            }

            return (limit > 0)
                ? $"{query} LIMIT {limit}"
                : query;
        }

        #endregion
    }
}
