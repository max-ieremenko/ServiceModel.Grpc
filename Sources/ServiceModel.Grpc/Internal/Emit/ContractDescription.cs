using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Grpc.Core;

namespace ServiceModel.Grpc.Internal.Emit
{
    internal static class ContractDescription
    {
        public static IEnumerable<Type> GetInterfacesImplementation(Type interfaceType)
        {
            return ReflectionTools.ExpandInterface(interfaceType);
        }

        public static IEnumerable<MethodInfo> GetMethodsForImplementation(Type interfaceType)
        {
            return ReflectionTools.GetMethods(interfaceType);
        }

        public static bool IsOperationMethod(Type interfaceType, MethodInfo method)
        {
            return ServiceContract.IsServiceContractInterface(interfaceType)
                   && ServiceContract.IsServiceOperation(method);
        }

        public static bool TryCreateMessage(MethodInfo method, out MessageAssembler message, out string error)
        {
            message = default;
            error = default;

            try
            {
                message = new MessageAssembler(method);
                return true;
            }
            catch (NotSupportedException ex)
            {
                var text = new StringBuilder();
                Exception e = ex;
                while (e != null)
                {
                    if (text.Length > 0)
                    {
                        text.Append(" --> ");
                    }

                    text.Append(ex.Message);
                    e = e.InnerException;
                }

                error = text.ToString();
            }

            return false;
        }

        public static MethodInfo GetClientCallOptionsCombine(MessageAssembler message)
        {
            var parameters = new Type[message.ContextInput.Length + 1];
            for (var i = 0; i < message.ContextInput.Length; i++)
            {
                parameters[i + 1] = message.Parameters[message.ContextInput[i]].ParameterType;
            }

            parameters[0] = typeof(CallOptions?);

            MethodInfo method = null;
            try
            {
                method = typeof(ClientChannelAdapter).StaticMethod(nameof(ClientChannelAdapter.Combine), parameters);
            }
            catch (ArgumentOutOfRangeException)
            {
                // method not found
            }

            return method;
        }

        public static string GetServiceName(Type interfaceType) => ServiceContract.GetServiceName(interfaceType);

        public static string GetOperationName(MethodInfo method) => ServiceContract.GetServiceOperationName(method);

        public static MethodInfo GetServiceContextOption(Type optionType)
        {
            MethodInfo method = null;
            try
            {
                method = typeof(ServerChannelAdapter).StaticMethodByReturnType(nameof(ServerChannelAdapter.GetContext), optionType);
            }
            catch (ArgumentOutOfRangeException)
            {
                // method not found
            }

            return method;
        }
    }
}
