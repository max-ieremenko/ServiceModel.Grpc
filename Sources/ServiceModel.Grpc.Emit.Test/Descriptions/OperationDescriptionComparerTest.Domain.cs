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

namespace ServiceModel.Grpc.Emit.Descriptions;

public partial class OperationDescriptionComparerTest
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    private sealed class CompatibleToAttribute : Attribute
    {
        public CompatibleToAttribute(string methodName)
        {
            MethodName = methodName;
        }

        public string MethodName { get; }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    private sealed class NotCompatibleToAttribute : Attribute
    {
        public NotCompatibleToAttribute(string methodName)
        {
            MethodName = methodName;
        }

        public string MethodName { get; }
    }

    private sealed class IsCompatibleToCases
    {
        [CompatibleTo(nameof(Empty1))]
        [CompatibleTo(nameof(Empty2))]
        [CompatibleTo(nameof(EmptyAsync1))]
        [CompatibleTo(nameof(EmptyAsync2))]
        [NotCompatibleTo(nameof(Empty3))]
        [NotCompatibleTo(nameof(Empty4))]
        public void Empty() => throw new NotSupportedException();

        public void Empty1() => throw new NotSupportedException();

        public void Empty2(CancellationToken token, CallContext context) => throw new NotSupportedException();

        public string Empty3() => throw new NotSupportedException();

        public void Empty4(string x) => throw new NotSupportedException();

        public Task EmptyAsync1() => throw new NotSupportedException();

        public ValueTask EmptyAsync2(CallContext context) => throw new NotSupportedException();

        [CompatibleTo(nameof(Data1))]
        [CompatibleTo(nameof(Data2))]
        [NotCompatibleTo(nameof(Data3))]
        [NotCompatibleTo(nameof(Data4))]
        public string Data(int x, string y) => throw new NotSupportedException();

        public Task<string?> Data1(int x, string y) => throw new NotSupportedException();

        public ValueTask<string> Data2(int x, string? y) => throw new NotSupportedException();

        public Task Data3(int x, string y) => throw new NotSupportedException();

        public string Data4(int? x, string y) => throw new NotSupportedException();
    }
}