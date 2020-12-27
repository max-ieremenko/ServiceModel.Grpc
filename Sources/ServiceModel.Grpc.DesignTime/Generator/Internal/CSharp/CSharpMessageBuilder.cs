// <copyright>
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

using System.Globalization;
using System.Text;
using ServiceModel.Grpc.Channel;

namespace ServiceModel.Grpc.DesignTime.Generator.Internal.CSharp
{
    internal sealed class CSharpMessageBuilder : CodeGeneratorBase
    {
        private readonly MessageDescription _description;

        public CSharpMessageBuilder(MessageDescription description)
        {
            _description = description;
        }

        public override string GetGeneratedMemberName() => "Message_" + _description.Properties.Length.ToString(CultureInfo.InvariantCulture);

        protected override void Generate()
        {
            Output
                .AppendLine("[Serializable]")
                .AppendLine("[DataContract(Name = \"m\", Namespace = \"s\")]")
                .Append("internal sealed class ")
                .Append(nameof(Message))
                .Append("<");

            for (var i = 0; i < _description.Properties.Length; i++)
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

        private static (string Alias, string Name) CreateFlag(string ownerFullName, MessageDescription description)
        {
            var aliasName = "__message{0}".FormatWith(description.Properties.Length);

            var typeName = new StringBuilder(ownerFullName)
                .Append(".")
                .Append(nameof(Message))
                .Append("<");

            for (var i = 0; i < description.Properties.Length; i++)
            {
                if (i != 0)
                {
                    typeName.Append(", ");
                }

                typeName.Append("object");
            }

            typeName.Append(">");

            return (aliasName, typeName.ToString());
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

            for (var i = 0; i < _description.Properties.Length; i++)
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
                for (var i = 0; i < _description.Properties.Length; i++)
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
            for (var i = 0; i < _description.Properties.Length; i++)
            {
                Output
                    .AppendLine()
                    .AppendFormat("[DataMember(Name = \"v{0}\", Order = {0})]", i + 1)
                    .AppendLine()
                    .AppendFormat("public T{0} Value{0}", i + 1)
                    .AppendLine(" { get; set; }");
            }
        }
    }
}
