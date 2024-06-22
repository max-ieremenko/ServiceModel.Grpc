﻿// <copyright>
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace ServiceModel.Grpc.Filters.Internal;

[DebuggerDisplay("Count = {Count}")]
internal sealed class RequestContext : IRequestContextInternal
{
    private readonly MessageProxy _messageProxy;
    private readonly IStreamAccessor? _streamProxy;
    private object? _request;
    private object? _stream;

    public RequestContext(MessageProxy messageProxy, IStreamAccessor? streamProxy)
    {
        _messageProxy = messageProxy;
        _streamProxy = streamProxy;
    }

    public int Count => _messageProxy.Names.Length;

    public object? Stream
    {
        get => _stream;
        set => UpdateStream(value);
    }

    public object? this[string name]
    {
        get => _messageProxy.GetValue(_request!, _messageProxy.GetPropertyIndex(name));
        set => _messageProxy.SetValue(_request!, _messageProxy.GetPropertyIndex(name), value);
    }

    public object? this[int index]
    {
        get => _messageProxy.GetValue(_request!, index);
        set => _messageProxy.SetValue(_request!, index, value);
    }

    public (object? Request, object? Stream) GetRaw() => (_request, _stream);

    public void SetRaw(object? request, object? stream)
    {
        _request = request;
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

    private void UpdateStream(object? stream)
    {
        if (_streamProxy == null)
        {
            throw new NotSupportedException("The current method does not accept a client stream.");
        }

        if (stream == null)
        {
            throw new ArgumentNullException(nameof(stream), "The client stream cannot be null.");
        }

        _streamProxy.Validate(stream);
        _stream = stream;
    }
}