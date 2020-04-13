using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Pirat.Configuration
{
    public class Language
    {
        [JsonProperty("consumable", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Needed for parsing")]
        public Dictionary<string, string> Consumable { get; set; }

        [JsonProperty("devices", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Needed for parsing")]
        public Dictionary<string, string> Devices { get; set; }
    }
}