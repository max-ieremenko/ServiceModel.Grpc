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

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis;

public sealed class DelegatingDebugLogger : IDebugLogger
{
    private readonly Action<string[]> _logMessage;
    private readonly Action<string, string> _logFile;

    public DelegatingDebugLogger(Action<string[]> logMessage, Action<string, string> logFile)
    {
        _logMessage = logMessage;
        _logFile = logFile;
    }

    public void Log(params string[] messages) => _logMessage(messages);

    public void LogSource(string fileName, string sourceText) => _logFile(fileName, sourceText);
}