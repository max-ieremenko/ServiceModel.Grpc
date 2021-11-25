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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ServiceModel.Grpc.Internal;
using TMessageGet = System.Func<object, int, object?>;
using TMessageSet = System.Action<object, int, object?>;
using TStreamCast = System.Func<object, object>;
using TStreamCreate = System.Func<object>;

namespace ServiceModel.Grpc.Filters.Internal
{
    internal static class ProxyCompiler
    {
        private static readonly ConcurrentDictionary<Type, Tuple<TMessageGet, TMessageSet>> MessageAccessorByType
            = new ConcurrentDictionary<Type, Tuple<TMessageGet, TMessageSet>>();

        private static readonly ConcurrentDictionary<Type, Tuple<TStreamCreate, TStreamCast>> StreamAccessorByType
            = new ConcurrentDictionary<Type, Tuple<TStreamCreate, TStreamCast>>();

        public static (TMessageGet GetValue, TMessageSet SetValue) GetMessageAccessors(Type messageType)
        {
            var result = MessageAccessorByType.GetOrAdd(messageType, BuildAccessors);
            return (result.Item1, result.Item2);
        }

        public static (TStreamCreate CreateDefault, TStreamCast Cast) GetStreamAccessors(Type itemType)
        {
            var result = StreamAccessorByType.GetOrAdd(itemType, BuildStreamAccessors);
            return (result.Item1, result.Item2);
        }

        private static Tuple<TMessageGet, TMessageSet> BuildAccessors(Type messageType)
        {
            var messageArg = Expression.Parameter(typeof(object));
            var valueArg = Expression.Parameter(typeof(object));
            var indexArg = Expression.Parameter(typeof(int));
            var typedMessage = Expression.Convert(messageArg, messageType);

            var valuesCount = messageType.GenericTypeArguments.Length;
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

            return new Tuple<TMessageGet, TMessageSet>(getValue, setValue);
        }

        private static Tuple<TStreamCreate, TStreamCast> BuildStreamAccessors(Type itemType)
        {
            var streamCreate = typeof(ProxyCompiler)
                .StaticMethod(nameof(CreateEmptyStream))
                .MakeGenericMethod(itemType)
                .CreateDelegate<TStreamCreate>();

            var valueArg = Expression.Parameter(typeof(object));
            var streamType = typeof(IAsyncEnumerable<>).MakeGenericType(itemType);
            var streamCast = Expression.Lambda<TStreamCast>(Expression.Convert(valueArg, streamType), valueArg).Compile();
            return new Tuple<TStreamCreate, TStreamCast>(streamCreate, streamCast);
        }

        private static IAsyncEnumerable<T> CreateEmptyStream<T>() => EmptyAsyncEnumerable<T>.Instance;

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

            public ValueTask DisposeAsync() => new ValueTask(Task.CompletedTask);

            public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(false);
        }
    }
}
