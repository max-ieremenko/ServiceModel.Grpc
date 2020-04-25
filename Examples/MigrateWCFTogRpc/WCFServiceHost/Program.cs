using System;
using System.Linq;
using System.ServiceModel.Channels;
using Contract;
using Service;
using Unity;
using Unity.Wcf;

namespace WCFServiceHost
{
    public static class Program
    {
        public static void Main()
        {
            using (var container = new UnityContainer())
            using (var host = new UnityServiceHost(container, typeof(PersonService), new Uri(SharedConfiguration.WCFPersonServiceLocation)))
            {
                PersonModule.ConfigureContainer(container);
                OpenHost(host);

                Console.WriteLine("WCF host is listening {0}", host.BaseAddresses.First());
                Console.WriteLine("Press enter to exit...");
                Console.ReadLine();
            }
        }

        private static void OpenHost(CommunicationObject host)
        {
            // if receive System.ServiceModel.AddressAccessDeniedException on start-up
            // re-start your visual studio with administrator permissions "Run as administrator"
            host.Open();
        }
    }
}
