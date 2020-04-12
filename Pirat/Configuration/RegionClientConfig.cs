﻿using Newtonsoft.Json;
using Pirat.Services.Resource;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Pirat.Configuration
{
    public class RegionClientConfig
    {
        [JsonProperty("languages", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Needed for parsing")]
        public Dictionary<string, Language> Languages { get; set; }

        [JsonProperty("categories", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Categories Categories { get; set; }
    }
}
