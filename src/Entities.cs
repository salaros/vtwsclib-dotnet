namespace Salaros.Vtiger.VTWSCLib
{
    public class Entities
    {
        /// <summary>
        /// The parent <see cref="WSClient"/> client object
        /// </summary>
        protected WSClient parentClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="Entities"/> class.
        /// </summary>
        /// <param name="parentClient">The parent client.</param>
        public Entities(WSClient parentClient)
        {
            this.parentClient = parentClient;
        }
    }
}
