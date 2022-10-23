using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Contract;
using Grpc.Core;
using Microsoft.IdentityModel.Tokens;
using ServiceModel.Grpc.Client;

namespace Client;

public static class Program
{
    public static async Task Main(string[] args)
    {
        // WARN: insecure connection should be used only in development environments
        var channel = new Channel("localhost", 8080, ChannelCredentials.Insecure);

        await CallAsAnonymous(channel);
        await CallAsDemoUser(channel);

        if (Debugger.IsAttached)
        {
            Console.WriteLine("...");
            Console.ReadLine();
        }
    }

    private static async Task CallAsAnonymous(Channel channel)
    {
        var proxy = new ClientFactory().CreateClient<IDemoService>(channel);

        Console.WriteLine("Invoke PingAsync");
        var response = await proxy.PingAsync();
        Console.WriteLine(response);
    }

    private static async Task CallAsDemoUser(Channel channel)
    {
        // create token
        var authenticationToken = ResolveJwtToken("demo-user");

        var clientFactory = new ClientFactory(new ServiceModelGrpcClientOptions
        {
            // setup authorization header for all calls, by all proxies created by this factory
            DefaultCallOptionsFactory = () => new CallOptions(new Metadata
            {
                { "Authorization", "Bearer " + authenticationToken }
            })
        });

        var proxy = clientFactory.CreateClient<IDemoService>(channel);

        Console.WriteLine("Invoke GetCurrentUserNameAsync");
        var userName = await proxy.GetCurrentUserNameAsync();
        Console.WriteLine(userName);

        Console.WriteLine("Invoke PingAsync");
        var response = await proxy.PingAsync();
        Console.WriteLine(response);
    }

    private static string ResolveJwtToken(string userName)
    {
        var securityKey = new SymmetricSecurityKey(Guid.Empty.ToByteArray());
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = "Demo App",
            Expires = DateTime.UtcNow.AddHours(2),
            Subject = new ClaimsIdentity(new []
            {
                new Claim(ClaimTypes.Name, userName)
            }),
            SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(descriptor);
        return handler.WriteToken(token);
    }
}