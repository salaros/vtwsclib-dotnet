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
    }
}
