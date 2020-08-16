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
using System.CodeDom.Compiler;
using System.IO;
using System.Text;

namespace ServiceModel.Grpc.DesignTime.Internal.CSharp
{
    internal sealed class CodeStringBuilder
    {
        private readonly StringBuilder _output;
        private readonly IndentedTextWriter _writer;

        public CodeStringBuilder()
        {
            _output = new StringBuilder();
            _writer = new IndentedTextWriter(new StringWriter(_output), "    ");
            _writer.Indent = 0;
        }

        public CodeStringBuilder AppendLine(string text)
        {
            _writer.WriteLine(text);
            return this;
        }

        public CodeStringBuilder AppendLine()
        {
            _writer.WriteLine();
            return this;
        }

        public CodeStringBuilder Append(string text)
        {
            _writer.Write(text);
            return this;
        }

        public CodeStringBuilder AppendCommaIf(bool condition)
        {
            if (condition)
            {
                Append(", ");
            }

            return this;
        }

        public CodeStringBuilder AppendFormat(string format, params object[] args)
        {
            _writer.Write(format, args);
            return this;
        }

        public IDisposable Indent() => new Indenter(this);

        public override string ToString()
        {
            _writer.Flush();
            return _output.ToString();
        }

        private sealed class Indenter : IDisposable
        {
            private readonly CodeStringBuilder _owner;

            public Indenter(CodeStringBuilder owner)
            {
                _owner = owner;
                owner._writer.Indent++;
            }

            public void Dispose()
            {
                _owner._writer.Indent--;
            }
        }
    }
}
