# ServiceModel.Grpc NerdbankMessagePackMarshaller example

.NET Framework 4.6.2 application based on `Grpc.Core.Channel` and `Grpc.Core.Server`.

#### Enable ServiceModel.Grpc source code generation

see [Client.csproj](Client/Client.csproj) and [Server/Server.csproj](Server/Server.csproj)

```xml
<ItemGroup>
    <PackageReference Include="ServiceModel.Grpc.Nerdbank.MessagePackMarshaller" />
</ItemGroup>
```

[Client/Services/GrpcServices.cs](Client/Services/GrpcServices.cs) and [Server/Services/GrpcServices.cs](Server/Services/GrpcServices.cs)

```cs
// instruct ServiceModel.Grpc.DesignTime to generate required code during the build process
[ExportGrpcService(...)] 
[ImportGrpcService(...)] 

// instruct ServiceModel.Grpc.Nerdbank.MessagePackMarshaller to generate required code during the build process
[NerdbankMessagePackDesignTimeExtension]
internal static partial class GrpcServices;
```

#### Set NerdbankMessagePackMarshaller as default marshaller

see [Client/Program.cs](Client/Program.cs) and [Server/ServerHost.cs](Server/ServerHost.cs)

```c#
services.AddServiceModelGrpcClientFactory((options, provider) =>
{
    options.MarshallerFactory = new NerdbankMessagePackMarshallerFactory(PolyTypes.TypeShapeProvider);
}

_server.Services.AddCalculator(options =>
{
    options.MarshallerFactory = new NerdbankMessagePackMarshallerFactory(PolyTypes.TypeShapeProvider);
}
```