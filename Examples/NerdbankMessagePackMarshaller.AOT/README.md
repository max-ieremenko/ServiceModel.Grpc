# ServiceModel.Grpc AOT example

#### Tell the trimming process that the `DataContractMarshaller` should be removed:

see [Client.csproj](Client/Client.csproj) and [Server/Server.csproj](Server/Server.csproj)

```xml
<ItemGroup>
    <RuntimeHostConfigurationOption Include="ServiceModel.Grpc.DisableDataContractMarshallerFactory" Value="true" Trim="true" />
</ItemGroup>
```

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

see [Client/Program.cs](Client/Program.cs) and [Server/Program.cs](Server/Program.cs)

```c#
services.AddServiceModelGrpcClientFactory((options, provider) =>
{
    options.MarshallerFactory = NerdbankMessagePackMarshallerFactory.CreateWithTypeShapeProviderFrom<Point>();
}

services.AddServiceModelGrpc(options =>
{
    options.DefaultMarshallerFactory = NerdbankMessagePackMarshallerFactory.CreateWithTypeShapeProviderFrom<Point>();
}
```

#### For [error handling](https://max-ieremenko.github.io/ServiceModel.Grpc/global-error-handling.html) override default serializers, they are not Trimming/AOT compatible

see [Client/Services/ClientFaultDetailDeserializer.cs](Client/Services/ClientFaultDetailDeserializer.cs) and [Server/Services/ServerFaultDetailSerializer.cs](Server/Services/ServerFaultDetailSerializer.cs)

```c#
// server
string SerializeDetailType(Type detailType) => nameof(InvalidRectangleError);

// client
Type DeserializeDetailType(string typePayload) => typeof(InvalidRectangleError);

// server
byte[] SerializeDetail(IMarshallerFactory marshallerFactory, object detail)
    => MarshallerExtensions.Serialize(marshallerFactory.CreateMarshaller<InvalidRectangleError>(), (InvalidRectangleError)detail);

// client
object DeserializeDetail(IMarshallerFactory marshallerFactory, Type detailType, byte[] detailPayload)
    => MarshallerExtensions.Deserialize(marshallerFactory.CreateMarshaller<InvalidRectangleError>(), detailPayload);
```

#### publish

```bash
dotnet publish NerdbankMessagePackMarshaller.AOT.slnx
```