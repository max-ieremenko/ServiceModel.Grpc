using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Contract.Internal;

namespace Contract;

[DataContract]
[KnownType(typeof(Triangle))]
[KnownType(typeof(Rectangle))]
[JsonConverter(typeof(FigureBaseJsonConverter))]
public abstract class FigureBase
{
    public abstract double GetArea();
}