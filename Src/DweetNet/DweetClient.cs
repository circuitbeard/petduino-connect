using DweetNet.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DweetNet
{
    public class DweetClient : IDisposable
    {
        private Action<Dweet> _dweetHandler;

        private WebClient _webClient;

        private string _thing;
        private int _timeout;
        private bool _isListening;

        public string Thing
        {
            get { return _thing; }
        }

        public DweetClient(string thing, int timeout)
        {
            _thing = thing;
            _timeout = timeout;
            _isListening = false;

            _webClient = new WebClient();
        }

        public bool ListenForDweets(Action<Dweet> dweetHandler)
        {
            _dweetHandler = dweetHandler;

            _webClient.OpenReadCompleted += WebClient_OpenReadCompleted;
            _webClient.OpenReadAsync(new Uri("https://dweet.io/listen/for/dweets/from/" + _thing));

            // If web client is busy, then connection is open
            _isListening = _webClient.IsBusy;

            return _isListening;
        }

        public void StopListeningForDweets()
        {
            StopListeningForDweets(false);
        }

        protected void StopListeningForDweets(bool stillListening)
        {
            _webClient.OpenReadCompleted -= WebClient_OpenReadCompleted;
            _webClient.CancelAsync();
            _isListening = stillListening;
        }

        public bool IsListeningForDweets()
        {
            return _isListening;
        }

        public void SendDweet(object data)
        {
            var url = "https://dweet.io/dweet/for/" + _thing;
            var payload = JsonConvert.SerializeObject(data);

            using (var wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                wc.UploadString(url, payload);
            }
        }

        protected void WebClient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                    throw e.Error;

                using (var reader = new StreamReader(e.Result))
                {
                    while (!reader.EndOfStream)
                    {
                        try
                        {
                            var ln = reader.ReadLine();

                            // Starts with a quote so assume it's a JSON encoded string
                            if (ln.StartsWith("\""))
                            {
                                ln = JsonConvert.DeserializeObject<string>(ln);
                            }

                            // Json object so parse as a dweet
                            if (ln.StartsWith("{") || ln.StartsWith("["))
                            {
                                var dweet = JsonConvert.DeserializeObject<Dweet>(ln);
                                if (_dweetHandler != null)
                                {
                                    _dweetHandler(dweet);
                                }
                            }

                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                var autoReconnect = true;

                // If request was caneled, we don't want to reconnect
                var webEx = ex as WebException;
                if(webEx != null && webEx.Status == WebExceptionStatus.RequestCanceled)
                {
                    autoReconnect = false;
                }

                StopListeningForDweets(true);
                if (autoReconnect)
                {
                    ListenForDweets(_dweetHandler);
                }
            }
        }

        public void Dispose()
        {
            StopListeningForDweets();
            _webClient.Dispose();
        }
    }
}
