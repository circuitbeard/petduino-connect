using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetduinoConnect.Models
{
    public class Entity
    {
        [JsonProperty("type")]
        public EntityType Type { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
