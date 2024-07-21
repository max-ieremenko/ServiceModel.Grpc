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

using System.CodeDom.Compiler;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CodeGenerators;

public sealed class CodeStringBuilder : ICodeStringBuilder
{
    private readonly StringBuilder _output;
    private readonly IndentedTextWriter _writer;

    public CodeStringBuilder()
    {
        _output = new StringBuilder();
        _writer = new IndentedTextWriter(new StringWriter(_output), IndentedTextWriter.DefaultTabString);
    }

    public int GetLength()
    {
        _writer.Flush();
        return _output.Length;
    }

    public ICodeStringBuilder AppendLine(string text)
    {
        _writer.WriteLine(text);
        return this;
    }

    public ICodeStringBuilder AppendLine()
    {
        _writer.WriteLine();
        return this;
    }

    public ICodeStringBuilder Append(string text)
    {
        _writer.Write(text);
        return this;
    }

    public ICodeStringBuilder AppendFormat(string format, params object[] args)
    {
        _writer.Write(format, args);
        return this;
    }

    public IDisposable Indent() => new Indenter(this);

    public string Clear()
    {
        _writer.Flush();
        var result = _output.ToString();
        _output.Clear();
        return result;
    }

    private sealed class Indenter : IDisposable
    {
        private readonly CodeStringBuilder _owner;

        public Indenter(CodeStringBuilder owner)
        {
            _owner = owner;
            owner._writer.Indent++;
        }

        public void Dispose() => _owner._writer.Indent--;
    }
}