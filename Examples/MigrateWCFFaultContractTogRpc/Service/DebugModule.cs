using Contract;
using ServiceModel.Grpc.Interceptors;
using Unity;
using Unity.Lifetime;

namespace Service
{
    public static class DebugModule
    {
        public static void ConfigureContainer(IUnityContainer container)
        {
            container.RegisterType<IDebugService, DebugService>(new TransientLifetimeManager());
            container.RegisterType<DebugService>(new TransientLifetimeManager());

            container.RegisterType<IServerErrorHandler, FaultExceptionServerHandler>(new ContainerControlledLifetimeManager());
        }
    }
}
