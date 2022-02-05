using System;
using System.Threading.Tasks;
using Contract;
using Grpc.Core;
using ServiceModel.Grpc.Client;

namespace Client
{
    public sealed class ClientCalls
    {
        private readonly IClientFactory _clientFactory;
        private readonly Channel _channel;

        public ClientCalls(int serverPort)
        {
            _clientFactory = new ClientFactory();

            // register generated IGenericCalculator<int> proxy, see MyGrpcProxies
            _clientFactory.AddGenericCalculatorInt32Client();

            _channel = new Channel("localhost", serverPort, ChannelCredentials.Insecure);
        }

        public async Task InvokeGenericCalculator()
        {
            // create instance of GenericCalculatorInt32Client, see MyGrpcProxies
            var proxy = _clientFactory.CreateClient<IGenericCalculator<int>>(_channel);

            // POST /IGenericCalculator-Int32/Touch
            Console.WriteLine("Invoke Touch");
            var touchResponse = proxy.Touch();
            Console.WriteLine("  {0}", touchResponse);

            // POST /IGenericCalculator-Int32/GetRandomValue
            Console.WriteLine("Invoke GetRandomValue");
            var x = await proxy.GetRandomValue();
            var y = await proxy.GetRandomValue();
            Console.WriteLine("  X = {0}", x);
            Console.WriteLine("  Y = {0}", y);

            // POST /IGenericCalculator-Int32/Sum
            Console.WriteLine("Invoke Sum");
            var sumResponse = await proxy.Sum(x, y);
            Console.WriteLine("  {0} + {1} = {2}", x, y, sumResponse);

            // POST /IGenericCalculator-Int32/Multiply
            Console.WriteLine("Invoke Multiply");
            var multiplyResponse = await proxy.Multiply(x, y);
            Console.WriteLine("  {0} * {1} = {2}", x, y, multiplyResponse);
        }

        public async Task InvokeDoubleCalculator()
        {
            // proxy will be generated on-fly
            var proxy = _clientFactory.CreateClient<IDoubleCalculator>(_channel);

            // POST /IDoubleCalculator/Touch
            Console.WriteLine("Invoke Touch");
            var touchResponse = proxy.Touch();
            Console.WriteLine("  {0}", touchResponse);

            // POST /IDoubleCalculator/GetRandomValue
            Console.WriteLine("Invoke GetRandomValue");
            var x = await proxy.GetRandomValue();
            var y = await proxy.GetRandomValue();
            Console.WriteLine("  X = {0}", x);
            Console.WriteLine("  Y = {0}", y);

            // POST /IDoubleCalculator/Sum
            Console.WriteLine("Invoke Sum");
            var sumResponse = await proxy.Sum(x, y);
            Console.WriteLine("  {0} + {1} = {2}", x, y, sumResponse);

            // POST /IDoubleCalculator/Multiply
            Console.WriteLine("Invoke Multiply");
            var multiplyResponse = await proxy.Multiply(x, y);
            Console.WriteLine("  {0} * {1} = {2}", x, y, multiplyResponse);
        }
    }
}
