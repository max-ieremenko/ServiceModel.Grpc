// <copyright>
// Copyright 2020-2022 Max Ieremenko
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
using System.Globalization;
using System.Runtime.Serialization;
using ServiceModel.Grpc.Channel;

namespace ServiceModel.Grpc.DesignTime.Generator.Internal.CSharp
{
    internal sealed class CSharpMessageBuilder : CodeGeneratorBase
    {
        private readonly int _propertiesCount;

        public CSharpMessageBuilder(int propertiesCount)
        {
            _propertiesCount = propertiesCount;
        }

        public override string GetGeneratedMemberName() => "Message_" + _propertiesCount.ToString(CultureInfo.InvariantCulture);

        protected override void Generate()
        {
            WriteMetadata();
            Output
                .AppendAttribute(typeof(SerializableAttribute))
                .AppendAttribute(typeof(DataContractAttribute), "Name = \"m\"", "Namespace = \"s\"")
                .Append("public sealed class ")
                .Append(nameof(Message))
                .Append("<");

            for (var i = 0; i < _propertiesCount; i++)
            {
                Output.AppendCommaIf(i != 0);
                Output.AppendFormat("T{0}", i + 1);
            }

            Output
                .AppendLine(">")
                .AppendLine("{");

            using (Output.Indent())
            {
                BuildCtorDefault();
                Output.AppendLine();

                BuildCtorFull();
                BuildProperties();
            }

            Output.AppendLine("}");
        }

        private void BuildCtorDefault()
        {
            Output
                .Append("public ")
                .Append(nameof(Message))
                .AppendLine("()")
                .AppendLine("{")
                .AppendLine("}");
        }

        private void BuildCtorFull()
        {
            Output
                .Append("public ")
                .Append(nameof(Message))
                .Append("(");

            for (var i = 0; i < _propertiesCount; i++)
            {
                Output
                    .AppendCommaIf(i != 0)
                    .AppendFormat("T{0} value{0}", i + 1);
            }

            Output
                .AppendLine(")")
                .AppendLine("{");

            using (Output.Indent())
            {
                for (var i = 0; i < _propertiesCount; i++)
                {
                    Output
                        .AppendFormat("Value{0} = value{0}", i + 1)
                        .AppendLine(";");
                }
            }

            Output.AppendLine("}");
        }

        private void BuildProperties()
        {
            for (var i = 0; i < _propertiesCount; i++)
            {
                var order = (i + 1).ToString(CultureInfo.InvariantCulture);
                Output
                    .AppendLine()
                    .AppendAttribute(typeof(DataMemberAttribute), string.Format("Name = \"v{0}\"", order), string.Format("Order = {0}", order))
                    .AppendLine()
                    .AppendFormat("public T{0} Value{0}", order)
                    .AppendLine(" { get; set; }");
            }
        }
    }
}
