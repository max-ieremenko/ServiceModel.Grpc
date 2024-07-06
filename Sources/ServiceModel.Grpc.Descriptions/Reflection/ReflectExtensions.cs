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

namespace ServiceModel.Grpc.Descriptions.Reflection;

internal static class ReflectExtensions
{
    public static bool ContainsAsyncEnumerable<TType>(this IReflect<TType> reflect, TType[] types)
    {
        for (var i = 0; i < types.Length; i++)
        {
            if (reflect.IsAsyncEnumerable(types[i]))
            {
                return true;
            }
        }

        return false;
    }

    public static bool ContainsAsyncEnumerable<TType>(this IReflect<TType> reflect, IParameterInfo<TType>[] parameters)
    {
        for (var i = 0; i < parameters.Length; i++)
        {
            if (reflect.IsAsyncEnumerable(parameters[i].Type))
            {
                return true;
            }
        }

        return false;
    }
}