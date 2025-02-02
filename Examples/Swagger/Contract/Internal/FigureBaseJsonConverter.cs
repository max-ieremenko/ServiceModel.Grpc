using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Contract.Internal;

/// <summary>
/// Some methods of IFigureService accept FigureBase as input parameter.
/// The converter helps JsonSerializer to deserialize FigureBase correctly.
/// JSON serialization is need only for ServiceModel.Grpc HTTP/1.1 JSON gateway for Swagger UI, button "Try it out"
/// </summary>
internal sealed class FigureBaseJsonConverter : JsonConverter<FigureBase>
{
    public override FigureBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        object result;
        using (var document = JsonDocument.ParseValue(ref reader))
        {
            var figureType = GetFigureConcreteType(document.RootElement);
            result = JsonSerializer.Deserialize(document.RootElement.GetRawText(), figureType, options)!;
        }

        return (FigureBase)result;
    }

    public override void Write(Utf8JsonWriter writer, FigureBase value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType());
    }

    private static Type GetFigureConcreteType(JsonElement element)
    {
        // it looks like Rectangle
        foreach (var item in element.EnumerateObject())
        {
            if (nameof(Rectangle.VertexLeftTop).Equals(item.Name, StringComparison.OrdinalIgnoreCase)
                || nameof(Rectangle.Width).Equals(item.Name, StringComparison.OrdinalIgnoreCase)
                || nameof(Rectangle.Height).Equals(item.Name, StringComparison.OrdinalIgnoreCase))
            {
                return typeof(Rectangle);
            }
        }

        // there are no other figures implemented, except Triangle
        return typeof(Triangle);
    }
}