// <copyright>
// Copyright 2021 Max Ieremenko
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

namespace ServiceModel.Grpc.Filters.Internal
{
    internal sealed class StreamProxy
    {
        private readonly Func<object, object> _streamCast;
        private readonly Func<object> _streamCreate;

        public StreamProxy(Type itemType)
        {
            (_streamCreate, _streamCast) = ProxyCompiler.GetStreamAccessors(itemType);
        }

        public void AssignValue(out object? target, object value)
        {
            target = _streamCast(value);
        }

        public object CreateDefault() => _streamCreate();
    }
}
