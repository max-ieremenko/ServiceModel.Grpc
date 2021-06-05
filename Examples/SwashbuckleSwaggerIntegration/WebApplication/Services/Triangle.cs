using System.Runtime.Serialization;

namespace WebApplication.Services
{
    [DataContract]
    public class Triangle : FigureBase
    {
        [DataMember]
        public Point Vertex1 { get; set; }

        [DataMember]
        public Point Vertex2 { get; set; }

        [DataMember]
        public Point Vertex3 { get; set; }
    }
}
