// <copyright>
// Copyright Max Ieremenko
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Reflection;

namespace ServiceModel.Grpc.Filters.Internal;

internal static class ReflectionTools
{
    public static MethodInfo ImplementationOfMethod(Type instance, MethodInfo method)
    {
        var methodDeclaringType = method.DeclaringType;
        if (methodDeclaringType != null)
        {
            var map = instance.GetInterfaceMap(methodDeclaringType);
            for (var i = 0; i < map.InterfaceMethods.Length; i++)
            {
                if (map.InterfaceMethods[i].Equals(method))
                {
                    return map.TargetMethods[i];
                }
            }
        }

        throw new ArgumentOutOfRangeException($"Implementation of method {methodDeclaringType?.Name}.{method.Name} not found in {instance.FullName}.");
    }
}