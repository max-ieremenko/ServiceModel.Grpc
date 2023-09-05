// <copyright>
// Copyright 2021-2023 Max Ieremenko
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
using Microsoft.Extensions.Logging;

namespace ServiceModel.Grpc.AspNetCore.Internal.Swagger;

internal sealed class MockLogger : Microsoft.Extensions.Logging.ILogger
{
    public IList<string> Output { get; } = new List<string>();

#if NET7_0_OR_GREATER
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
#else
    public IDisposable BeginScope<TState>(TState state)
#endif
    {
        throw new NotSupportedException();
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        throw new NotSupportedException();
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Output.Add("{0}: {1}".FormatWith(logLevel, state));
    }
}