using System.Diagnostics;
using System.Reflection;

namespace ServiceModel.Grpc.Internal
{
    [DebuggerDisplay("{Error}")]
    internal sealed class MethodDescription
    {
        public MethodDescription(MethodInfo method, string error)
        {
            Method = method;
            Error = error;
        }

        public MethodInfo Method { get; }
        
        public string Error { get; }
    }
}