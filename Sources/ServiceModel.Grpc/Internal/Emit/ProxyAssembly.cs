using System;
using System.Reflection;
using System.Reflection.Emit;

namespace ServiceModel.Grpc.Internal.Emit
{
    internal static class ProxyAssembly
    {
        public static readonly object SyncRoot = new object();
        public static readonly ModuleBuilder DefaultModule = CreateModule("ServiceModel.Grpc.Proxy");

        private static ModuleBuilder CreateModule(string name)
        {
            var assemblyName = new AssemblyName(name);
            assemblyName.SetPublicKey(Convert.FromBase64String("ACQAAASAAACUAAAABgIAAAAkAABSU0ExAAQAAAEAAQDZAJwN9Rn4Om4Qwz1i4sVhZ0Mf9p6t9OEuXu8D/s9N1N4FKL8eh1REUxAnFNhK6lmIxEb5UAMWXBtJ3WQ4kVCo3FKIJAXX+MYemcRD+YLKkK/ZEHb3m3KwbQ7La456yxhoXh1+5XlA7StUhj3dW3jfDkWzOHhKlxSeOznDqBjb6g=="));

            return AssemblyBuilder
                .DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run)
                .DefineDynamicModule(name);
        }
    }
}
