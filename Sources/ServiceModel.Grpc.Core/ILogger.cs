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

namespace ServiceModel.Grpc;

/// <summary>
/// Represents a type used to perform logging.
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Formats and writes an error log message.
    /// </summary>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="args">An objects array that contains zero or more objects to format.</param>
    void LogError(string message, params object[] args);

    /// <summary>
    /// Formats and writes a warning log message.
    /// </summary>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="args">An objects array that contains zero or more objects to format.</param>
    void LogWarning(string message, params object[] args);

    /// <summary>
    /// Formats and writes a debug log message.
    /// </summary>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="args">An objects array that contains zero or more objects to format.</param>
    void LogDebug(string message, params object[] args);
}