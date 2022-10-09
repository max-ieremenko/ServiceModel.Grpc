// <copyright>
// Copyright 2022 Max Ieremenko
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
using System.Diagnostics;

namespace ServiceModel.Grpc.TestApi;

public static class TestOutput
{
    [Conditional("DEBUG")]
    public static void WriteLine() => Console.WriteLine();

    [Conditional("DEBUG")]
    public static void WriteLine(string? value) => Console.WriteLine(value);

    [Conditional("DEBUG")]
    public static void WriteLine(object? value) => Console.WriteLine(value);

    [Conditional("DEBUG")]
    public static void WriteLine(string format, object? arg0) => Console.WriteLine(format, arg0);
}