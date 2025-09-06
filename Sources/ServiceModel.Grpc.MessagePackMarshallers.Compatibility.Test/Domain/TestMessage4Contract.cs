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

using ServiceModel.Grpc.Configuration;

#pragma warning disable ServiceModelGrpcInternalAPI

namespace ServiceModel.Grpc.Domain;

internal static class TestMessage4Contract
{
    public static void Register<T1, T2, T3, T4>()
    {
        if (!NerdbankMessagePackMarshaller.IsRegisteredMessage<TestMessage<T1, T2, T3, T4>>())
        {
            NerdbankMessagePackMarshaller.NewMessageShapeBuilder<TestMessage<T1, T2, T3, T4>>(4)
                .AddProperty(TestMessage<T1, T2, T3, T4>.Get1, TestMessage<T1, T2, T3, T4>.Set1)
                .AddProperty(TestMessage<T1, T2, T3, T4>.Get2, TestMessage<T1, T2, T3, T4>.Set2)
                .AddProperty(TestMessage<T1, T2, T3, T4>.Get3, TestMessage<T1, T2, T3, T4>.Set3)
                .AddProperty(TestMessage<T1, T2, T3, T4>.Get4, TestMessage<T1, T2, T3, T4>.Set4)
                .Register();
        }
    }
}