using Microsoft.Extensions.DependencyInjection;

namespace Service;

public static class PersonModule
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<PersonService>();
        services.AddTransient<IPersonRepository, PersonRepository>();
    }
}