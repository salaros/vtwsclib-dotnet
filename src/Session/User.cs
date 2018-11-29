namespace Salaros.Vtiger.WebService
{
    public class User
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id { get; internal set; }

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
        public string SecretKey { get; internal set; }
    }
}
