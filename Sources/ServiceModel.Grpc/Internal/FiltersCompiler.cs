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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ServiceModel.Grpc.Emit;
using TMessageGet = System.Func<object, int, object?>;
using TMessageSet = System.Action<object, int, object?>;
using TStreamCast = System.Func<object, object>;
using TStreamCreate = System.Func<object>;

namespace ServiceModel.Grpc.Internal;

internal static class FiltersCompiler
{
    private static readonly ConcurrentDictionary<Type, (TMessageGet Get, TMessageSet Set)> MessageAccessorByType = new();

    private static readonly ConcurrentDictionary<Type, (TStreamCreate Create, TStreamCast Cast)> StreamAccessorByType = new();

    public static (TMessageGet GetValue, TMessageSet SetValue) GetMessageAccessors(Type messageType) =>
        MessageAccessorByType.GetOrAdd(messageType, BuildAccessors);

    public static (TStreamCreate CreateDefault, TStreamCast Cast) GetStreamAccessors(Type itemType) =>
        StreamAccessorByType.GetOrAdd(itemType, BuildStreamAccessors);

    private static (TMessageGet Get, TMessageSet Set) BuildAccessors(Type messageType)
    {
        var valuesCount = messageType.GenericTypeArguments.Length;
        if (valuesCount == 0)
        {
            return (GetMessageProperty, SetMessageProperty);
        }

        var messageArg = Expression.Parameter(typeof(object));
        var valueArg = Expression.Parameter(typeof(object));
        var indexArg = Expression.Parameter(typeof(int));
        var typedMessage = Expression.Convert(messageArg, messageType);

        var getCases = new SwitchCase[valuesCount];
        var setCases = new SwitchCase[valuesCount];
        for (var i = 0; i < valuesCount; i++)
        {
            var propertyInfo = messageType.InstanceProperty("Value" + (i + 1).ToString(CultureInfo.InvariantCulture));
            var property = Expression.Property(typedMessage, propertyInfo.Name);
            var testValue = Expression.Constant(i);

            getCases[i] = Expression.SwitchCase(
                Expression.Convert(property, typeof(object)),
                testValue);

            setCases[i] = Expression.SwitchCase(
                Expression.Assign(property, Expression.Convert(valueArg, propertyInfo.PropertyType)),
                testValue);
        }

        var getValueSwitch = Expression.Switch(
            indexArg,
            Expression.Constant(null),
            getCases);
        var getValue = Expression.Lambda<TMessageGet>(getValueSwitch, messageArg, indexArg).Compile();

        var setValueSwitch = Expression.Switch(
            typeof(void),
            indexArg,
            null,
            null,
            setCases);
        var setValue = Expression.Lambda<TMessageSet>(setValueSwitch, messageArg, indexArg, valueArg).Compile();

        return (getValue, setValue);
    }

    private static (TStreamCreate Create, TStreamCast Cast) BuildStreamAccessors(Type itemType)
    {
        var streamCreate = typeof(FiltersCompiler)
            .StaticMethod(nameof(CreateEmptyStream))
            .MakeGenericMethod(itemType)
            .CreateDelegate<TStreamCreate>();

        var valueArg = Expression.Parameter(typeof(object));
        var streamType = typeof(IAsyncEnumerable<>).MakeGenericType(itemType);
        var streamCast = Expression.Lambda<TStreamCast>(Expression.Convert(valueArg, streamType), valueArg).Compile();
        return (streamCreate, streamCast);
    }

    private static IAsyncEnumerable<T> CreateEmptyStream<T>() => EmptyAsyncEnumerable<T>.Instance;

    private static object GetMessageProperty(object message, int index) => throw new NotSupportedException();

    private static void SetMessageProperty(object message, int index, object? value) => throw new NotSupportedException();

    private sealed class EmptyAsyncEnumerable<T> : IAsyncEnumerable<T>
    {
        public static readonly IAsyncEnumerable<T> Instance = new EmptyAsyncEnumerable<T>();

        private EmptyAsyncEnumerable()
        {
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken) => EmptyAsyncEnumerator<T>.Instance;
    }

    private sealed class EmptyAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        public static readonly IAsyncEnumerator<T> Instance = new EmptyAsyncEnumerator<T>();

        private EmptyAsyncEnumerator()
        {
        }

        public T Current => throw new InvalidOperationException();

        public ValueTask DisposeAsync() => new(Task.CompletedTask);

        public ValueTask<bool> MoveNextAsync() => new(false);
    }
}