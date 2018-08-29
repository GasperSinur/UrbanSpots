using System;
using System.ServiceModel;

namespace UrbanSpots
{
    public static class WCFFunctions
    {
        private static readonly EndpointAddress EndPoint = new EndpointAddress("http://178.172.53.248:9607/UrbanSpotsWcf.svc");
        //private static readonly EndpointAddress EndPoint = new EndpointAddress("http://178.172.53.248:9608/UrbanSpotsWcf.svc");
        //private static readonly EndpointAddress EndPoint = new EndpointAddress("http://158.197.37.239:9607/UrbanSpotsWcf.svc");        

        private static BasicHttpBinding CreateBasicHttp()
        {
            BasicHttpBinding binding = new BasicHttpBinding
            {
                Name = "basicHttpBinding",
                MaxBufferSize = 21474830,
                MaxReceivedMessageSize = 21474830,
                MaxBufferPoolSize = 10000000,
                TransferMode = TransferMode.Buffered,
                ReaderQuotas = { MaxStringContentLength = 15000000, MaxArrayLength = 5000000 }
            };
            TimeSpan timeout = new TimeSpan(0, 0, 30);
            binding.SendTimeout = timeout;
            binding.OpenTimeout = timeout;
            binding.ReceiveTimeout = timeout;            
            return binding;
        }

        public static UrbanSpotsWcfClient InitializeWcfServiceClient()
        {
            BasicHttpBinding binding = WCFFunctions.CreateBasicHttp();
            
            UrbanSpotsWcfClient client = new UrbanSpotsWcfClient(binding, EndPoint);
            return client;
        }
    }
}