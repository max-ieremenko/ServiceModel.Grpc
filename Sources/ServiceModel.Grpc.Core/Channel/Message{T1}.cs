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

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1619

namespace ServiceModel.Grpc.Channel;

/// <summary>
/// This API supports ServiceModel.Grpc infrastructure and is not intended to be used directly from your code.
/// This API may change or be removed in future releases.
/// </summary>
public partial class Message<T1> : IMessage<T1>
{
    public T1? GetValue1() => Value1;

    public void SetValue1(T1? value) => Value1 = value;
}