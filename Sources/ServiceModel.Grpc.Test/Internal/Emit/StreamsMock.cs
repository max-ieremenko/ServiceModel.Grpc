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
            SetupMoveNext(reader, token, source, 0);
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

        private static void SetupMoveNext<T>(
            Mock<IAsyncStreamReader<Message<T>>> reader,
            CancellationToken token,
            IReadOnlyList<T> source,
            int sourceIndex)
        {
            if (sourceIndex < source.Count)
            {
                reader
                    .Setup(s => s.MoveNext(token))
                    .Callback(() =>
                    {
                        reader
                            .SetupGet(s => s.Current)
                            .Returns(new Message<T>(source[sourceIndex]))
                            .Verifiable();

                        SetupMoveNext(reader, token, source, sourceIndex + 1);
                    })
                    .Returns(Task.FromResult(true))
                    .Verifiable();
            }
            else
            {
                reader
                    .Setup(s => s.MoveNext(token))
                    .Callback(() =>
                    {
                        reader.SetupGet(s => s.Current).Throws<NotSupportedException>();
                    })
                    .Returns(Task.FromResult(false))
                    .Verifiable();
            }
        }
    }
}
