using CommandMessenger;
using CommandMessenger.Transport.Serial;
using DweetNet;
using DweetNet.Models;
using PetduinoConnect.Models;
using PetduinoConnect.Utils;
using PetduinoConnect.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace PetduinoConnect
{
    public class PetduinoConnect : AbstractSketch
    {
        private CmdMessenger _cmdMessenger;
        private SerialTransport _serialTransport;
        private DweetClient _dweetClient;

        public override void Setup()
        {
            var version = typeof(Program).Assembly.GetName().Version;

            // Set console title
            Console.Title = "PetduinoConnect v" + version;

            // Display header
            Console.WriteLine(@"                                                                           ");
            Console.WriteLine(@"  ########  ######## ######## ########  ##     ## #### ##    ##  #######   ");
            Console.WriteLine(@"  ##     ## ##          ##    ##     ## ##     ##  ##  ####  ## ##     ##  ");
            Console.WriteLine(@"  ########  ######      ##    ##     ## ##     ##  ##  ## ## ## ##     ##  ");
            Console.WriteLine(@"  ##        ##          ##    ##     ## ##     ##  ##  ##  #### ##     ##  ");
            Console.WriteLine(@"  ##        ########    ##    ########   #######  #### ##    ##  #######   ");
            Console.WriteLine(@"                                                                           ");
            Console.WriteLine(@"                      PetduinoConnect Version " + version + "              ");
            Console.WriteLine(@"                       Copyright "+ DateTime.Today.Year +" @Circuitbeard   ");
            Console.WriteLine(@"                                                                           ");
            Console.WriteLine(@"                                                                           ");

            // Force enum serialization to be in camelcase
            JsonConvert.DefaultSettings = (() =>
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
                return settings;
            });

            // Setup serial
            _serialTransport = new SerialTransport
            {
                CurrentSerialSettings = { 
                    PortName = Config.Instance.Serial.Port, 
                    BaudRate = Config.Instance.Serial.BuadRate,
                    DataBits = Config.Instance.Serial.DataBits,
                    StopBits = Config.Instance.Serial.StopBits,
                    Parity = Config.Instance.Serial.Parity,
                    DtrEnable = Config.Instance.Serial.DtrEnable,
                    Timeout = Config.Instance.General.Timeout 
                }
            };

            // Setup command messenger
            _cmdMessenger = new CmdMessenger(_serialTransport, BoardType.Bit16);

            // Setup handlers
            _cmdMessenger.Attach(OnUnknownCommand);

            foreach (var val in Enum.GetValues(typeof(EventType)))
            {
                _cmdMessenger.Attach((int)val, OnEventCommand);
            }

            // Setup dweet client
            _dweetClient = new DweetClient(Config.Instance.Dweet.Thing,
                Config.Instance.General.Timeout);
        }

        public override void Loop()
        {
            if (!_cmdMessenger.IsConnected()) // Ensure serial connection
            {
                if (_dweetClient.IsListeningForDweets())
                {
                    _dweetClient.StopListeningForDweets();
                }

                while (!_cmdMessenger.IsConnected())
                {
                    LogUtil.Info("Connecting to com port '" +  _serialTransport.CurrentSerialSettings.PortName + "'...");

                    var connected = _cmdMessenger.Connect();
                    if (!connected)
                    {
                        LogUtil.Error("Error opening port");
                        LogUtil.Info("Retrying in 5 seconds...");
                        Thread.Sleep(5000);
                    }
                    else
                    {
                        LogUtil.Success("Connected to com port '" + _serialTransport.CurrentSerialSettings.PortName + "'...");
                    }
                }
            }
            else if (!_dweetClient.IsListeningForDweets()) // Ensure dweet connection
            {
                while (!_dweetClient.IsListeningForDweets())
                {
                    LogUtil.Info("Connecting to dweet.io thing '" + _dweetClient.Thing + "'...");

                    var listening = _dweetClient.ListenForDweets(HandleDweet);
                    if (!listening)
                    {
                        LogUtil.Error("Error connecting to dweet.io");
                        LogUtil.Info("Retrying in 5 seconds...");
                        Thread.Sleep(5000);
                    }
                    else
                    {
                        LogUtil.Success("Connected to dweet.io thing '" + _dweetClient.Thing + "'...");
                    }
                }
            }
        }

        public override void Exit()
        {
            if (_cmdMessenger != null) {
                _cmdMessenger.Disconnect();
                _cmdMessenger.Dispose();
            }

            if (_serialTransport != null) { 
                _serialTransport.Dispose(); 
            }

            if (_dweetClient != null)
            {
                _dweetClient.StopListeningForDweets();
                _dweetClient.Dispose();
            }
        }

        public bool isReady()
        {
            return _cmdMessenger.IsConnected() && _dweetClient.IsListeningForDweets();
        }

        #region Dweet Handlers

        protected void HandleDweet(Dweet dweet)
        {
            if (isReady())
            {
                var entity = dweet.Content.As<Entity>();
                if (entity != null)
                {
                    switch (entity.Type)
                    {
                        case EntityType.Action:
                            var action = dweet.Content.As<ActionEntity>();
                            if (action.Params.ContainsKey("deviceId") && action.Params["deviceId"].ToString() == Config.Instance.Petduino.DeviceId)
                            {
                                HandleAction(action);
                            }
                            break;
                    }
                }
                else
                {
                    LogUtil.Warning("Received dweet is not a valid entity.");
                }
            }
        }

        protected void HandleAction(ActionEntity action)
        {
            if (isReady())
            {
                LogUtil.Info("[Action] Received action from dweet: " + action.Name.ToString());
                LogUtil.Info("[Action] Sending action to Petduino: " + action.Name.ToString());

                var cmd = action.Params.ContainsKey("value")
                    ? new SendCommand((int)action.Name, action.Params["value"].ToString())
                    : new SendCommand((int)action.Name);

                _cmdMessenger.SendCommand(cmd);
            }
        }
        
        #endregion

        #region CmdMessenger Handlers

        protected void OnUnknownCommand(ReceivedCommand arguments)
        {
            if (isReady())
            {
                LogUtil.Warning("Command without attached callback received: " + arguments.CmdId);
            }
        }

        protected void OnEventCommand(ReceivedCommand arguments)
        {
            if (isReady())
            {
                var evtType = (EventType)Enum.Parse(typeof(EventType), arguments.CmdId.ToString());

                LogUtil.Info("[Event] Received event from Petduino: " + evtType);
                LogUtil.Info("[Event] Sending event to dweet: " + evtType);

                // Create event entity
                var entity = new EventEntity
                {
                    Name = evtType,
                    Data = new Dictionary<string, object>
                {
                    { "deviceId", Config.Instance.Petduino.DeviceId }
                }
                };

                // Check for a value
                var value = arguments.ReadStringArg();
                if (value != null)
                {
                    entity.Data.Add("value", value);
                }

                // Send dweet
                _dweetClient.SendDweet(entity);
            }
        }

        #endregion
    }
}
