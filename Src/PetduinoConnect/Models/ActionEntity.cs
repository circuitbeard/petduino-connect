using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetduinoConnect.Models
{
    public class ActionEntity : Entity
    {
        public ActionEntity()
        {
            Type = EntityType.Action;
        }

        [JsonProperty("name")]
        public ActionType Name { get; set; }
    }
}
