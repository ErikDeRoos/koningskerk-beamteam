using ConnectTools;
using Microsoft.Practices.Unity;
using System;
using System.ServiceModel;

namespace RemoteGenerator.WCF
{
    class Host : IHost, IDisposable
    {
        internal static IUnityContainer DI;  // Dirty, maar WCF ondersteunt geen DI

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
            binding.TransferMode = TransferMode.Streamed;
            binding.MaxBufferSize = 65536;  // 64kb
            binding.MaxReceivedMessageSize = 67108864; // max 64mb
            binding.OpenTimeout = new TimeSpan(0, 1, 0);
            binding.CloseTimeout = new TimeSpan(0, 1, 0);
            binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
            binding.SendTimeout = new TimeSpan(0, 10, 0);
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
