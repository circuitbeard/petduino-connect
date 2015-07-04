using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetduinoConnect.Models
{
    public enum ActionType
    {
        SetState,
        GetState,
        SetLed,
        ToggleLed,
        GetLed,
        GetTemperature,
        GetLightLevel,

        SetData
    }
}
