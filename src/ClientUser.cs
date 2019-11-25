namespace Salaros.vTiger.WebService
{
    public class ClientUser : ClientCredentials
    {
        /// <summary>Initializes a new instance of the <see cref="ClientUser"/> class.</summary>
        /// <param name="id">The identifier.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="accessKey">The access key.</param>
        internal ClientUser(string id, string userName, string accessKey)
            : base(userName, accessKey)
        {
            Id = id;
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id { get; internal set; }
    }
}
