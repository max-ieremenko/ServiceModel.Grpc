// <copyright>
// Copyright 2022-2024 Max Ieremenko
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

using ServiceModel.Grpc.DesignTime.CodeAnalysis.CodeGenerators;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.CodeGenerators;

internal static partial class CodeStringBuilderExtensions
{
    public static ICodeStringBuilder WriteString(this ICodeStringBuilder output, string value) =>
        output.Append("\"").Append(value).Append("\"");

    public static ICodeStringBuilder WriteBoolean(this ICodeStringBuilder output, bool value) =>
        output.Append(value ? "true" : "false");

    public static ICodeStringBuilder WriteCommaIf(this ICodeStringBuilder output, bool condition)
    {
        if (condition)
        {
            output.Append(", ");
        }

        return output;
    }
}