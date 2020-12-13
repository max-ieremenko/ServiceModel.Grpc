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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ServiceModel.Grpc.DesignTime.Generator.Internal;

namespace ServiceModel.Grpc.DesignTime.Generator
{
    internal sealed class CodeGeneratorCache
    {
        private readonly IDictionary<string, HashSet<string>> _memberNamesByOwner = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        public bool AddNew(ClassDeclarationSyntax owner, string memberName)
        {
            var ownerName = owner.GetFullName();
            if (!_memberNamesByOwner.TryGetValue(ownerName, out var members))
            {
                members = new HashSet<string>(StringComparer.Ordinal);
                _memberNamesByOwner.Add(ownerName, members);
            }

            return members.Add(memberName);
        }
    }
}
