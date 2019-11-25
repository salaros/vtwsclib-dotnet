namespace Salaros.vTiger.WebService
{
    public class ClientCredentials
    {
        public ClientCredentials(string userName, string accessKey)
        {
            if (string.IsNullOrWhiteSpace(userName)) throw new System.ArgumentException(nameof(userName));
            if (string.IsNullOrWhiteSpace(accessKey)) throw new System.ArgumentException(nameof(accessKey));

            UserName = userName;
            AccessKey = accessKey;
        }

        /// <summary>
        /// Gets the name of the user.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        public string UserName { get; internal set; }

        /// <summary>
        /// Gets the access key or password.
        /// </summary>
        /// <value>
        /// Access or password key.
        /// </value>
        internal string AccessKey { get; set; }
    }
}
