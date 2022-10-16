﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Contract;
using Grpc.Core;
using ServiceModel.Grpc.Client;
using ServiceModel.Grpc.Configuration;

namespace Client
{
    public static class Program
    {
        private static readonly IClientFactory DefaultClientFactory = new ClientFactory(new ServiceModelGrpcClientOptions
        {
            // set ProtobufMarshaller as default Marshaller
            MarshallerFactory = ProtobufMarshallerFactory.Default
        });

        public static async Task Main(string[] args)
        {
            Console.WriteLine("Call ServerAspNetCore");
            await Run(new Channel("localhost", ServiceConfiguration.AspNetCorePort, ChannelCredentials.Insecure));

            Console.WriteLine("Call ServerSelfHost");
            await Run(new Channel("localhost", ServiceConfiguration.SelfHostPort, ChannelCredentials.Insecure));

            if (Debugger.IsAttached)
            {
                Console.WriteLine("...");
                Console.ReadLine();
            }
        }

        private static async Task Run(ChannelBase channel)
        {
            var personService = DefaultClientFactory.CreateClient<IPersonService>(channel);
            var person = await personService.CreatePerson("John X", DateTime.Today.AddYears(-20));

            Console.WriteLine("  Name: {0}", person.Name);
            Console.WriteLine("  BirthDay: {0}", person.BirthDay);
            Console.WriteLine("  CreatedBy: {0}", person.CreatedBy);
        }
    }
}
