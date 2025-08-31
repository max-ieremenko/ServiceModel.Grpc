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

using ServiceModel.Grpc.Channel;

namespace ServiceModel.Grpc.Configuration.Internal;

internal static class MessageProperty
{
    public static class M1<T1>
    {
        public static T1? Get1(ref Message<T1> message) => message.Value1;

        public static void Set1(ref Message<T1> message, T1? value) => message.Value1 = value;
    }

    public static class M2<T1, T2>
    {
        public static T1? Get1(ref Message<T1, T2> message) => message.Value1;

        public static void Set1(ref Message<T1, T2> message, T1? value) => message.Value1 = value;

        public static T2? Get2(ref Message<T1, T2> message) => message.Value2;

        public static void Set2(ref Message<T1, T2> message, T2? value) => message.Value2 = value;
    }

    public static class M3<T1, T2, T3>
    {
        public static T1? Get1(ref Message<T1, T2, T3> message) => message.Value1;

        public static void Set1(ref Message<T1, T2, T3> message, T1? value) => message.Value1 = value;

        public static T2? Get2(ref Message<T1, T2, T3> message) => message.Value2;

        public static void Set2(ref Message<T1, T2, T3> message, T2? value) => message.Value2 = value;

        public static T3? Get3(ref Message<T1, T2, T3> message) => message.Value3;

        public static void Set3(ref Message<T1, T2, T3> message, T3? value) => message.Value3 = value;
    }
}