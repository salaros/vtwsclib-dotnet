using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Salaros.Vtiger.VTWSCLib
{
    public class Modules
    {
        /// <summary>
        /// The parent <see cref="WSClient"/> client object
        /// </summary>
        protected WSClient parentClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="Modules"/> class.
        /// </summary>
        /// <param name="parentClient">The parent client.</param>
        public Modules(WSClient parentClient)
        {
            this.parentClient = parentClient;
        }

        /// <summary>
        /// Lists all the vTiger entity types available through the API
        /// </summary>
        /// <returns>List of entity types</returns>
        public async Task<Module[]> GetAllAsync()
        {
            var typeInfos = await parentClient.InvokeOperationAsync<Module[]>(
                "listtypes",
                new Dictionary<string, string>(0),
                HttpMethod.Get
            );

            if (typeInfos?.Any() ?? false)
                throw new WSException($"Failed to retrieve CRM modules");

            return typeInfos;
        }

        /// <summary>
        /// Gets the info on all the modules
        /// </summary>
        /// <returns></returns>
        public Module[] GetAll()
        {
            var getAllTask = GetAllAsync();
            getAllTask.Wait();
            return getAllTask.Result;
        }

        /// <summary>
        /// Get the type information about a given VTiger entity type.
        /// </summary>
        /// <param name="moduleName">Name of the module / entity type.</param>
        /// <returns>List of modules</returns>
        public async Task<Module> GetOneAsync(string moduleName)
        {
            return await parentClient.InvokeOperationAsync<Module>(
                "describe",
                new Dictionary<string, string> { { "elementType", moduleName } },
                HttpMethod.Get
            );
        }

        /// <summary>
        /// Gets the one.
        /// </summary>
        /// <param name="moduleName">Name of the module.</param>
        /// <returns></returns>
        public Module GetOne(string moduleName)
        {
            var getOneTask = GetOneAsync(moduleName);
            getOneTask.Wait();
            return getOneTask.Result;
        }

        /// <summary>
        /// Gets the entity ID perpended with module / entity type ID.
        /// </summary>
        /// <param name="moduleName">Name of the module / entity type.</param>
        /// <param name="entityID">Numeric entity ID.</param>
        /// <returns>Returns false if it is not possible to retrieve module / entity type ID</returns>
        /// <exception cref="WSException">
        /// Entity ID must be a valid number
        /// or
        /// The following module is not installed: moduleName
        /// </exception>
        public async Task<string> GetTypedIdAsync(string moduleName, long entityID)
        {
            if (entityID < 1)
                throw new WSException("Entity ID must be a valid number");

            var typeInfo = await GetOneAsync(moduleName);
            if (null == typeInfo || string.IsNullOrWhiteSpace(typeInfo.IdPrefix))
                throw new WSException($"The following module is not installed: {moduleName}");

            return $"{typeInfo}x{entityID}";
        }

        /// <summary>
        /// Gets the entity ID perpended with module / entity type ID.
        /// </summary>
        /// <param name="moduleName">Name of the module / entity type.</param>
        /// <param name="entityID">Numeric entity ID.</param>
        /// <returns>Returns false if it is not possible to retrieve module / entity type ID</returns>
        /// <exception cref="WSException">
        /// Entity ID must be a valid number
        /// or
        /// The following module is not installed: moduleName
        /// </exception>
        public string GetTypedID(string moduleName, long entityID)
        {
            var getTypedTask = GetTypedIdAsync(moduleName, entityID);
            getTypedTask.Wait();
            return getTypedTask.Result;
        }
    }
}
