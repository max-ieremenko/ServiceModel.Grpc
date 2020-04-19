using System;
using System.Collections.Generic;

namespace ServiceModel.Grpc.Internal
{
    internal sealed class InterfaceDescription
    {
        public Type InterfaceType { get; set; }

        public IList<MethodDescription> Methods { get; } = new List<MethodDescription>();

        public IList<OperationDescription> Operations { get; } = new List<OperationDescription>();

        public IList<MethodDescription> NotSupportedOperations { get; } = new List<MethodDescription>();
    }
}
