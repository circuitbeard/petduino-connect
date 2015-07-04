using Nini.Config;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PetduinoConnect.Extensions;

namespace PetduinoConnect
{
    public class Config
    {
        private static Config _instance;

        public PetduinoConfig Petduino { get; private set; }
        public DweetConfig Dweet { get; private set; }
        public SerialConfig Serial { get; private set; }
        public GeneralConfig General { get; private set; }

        static Config()
        {
            var cfg = new IniConfigSource("PetduinoConnect.ini");

            _instance = new Config
            {
                Petduino = new PetduinoConfig
                {
                    DeviceId = cfg.Configs["Petduino"].Get("device_id")
                },
                Dweet = new DweetConfig
                {
                    Thing = cfg.Configs["Dweet"].Get("dweet_thing")
                },
                Serial = new SerialConfig
                {
                    Port = cfg.Configs["Serial"].Get("com_port"),
                    BuadRate = cfg.Configs["Serial"].GetInt("com_baud", 9660),
                    DataBits = cfg.Configs["Serial"].GetInt("com_databits", 8),
                    StopBits = cfg.Configs["Serial"].GetStopBits("com_stopbits", StopBits.One),
                    Parity = cfg.Configs["Serial"].GetEnum<Parity>("parity", Parity.None),
                    DtrEnable = cfg.Configs["Serial"].GetBoolean("dtr_enable")
                },
                General = new GeneralConfig
                {
                    Timeout = cfg.Configs["General"].GetInt("timeout", 3000)
                }
            };
        }

        public static Config Instance
        {
            get { return _instance; }
        }
    }

    #region ConfigModels

    public class PetduinoConfig
    {
        public string DeviceId { get; internal set; }
    }

    public class DweetConfig
    {
        public string Thing { get; internal set; }
    }

    public class SerialConfig
    {
        public string Port { get; internal set; }
        public int BuadRate { get; internal set; }
        public int DataBits { get; internal set; }
        public StopBits StopBits { get; internal set; }
        public Parity Parity { get; internal set; }
        public bool DtrEnable { get; internal set; }
    }

    public class GeneralConfig
    {
        public int Timeout { get; internal set; }
    }

    #endregion
}
