using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetduinoConnect.Models
{
    public class PayloadEntity<TPayload>
    {
        [JsonProperty("payload")]
        public TPayload Payload { get; set; }
    }

    public class PayloadEntity : PayloadEntity<object>
    { }
}
