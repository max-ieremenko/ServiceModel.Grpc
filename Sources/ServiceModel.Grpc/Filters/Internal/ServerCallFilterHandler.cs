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
using System.Threading;
using System.Threading.Tasks;

namespace ServiceModel.Grpc.Filters.Internal
{
    internal sealed class ServerCallFilterHandler
    {
        private readonly Func<ValueTask> _nextAsync;
        private int _processedIndex;
        private Func<IServerFilterContextInternal, ValueTask>? _last;

        public ServerCallFilterHandler(IServerFilterContextInternal context, IServerFilter[] filters)
        {
            Context = context;
            Filters = filters;
            _nextAsync = NextAsync;
            _processedIndex = -1;
        }

        public IServerFilterContextInternal Context { get; }

        public IServerFilter[] Filters { get; }

        public ValueTask InvokeAsync(Func<IServerFilterContextInternal, ValueTask> last)
        {
            _last = last;
            return NextAsync();
        }

        private ValueTask NextAsync()
        {
            var index = Interlocked.Increment(ref _processedIndex);
            if (index >= 0 && index < Filters.Length)
            {
                var filter = Filters[index];
                Filters[index] = null!;
                return filter.InvokeAsync(Context, _nextAsync);
            }

            var last = Interlocked.Exchange(ref _last, null);
            if (last != null)
            {
                return last(Context);
            }

            return new ValueTask(Task.CompletedTask);
        }
    }
}
