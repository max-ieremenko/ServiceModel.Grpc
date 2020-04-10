using System;
using System.ServiceModel;

namespace ServiceModel.Grpc.Internal
{
    public partial class ServiceContractTest
    {
        [ServiceContract]
        public interface IServiceContract1
        {
            [OperationContract]
            void Empty();

            void Ignore();
        }

        [ServiceContract]
        public interface IServiceContract2 : IServiceContract1
        {
        }

        public interface IContract1 : IServiceContract1, IDisposable
        {
        }

        internal class ServiceContract1 : IServiceContract1
        {
            public void Empty() => throw new NotImplementedException();

            public void Ignore() => throw new NotImplementedException();
        }

        internal class ServiceContract2 : IServiceContract2
        {
            public void Empty() => throw new NotImplementedException();

            public void Ignore() => throw new NotImplementedException();
        }

        internal class Contract1 : IContract1
        {
            public void Empty() => throw new NotImplementedException();

            public void Ignore() => throw new NotImplementedException();

            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }
    }
}
