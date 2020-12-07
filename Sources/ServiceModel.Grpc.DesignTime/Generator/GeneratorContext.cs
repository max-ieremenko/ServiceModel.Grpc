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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace ServiceModel.Grpc.DesignTime.Generator
{
    // mostly workaround intellisense issue, see .props file
    internal sealed class GeneratorContext
    {
        private readonly GeneratorExecutionContext _executionContext;
        private readonly string? _intermediateOutputPath;
        private readonly HashSet<string> _filesToDelete;

        public GeneratorContext(GeneratorExecutionContext executionContext)
        {
            _executionContext = executionContext;
            _intermediateOutputPath = GetIntermediateOutputPath(executionContext);

            _filesToDelete = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrEmpty(_intermediateOutputPath) && Directory.Exists(_intermediateOutputPath))
            {
                var files = Directory.GetFiles(_intermediateOutputPath!, "*" + GetOutFileExtension(executionContext));
                for (var i = 0; i < files.Length; i++)
                {
                    _filesToDelete.Add(Path.GetFileName(files[i]));
                }
            }
        }

        public CancellationToken CancellationToken => _executionContext.CancellationToken;

        public static bool LaunchDebugger(GeneratorExecutionContext context)
        {
            if (!context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.servicemodelgrpcdesigntime_launchdebugger", out var value)
                || string.IsNullOrEmpty(value))
            {
                return false;
            }

            return bool.Parse(value);
        }

        public void AddOutput(ClassDeclarationSyntax node, string hintName, SourceText source)
        {
            var fileName = GetOutputFileName(_executionContext, node, hintName);

            if (!string.IsNullOrEmpty(_intermediateOutputPath))
            {
                Directory.CreateDirectory(_intermediateOutputPath!);

                var outFileName = Path.Combine(_intermediateOutputPath!, fileName);
                using (var writer = new StreamWriter(outFileName, false, GetOutEncoding()))
                {
                    source.Write(writer);
                }

                _filesToDelete.Remove(fileName);
            }

            _executionContext.AddSource(fileName, WithEncoding(source));
        }

        public void CleanUp()
        {
            if (_executionContext.CancellationToken.IsCancellationRequested
                || _filesToDelete.Count == 0
                || string.IsNullOrEmpty(_intermediateOutputPath))
            {
                return;
            }

            foreach (var file in _filesToDelete)
            {
                var fileName = Path.Combine(_intermediateOutputPath!, file);
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
        }

        internal static string GetOutputFileName(GeneratorExecutionContext context, ClassDeclarationSyntax node, string hintName)
        {
            var result = new StringBuilder()
                .Append(node.Identifier.WithoutTrivia().ToString())
                .Append(".")
                .Append(GetOutputFileNameIdentifier(node, hintName))
                .Append(GetOutFileExtension(context))
                .ToString();
            return result.ToLowerInvariant();
        }

        private static string? GetIntermediateOutputPath(GeneratorExecutionContext context)
        {
            context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.servicemodelgrpcdesigntime_designtime", out var designTime);
            context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.intermediateoutputpath", out var intermediatePath);
            context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.projectdir", out var projectDir);

            if (string.IsNullOrEmpty(designTime)
                || !bool.Parse(designTime!)
                || string.IsNullOrEmpty(intermediatePath)
                || string.IsNullOrEmpty(projectDir))
            {
                return null;
            }

            if (Path.IsPathRooted(intermediatePath))
            {
                return intermediatePath;
            }

            if (!Path.IsPathRooted(projectDir))
            {
                return null;
            }

            return Path.Combine(projectDir!, intermediatePath!);
        }

        private static string GetOutFileExtension(GeneratorExecutionContext context)
        {
            context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.servicemodelgrpcdesigntime_csextension", out var value);
            return string.IsNullOrEmpty(value) ? ".smgrpcdtg.cs" : value!;
        }

        private static SourceText WithEncoding(SourceText text)
        {
            if (text.Encoding != null)
            {
                return text;
            }

            // Warning CS8785 Generator failed to generate source. It will not contribute to the output and compilation errors may occur as a result.
            // Exception was of type 'ArgumentException' with message 'The provided SourceText must have an explicit encoding set.
            var source = new StringBuilder();
            using (var writer = new StringWriter(source))
            {
                text.Write(writer);
            }

            return SourceText.From(new StringReader(source.ToString()), source.Length, Encoding.UTF8);
        }

        private static Encoding GetOutEncoding() => Encoding.UTF8;

        private static string GetOutputFileNameIdentifier(ClassDeclarationSyntax node, string hintName)
        {
            StringBuilder result;

            using (var sha1 = System.Security.Cryptography.SHA1.Create())
            {
                var name = node.GetFullName() + "." + hintName;
                var hash = sha1.ComputeHash(GetOutEncoding().GetBytes(name));
                result = new StringBuilder(Convert.ToBase64String(hash));
            }

            // ArgumentException : The hintName contains an invalid character '=' at position 37. (Parameter 'hintName')
            result
                .Replace('=', '-')
                .Replace('+', '-')
                .Replace('/', '-');

            return result.ToString();
        }
    }
}
