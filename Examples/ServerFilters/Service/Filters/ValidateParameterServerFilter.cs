using System;
using System.Reflection;
using System.Threading.Tasks;
using Contract;
using ServiceModel.Grpc.Filters;

namespace Service.Filters
{
    public sealed class ValidateParameterFilterAttribute : ServerFilterRegistrationAttribute
    {
        public ValidateParameterFilterAttribute(int order)
            : base(order)
        {
        }

        public override IServerFilter CreateFilter(IServiceProvider serviceProvider)
        {
            return new ValidateParameterFilter();
        }
    }

    internal sealed class ValidateParameterFilter : IServerFilter
    {
        public ValueTask InvokeAsync(IServerFilterContext context, Func<ValueTask> next)
        {
            var result = new DivideByResult { IsSuccess = true };

            var parameters = context.ServiceMethodInfo.GetParameters();
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                foreach (var attribute in parameter.GetCustomAttributes<ValidateParameterAttribute>())
                {
                    attribute.Validate(context.Request[i], parameter.Name, result);
                }
            }

            if (!result.IsSuccess)
            {
                // skip the method: pass non-success result to the client
                context.Response[0] = result;
                return new ValueTask(Task.CompletedTask);
            }

            // call the method
            return next();
        }
    }
}
