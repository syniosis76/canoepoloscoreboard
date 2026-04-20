using System;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace Utilities
{
    public class SimpleWebServerOptions : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
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
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n =>
                    n.OperationalStatus == OperationalStatus.Up &&
                    (n.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                    n.NetworkInterfaceType == NetworkInterfaceType.Ethernet) &&
                    !n.Description.ToLower().Contains("virtual") &&
                    !n.Description.ToLower().Contains("vpn"));

            foreach (var ni in interfaces)
            {
                var ipProps = ni.GetIPProperties();

                // Must have a gateway to be considered "active"
                if (!ipProps.GatewayAddresses.Any(g => g.Address.AddressFamily == AddressFamily.InterNetwork))
                    continue;

                var address = ipProps.UnicastAddresses
                    .FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork);

                if (address != null)
                {
                    return address.Address.ToString();
                }
            }

            return "Unknown";
        }
    }
}