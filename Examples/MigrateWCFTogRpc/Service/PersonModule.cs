using Contract;
using Unity;
using Unity.Lifetime;

namespace Service
{
    public static class PersonModule
    {
        public static void ConfigureContainer(IUnityContainer container)
        {
            container.RegisterType<IPersonService, PersonService>(new TransientLifetimeManager());
            container.RegisterType<PersonService>(new TransientLifetimeManager());

            container.RegisterType<IPersonRepository, PersonRepository>(new TransientLifetimeManager());
        }
    }
}
