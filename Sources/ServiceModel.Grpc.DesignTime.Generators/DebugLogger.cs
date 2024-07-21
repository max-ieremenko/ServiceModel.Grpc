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

using Microsoft.CodeAnalysis.Diagnostics;

namespace ServiceModel.Grpc.DesignTime.Generators;

internal sealed class DebugLogger
{
    private const string PropertyLogFileName = "build_property.servicemodelgrpcdesigntime_debuglogfilename";
    private const string PropertyGeneratedOutput = "build_property.servicemodelgrpcdesigntime_debuggeneratedoutput";

    private readonly string _fileName;
    private readonly string? _sourceOutput;

    public DebugLogger(string fileName, string? sourceOutput)
    {
        _fileName = fileName;
        _sourceOutput = sourceOutput;
    }

    public static DebugLogger? Create(AnalyzerConfigOptions globalOptions)
    {
        if (!globalOptions.TryGetValue(PropertyLogFileName, out var fileName)
            || string.IsNullOrEmpty(fileName))
        {
            return null;
        }

        globalOptions.TryGetValue(PropertyGeneratedOutput, out var sourceOutput);

        var logger = new DebugLogger(fileName, string.IsNullOrEmpty(sourceOutput) ? null : sourceOutput);

        ////var field = globalOptions
        ////    .GetType()
        ////    .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
        ////    .First(i => i.FieldType == typeof(ImmutableDictionary<string, string>));

        ////var dict = (ImmutableDictionary<string, string>)field.GetValue(globalOptions);
        ////foreach (var pair in dict)
        ////{
        ////    logger.Log($"{pair.Key} = {pair.Value}");
        ////}

        return logger;
    }

    public void Log(params string[] messages) => File.AppendAllLines(_fileName, messages);

    public void LogSource(string fileName, string sourceText)
    {
        if (_sourceOutput != null)
        {
            Directory.CreateDirectory(_sourceOutput);
            File.WriteAllText(Path.Combine(_sourceOutput, fileName), sourceText);
        }
    }
}