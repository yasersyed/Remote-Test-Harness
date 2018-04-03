using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CommChannel
{
    class Class1
    {
        IComm channel;
        public void CreateChannel(string url)
        {
            EndpointAddress baseAddress = new EndpointAddress(url);
            WSHttpBinding binding = new WSHttpBinding();
            ChannelFactory<IComm> factory = new ChannelFactory<IComm>(binding, url);
            channel = factory.CreateChannel();
            Console.WriteLine("Proxy Created for : " + url);
        }
    }
}
