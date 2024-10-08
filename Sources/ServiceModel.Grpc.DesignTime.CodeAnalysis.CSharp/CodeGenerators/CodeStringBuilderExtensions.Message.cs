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

using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.CodeGenerators;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.CodeGenerators;

internal static partial class CodeStringBuilderExtensions
{
    public static ICodeStringBuilder WriteMessage(this ICodeStringBuilder output, IMessageDescription message, string? ending = null) =>
        WriteMessageOrDefault(output, message, ending);

    public static ICodeStringBuilder WriteMessageOrDefault(this ICodeStringBuilder output, IMessageDescription? message, string? ending = null)
    {
        if (message == null || message.IsBuiltIn)
        {
            output.WriteType(typeof(Message));
        }
        else
        {
            output.Append(nameof(Message));
        }

        output.Append(ending ?? string.Empty);
        if (message?.Properties.Length > 0)
        {
            output.Append("<");
            for (var i = 0; i < message.Properties.Length; i++)
            {
                output
                    .WriteCommaIf(i > 0)
                    .WriteType(message.Properties[i]);
            }

            output.Append(">");
        }

        return output;
    }

    public static ICodeStringBuilder WriteGenericMessage(this ICodeStringBuilder output, int propertiesCount, string? ending = null)
    {
        output.Append(nameof(Message));
        output.Append(ending ?? string.Empty);

        if (propertiesCount > 0)
        {
            output.Append("<");
            for (var i = 0; i < propertiesCount; i++)
            {
                output
                    .WriteCommaIf(i > 0)
                    .AppendFormat("T{0}", i + 1);
            }

            output.Append(">");
        }

        return output;
    }
}