﻿// <copyright>
// Copyright 2020 Max Ieremenko
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
using System.Text;

namespace ServiceModel.Grpc.Internal;

internal static class NamingContract
{
    public static string GetServiceName(
        string serviceTypeName,
        string? serviceContractAttributeNamespace,
        string? serviceContractAttributeName,
        IList<string> serviceGenericEnding)
    {
        var result = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(serviceContractAttributeNamespace))
        {
            result
                .Append(serviceContractAttributeNamespace)
                .Append('.');
        }

        if (string.IsNullOrWhiteSpace(serviceContractAttributeName))
        {
            result.Append(serviceTypeName);
        }
        else
        {
            result.Append(serviceContractAttributeName);
        }

        for (var i = 0; i < serviceGenericEnding.Count; i++)
        {
            result
                .Append('-')
                .Append(serviceGenericEnding[i]);
        }

        return result.ToString();
    }
}