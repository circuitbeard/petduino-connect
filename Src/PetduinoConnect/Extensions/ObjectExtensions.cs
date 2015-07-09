using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetduinoConnect.Extensions
{
    internal static class ObjectExtensions
    {
        public static bool IsPayloadObject(this object obj)
        {
            var jObj = obj as JObject;
            return jObj == null ? false : jObj["payload"] != null;
        }
    }
}
