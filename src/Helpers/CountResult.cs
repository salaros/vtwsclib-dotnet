using Newtonsoft.Json;

namespace Salaros.vTiger.WebService
{
    internal class CountResult
    {
        [JsonProperty("count")]
        public long Count { get; set; }
    }
}
