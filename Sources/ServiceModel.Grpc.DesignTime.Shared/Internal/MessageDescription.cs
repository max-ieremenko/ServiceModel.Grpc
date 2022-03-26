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
using System.Text;
using ServiceModel.Grpc.Channel;

namespace ServiceModel.Grpc.DesignTime.Generator.Internal
{
    internal sealed class MessageDescription
    {
        public MessageDescription(string[] properties)
        {
            Properties = properties;
            ClassName = GetClassName(properties);
        }

        public string ClassName { get; }

        public string[] Properties { get; }

        internal static MessageDescription Empty() => new MessageDescription(Array.Empty<string>());

        private static string GetClassName(string[] properties)
        {
            var result = new StringBuilder(nameof(Message));
            if (properties.Length > 0)
            {
                result.Append("<");
                for (var i = 0; i < properties.Length; i++)
                {
                    if (i > 0)
                    {
                        result.Append(", ");
                    }

                    result.Append(properties[i]);
                }

                result.Append(">");
            }

            return result.ToString();
        }
    }
}
