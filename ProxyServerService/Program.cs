using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace ProxyServerService
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(serviceConfig => 
            {
                serviceConfig.Service<ProxyService>(serviceInstance =>
                {
                    serviceInstance
                        .ConstructUsing(() => new ProxyService())
                        .WhenStarted(execute => execute.Start())
                        .WhenStopped(execute => execute.Stop());
                });
                serviceConfig.SetServiceName("SampleProxyServer");
                serviceConfig.SetDisplayName("Sample Proxy Server");

                serviceConfig.StartManually();
            });
        }
    }
}
