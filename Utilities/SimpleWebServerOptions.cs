using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class SimpleWebServerOptions : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        public void NotifyPropertyChanged(string name)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(name));
        }

        #endregion

        private bool _active = false;
        private int _port = 8080;
        private string _computerName = String.Empty;
        private string _ipAddress = String.Empty;
        private string _startButtonText = "Start";

        public SimpleWebServerOptions()
        {
            _computerName = Environment.MachineName;
            _ipAddress = GetIpAddress();
        }

        public bool Active
        {
            get
            {
                return _active;
            }
            set
            {
                _active = value;

                NotifyPropertyChanged("Active");
            }
        }
        
        public int Port
        {
            get
            {
                return _port;
            }
            set
            {
                _port = value;

                NotifyPropertyChanged("Port");
                NotifyPropertyChanged("Url");
                NotifyPropertyChanged("IpUrl");
            }
        }

        public string Url
        {
            get
            {
                return "http://" + _computerName + ":" + _port.ToString() + "/";
            }
        }

        public string IpUrl
        {
            get
            {
                return "http://" + _ipAddress + ":" + _port.ToString() + "/";
            }
        }

        public string StartButtonText
        {
            get
            {
                return _startButtonText;
            }
            set
            {
                _startButtonText = value;
                NotifyPropertyChanged("StartButtonText");

            }
        }

        private string GetIpAddress()
        {
            IPHostEntry host;
            string localIp = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIp = ip.ToString();
                    break;
                }
            }
            return localIp;
        }
    }
}