using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetduinoConnect.Models
{
    public class EventEntity : Entity 
    {
        public EventEntity()
        {
            Type = EntityType.Event;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public EventType Name { get; set; }
    }
}
