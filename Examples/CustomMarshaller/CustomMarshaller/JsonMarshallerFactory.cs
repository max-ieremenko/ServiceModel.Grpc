using System;
using System.Text.Json;
using Grpc.Core;
using ServiceModel.Grpc.Configuration;

namespace CustomMarshaller;

public sealed class JsonMarshallerFactory : IMarshallerFactory
{
    public static readonly IMarshallerFactory Default = new JsonMarshallerFactory();

    public JsonMarshallerFactory()
        : this(CreateDefaultOptions())
    {
    }

    public JsonMarshallerFactory(JsonSerializerOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        Options = options;
    }

    public JsonSerializerOptions Options { get; }

    public Marshaller<T> CreateMarshaller<T>()
    {
        return new Marshaller<T>(Serialize, Deserialize<T>);
    }

    private static JsonSerializerOptions CreateDefaultOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    private void Serialize<T>(T value, SerializationContext context)
    {
        var bufferWriter = context.GetBufferWriter();
        var jsonWriter = new Utf8JsonWriter(bufferWriter);

        JsonSerializer.Serialize(jsonWriter, value, Options);
        context.Complete();
    }

    private T Deserialize<T>(DeserializationContext context)
    {
        var jsonData = context.PayloadAsReadOnlySequence();
        var reader = new Utf8JsonReader(jsonData);
        return JsonSerializer.Deserialize<T>(ref reader, Options);
    }
}