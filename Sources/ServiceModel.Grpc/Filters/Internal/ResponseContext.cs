// <copyright>
// Copyright 2021 Max Ieremenko
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace ServiceModel.Grpc.Filters.Internal
{
    [DebuggerDisplay("Count = {Count}")]
    internal sealed class ResponseContext : IResponseContextInternal
    {
        private readonly MessageProxy _messageProxy;
        private readonly StreamProxy? _streamProxy;
        private object? _response;
        private object? _stream;

        public ResponseContext(MessageProxy messageProxy, StreamProxy? streamProxy)
        {
            _messageProxy = messageProxy;
            _streamProxy = streamProxy;
        }

        public int Count => _messageProxy.Names.Length;

        public object? Stream
        {
            get => SafeGetStream();
            set => UpdateStream(value);
        }

        public bool IsProvided { get; private set; }

        public object? this[string name]
        {
            get => _messageProxy.GetValue(SafeGetResponse(), _messageProxy.GetPropertyIndex(name));
            set => _messageProxy.SetValue(SafeGetResponse(), _messageProxy.GetPropertyIndex(name), value);
        }

        public object? this[int index]
        {
            get => _messageProxy.GetValue(SafeGetResponse(), index);
            set => _messageProxy.SetValue(SafeGetResponse(), index, value);
        }

        public (object Response, object? Stream) GetRaw() => (SafeGetResponse(), Stream);

        public void SetRaw(object? response, object? stream)
        {
            IsProvided = true;
            _response = response;
            _stream = stream;
        }

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            for (var i = 0; i < _messageProxy.Names.Length; i++)
            {
                var name = _messageProxy.Names[i];
                var value = this[i];
                yield return new KeyValuePair<string, object?>(name, value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private object SafeGetResponse()
        {
            if (_response == null)
            {
                _response = _messageProxy.CreateDefault();
            }

            return _response;
        }

        private object? SafeGetStream()
        {
            if (_stream == null && _streamProxy != null)
            {
                _stream = _streamProxy.CreateDefault();
            }

            return _stream;
        }

        private void UpdateStream(object? stream)
        {
            if (_streamProxy == null)
            {
                throw new NotSupportedException("The current method does not return server stream.");
            }

            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream), "The server stream cannot be null.");
            }

            _streamProxy.AssignValue(out _stream, stream);
        }
    }
}
