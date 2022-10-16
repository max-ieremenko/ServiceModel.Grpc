using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading.Tasks;
using Contract;

namespace WCFClient
{
    public static class Program
    {
        public static async Task Main()
        {
            using (var factory = new ChannelFactory<IPersonService>(new BasicHttpBinding(), SharedConfiguration.WCFPersonServiceLocation))
            {
                var proxy = factory.CreateChannel();
                await CallGet(proxy);
                await CallGetAll(proxy);
            }

            if (Debugger.IsAttached)
            {
                Console.WriteLine("...");
                Console.ReadLine();
            }
        }

        private static async Task CallGet(IPersonService proxy)
        {
            Console.WriteLine("WCF Get person by id = 1");

            var person = await proxy.Get(1);
            Console.WriteLine("  {0}", person);

            Console.WriteLine("WCF Get person by id = 0");

            person = await proxy.Get(0);
            Trace.Assert(person == null);
            Console.WriteLine("  person not found, id=0");
        }

        private static async Task CallGetAll(IPersonService proxy)
        {
            Console.WriteLine("WCF Get all persons");

            var persons = await proxy.GetAll();
            foreach (var person in persons)
            {
                Console.WriteLine("  {0}", person);
            }
        }
    }
}
