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

using System;
using System.Collections.Generic;

namespace ServiceModel.Grpc.Internal
{
    internal sealed class InterfaceDescription
    {
        public Type InterfaceType { get; set; }

        public IList<MethodDescription> Methods { get; } = new List<MethodDescription>();

        public IList<OperationDescription> Operations { get; } = new List<OperationDescription>();

        public IList<MethodDescription> NotSupportedOperations { get; } = new List<MethodDescription>();
    }
}
