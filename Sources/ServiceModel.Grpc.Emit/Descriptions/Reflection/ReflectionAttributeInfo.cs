﻿// <copyright>
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

using ServiceModel.Grpc.Descriptions.Reflection;

namespace ServiceModel.Grpc.Emit.Descriptions.Reflection;

internal sealed class ReflectionAttributeInfo : IAttributeInfo
{
    public ReflectionAttributeInfo(Attribute source)
    {
        Source = source;
    }

    public Attribute Source { get; }

    public bool TryGetPropertyValue<T>(string propertyName, [NotNullWhen(true)] out T? value)
    {
        var candidate = Source.GetType().TryInstanceProperty(propertyName)?.GetValue(Source);
        if (candidate is T result)
        {
            value = result;
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetPropertyValues<TItem>(string propertyName, [NotNullWhen(true)] out IReadOnlyList<TItem>? value)
        => TryGetPropertyValue(propertyName, out value);
}