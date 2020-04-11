using System;
using System.ComponentModel;

namespace ServiceModel.Grpc.Internal
{
    public partial class ReflectionToolsTest
    {
        private interface I1
        {
            string Overload();

            string Overload(int x);
        }

        private interface I2
        {
            string Overload(int x);
        }

        private sealed class Implementation : I1, I2
        {
            [Description("I1.Overload")]
            public string Overload() => throw new NotImplementedException();

            [Description("I1.Overload(int)")]
            public string Overload(int x) => throw new NotImplementedException();

            [Description("I2.Overload(int)")]
            string I2.Overload(int x) => throw new NotImplementedException();
        }
    }
}
