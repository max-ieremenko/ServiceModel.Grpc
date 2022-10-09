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
using System.Reflection;
using Grpc.Core;
using Moq;
using NUnit.Framework;
using ServiceModel.Grpc.Internal;
using ServiceModel.Grpc.TestApi.Domain;
using Shouldly;

namespace ServiceModel.Grpc.TestApi;

public abstract class ClientBuilderGenericTestBase
{
    protected Func<IGenericContract<int, string>> Factory { get; set; } = null!;

    protected Mock<CallInvoker> CallInvoker { get; private set; } = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        CallInvoker = new Mock<CallInvoker>(MockBehavior.Strict);
    }

    [Test]
    public void Invoke()
    {
        TestOutput.WriteLine(GetClientInstanceMethod(nameof(IGenericContract<int, string>.Invoke)).Disassemble());

        CallInvoker.SetupBlockingUnaryCallInOut(3, "4", "34");

        Factory().Invoke(3, "4").ShouldBe("34");

        CallInvoker.VerifyAll();
    }

    [Test]
    public void BlockingCall()
    {
        TestOutput.WriteLine(GetClientInstanceMethod(nameof(IGenericContract<int, string>.BlockingCall)).Disassemble());

        CallInvoker.SetupBlockingUnaryCallInOut(
            3,
            "4",
            "34",
            method =>
            {
                method.Name.ShouldBe(nameof(IGenericContract<int, string>.BlockingCallAsync));
            });

        Factory().BlockingCall(3, "4").ShouldBe("34");

        CallInvoker.VerifyAll();
    }

    protected virtual MethodInfo GetClientInstanceMethod(string name)
    {
        return Factory().GetType().InstanceMethod(name);
    }
}