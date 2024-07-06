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

using Microsoft.Extensions.Logging;

namespace ServiceModel.Grpc.Client.DependencyInjection.Internal;

internal sealed class LogAdapter : ILogger
{
    private readonly Microsoft.Extensions.Logging.ILogger _logger;

    public LogAdapter(Microsoft.Extensions.Logging.ILogger logger)
    {
        _logger = logger;
    }

    public static ILogger? Wrap(Microsoft.Extensions.Logging.ILogger? logger) => logger == null ? null : new LogAdapter(logger);

    public void LogError(string message, params object?[] args) => _logger.LogError(message, args);

    public void LogWarning(string message, params object?[] args) => _logger.LogWarning(message, args);

    public void LogDebug(string message, params object?[] args) => _logger.LogDebug(message, args);
}