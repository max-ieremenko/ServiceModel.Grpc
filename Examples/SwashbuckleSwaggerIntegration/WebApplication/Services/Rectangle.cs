using System.Runtime.Serialization;

namespace WebApplication.Services
{
    [DataContract]
    public class Rectangle : FigureBase
    {
        [DataMember]
        public Point VertexLeftTop { get; set; }

        [DataMember]
        public int Width { get; set; }

        [DataMember]
        public int Height { get; set; }
    }
}
