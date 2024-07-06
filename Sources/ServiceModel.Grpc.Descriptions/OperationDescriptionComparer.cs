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

using Grpc.Core;
using ServiceModel.Grpc.Descriptions.Reflection;

namespace ServiceModel.Grpc.Descriptions;

internal static class OperationDescriptionComparer
{
    public static bool IsCompatibleWith<TType>(this OperationDescription<TType> description, OperationDescription<TType> other, IReflect<TType> reflect)
    {
        if (description.OperationType != MethodType.Unary)
        {
            throw new NotImplementedException();
        }

        if (description.OperationType != other.OperationType
            || description.RequestType.Properties.Length != other.RequestType.Properties.Length
            || description.ResponseType.Properties.Length != other.ResponseType.Properties.Length)
        {
            return false;
        }

        for (var i = 0; i < description.RequestType.Properties.Length; i++)
        {
            var x = description.RequestType.Properties[i];
            var y = other.RequestType.Properties[i];
            if (!reflect.Equals(x, y))
            {
                return false;
            }
        }

        for (var i = 0; i < description.ResponseType.Properties.Length; i++)
        {
            var x = description.ResponseType.Properties[i];
            var y = other.ResponseType.Properties[i];
            if (!reflect.Equals(x, y))
            {
                return false;
            }
        }

        return true;
    }
}