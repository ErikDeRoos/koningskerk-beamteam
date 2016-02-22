using ConnectTools;
using System;
using System.ServiceModel;

namespace RemoteGenerator.WCF
{
    class Host : IHost, IDisposable
    {
        private string _address;
        private ServiceHost _serviceHost;

        public Host(string address)
        {
            _address = address;
        }

        public void Start()
        {
            if (_serviceHost != null)
                return;
            var baseAddress = new Uri(_address);
            lock (this)
            {
                if (_serviceHost != null)
                    return;
                _serviceHost = new ServiceHost(typeof(WCFServer), baseAddress);
            }
            var binding = new NetTcpBinding();
            _serviceHost.AddServiceEndpoint(typeof(IWCFServer), binding, baseAddress);
            _serviceHost.Open();
        }

        public void Stop()
        {
            if (_serviceHost == null)
                return;
            lock (this)
            {
                if (_serviceHost == null)
                    return;
                _serviceHost.Close();
                _serviceHost = null;
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
