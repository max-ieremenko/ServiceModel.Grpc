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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ServiceModel.Grpc.Descriptions.Reflection;

namespace ServiceModel.Grpc.Descriptions;

internal readonly ref struct InterfaceTree<TType>
{
    private readonly IReflect<TType> _reflect;

    public InterfaceTree(TType rootType, IReflect<TType> reflect)
    {
        _reflect = reflect;
        Services = new();

        var interfaces = reflect.GetInterfaces(rootType).ToList();
        ExtractServiceContracts(interfaces);
        ExtractAttachedContracts(interfaces);
        Interfaces = interfaces;
    }

    public List<(string ServiceName, TType ServiceType)> Services { get; }

    public List<TType> Interfaces { get; }

    private void ExtractServiceContracts(List<TType> interfaces)
    {
        for (var i = 0; i < interfaces.Count; i++)
        {
            var interfaceType = interfaces[i];
            if (!_reflect.TryGetServiceName(interfaceType, out var serviceName))
            {
                continue;
            }

            Services.Add((serviceName, interfaceType));
            interfaces.RemoveAt(i);
            i--;
        }
    }

    private void ExtractAttachedContracts(List<TType> interfaces)
    {
        // take into account only ServiceContracts
        var servicesIndex = Services.Count;

        for (var i = 0; i < interfaces.Count; i++)
        {
            var interfaceType = interfaces[i];
            if (!ContainsOperation(interfaceType) || !TryFindParentService(interfaceType, servicesIndex, out var serviceName))
            {
                continue;
            }

            Services.Add((serviceName, interfaceType));
            interfaces.RemoveAt(i);
            i--;
        }
    }

    private bool ContainsOperation(TType type)
    {
        var methods = _reflect.GetMethods(type);
        for (var i = 0; i < methods.Length; i++)
        {
            if (methods[i].TryGetOperationName(out _))
            {
                return true;
            }
        }

        return false;
    }

    private bool TryFindParentService(TType interfaceType, int servicesIndex, [NotNullWhen(true)] out string? serviceName)
    {
        serviceName = null;
        TType? parent = default;

        for (var i = 0; i < servicesIndex; i++)
        {
            var test = Services[i];
            if (!_reflect.IsAssignableFrom(interfaceType, test.ServiceType))
            {
                continue;
            }

            if (parent == null || _reflect.IsAssignableFrom(parent, test.ServiceType))
            {
                parent = test.ServiceType;
                serviceName = test.ServiceName;
            }
        }

        return parent != null;
    }
}