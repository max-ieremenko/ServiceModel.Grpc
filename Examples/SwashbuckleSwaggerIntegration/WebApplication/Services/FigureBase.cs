using System.Runtime.Serialization;

namespace WebApplication.Services
{
    [DataContract]
    [KnownType(typeof(Triangle))]
    [KnownType(typeof(Rectangle))]
    public abstract class FigureBase
    {
        [DataMember]
        public string Name { get; set; }
    }
}
