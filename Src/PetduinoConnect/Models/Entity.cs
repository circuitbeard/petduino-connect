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
        [JsonConverter(typeof(StringEnumConverter))]
        public EntityType Type { get; set; } 
    }
}
