using System.Runtime.Serialization;

namespace Contract
{
    [DataContract]
    public class Point
    {
        public Point()
        {
        }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        [DataMember]
        public double X { get; set; }

        [DataMember]
        public double Y { get; set; }
    }
}
