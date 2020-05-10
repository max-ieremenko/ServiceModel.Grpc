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
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Moq;
using ServiceModel.Grpc.Channel;

namespace ServiceModel.Grpc.Internal.Emit
{
    internal static class StreamsMock
    {
        public static void Setup<T>(this Mock<IAsyncStreamReader<Message<T>>> reader, CancellationToken token, params T[] source)
        {
            SetupMoveNext(reader, token, source, 0, i => i);
        }

        public static void Setup<TInput, TOutput>(
            this Mock<IAsyncStreamReader<Message<TOutput>>> reader,
            CancellationToken token,
            IList<TInput> source,
            Func<TInput, TOutput> converter)
        {
            SetupMoveNext(reader, token, source, 0, converter);
        }

        public static IList<T> Setup<T>(this Mock<IServerStreamWriter<Message<T>>> writer)
        {
            var values = new List<T>();

            writer
                .Setup(s => s.WriteAsync(It.IsNotNull<Message<T>>()))
                .Callback<Message<T>>(message =>
                {
                    values.Add(message.Value1);
                })
                .Returns(Task.CompletedTask);

            return values;
        }

        public static void Setup<T>(this Mock<IClientStreamWriter<Message<T>>> writer, IList<T> output)
        {
            writer
                .Setup(s => s.WriteAsync(It.IsNotNull<Message<T>>()))
                .Callback<Message<T>>(message =>
                {
                    output.Add(message.Value1);
                })
                .Returns(Task.CompletedTask);

            writer
                .Setup(s => s.CompleteAsync())
                .Returns(Task.CompletedTask);
        }

        private static void SetupMoveNext<TInput, TOutput>(
            Mock<IAsyncStreamReader<Message<TOutput>>> reader,
            CancellationToken token,
            IList<TInput> source,
            int sourceIndex,
            Func<TInput, TOutput> converter)
        {
            reader
                .Setup(s => s.MoveNext(token))
                .Callback(() =>
                {
                    if (sourceIndex < source.Count)
                    {
                        reader
                            .SetupGet(s => s.Current)
                            .Returns(new Message<TOutput>(converter(source[sourceIndex])))
                            .Verifiable();

                        SetupMoveNext(reader, token, source, sourceIndex + 1, converter);
                    }
                    else
                    {
                        reader
                            .SetupGet(s => s.Current)
                            .Throws<NotSupportedException>();
                    }
                })
                .Returns(() => Task.FromResult(sourceIndex < source.Count))
                .Verifiable();
        }
    }
}
