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

namespace ServiceModel.Grpc.Filters.Internal;

internal abstract class CallFilterHandlerBase<TContext, TFilter>
    where TContext : class
    where TFilter : class
{
    private Func<ValueTask>? _nextAsync;
    private Action? _next;
    private int _processedIndex;
    private Func<TContext, ValueTask>? _lastAsync;
    private Action<TContext>? _last;

    protected CallFilterHandlerBase(TContext context, TFilter[] filters)
    {
        Context = context;
        Filters = filters;
        _processedIndex = -1;
    }

    public TContext Context { get; }

    public TFilter[] Filters { get; }

    public void Invoke(Action<TContext> last)
    {
        _last = last;
        _next = Next;
        Next();
    }

    public ValueTask InvokeAsync(Func<TContext, ValueTask> last)
    {
        _lastAsync = last;
        _nextAsync = NextAsync;
        return NextAsync();
    }

    protected abstract ValueTask HandleAsync(TFilter filter, Func<ValueTask> next);

    protected abstract void Handle(TFilter filter, Action next);

    private ValueTask NextAsync()
    {
        var filter = GetNextFilter();
        if (filter != null)
        {
            return HandleAsync(filter, _nextAsync!);
        }

        var last = Interlocked.Exchange(ref _lastAsync, null);
        if (last != null)
        {
            return last(Context);
        }

        return new ValueTask(Task.CompletedTask);
    }

    private void Next()
    {
        var filter = GetNextFilter();
        if (filter != null)
        {
            Handle(filter, _next!);
            return;
        }

        var last = Interlocked.Exchange(ref _last, null);
        last?.Invoke(Context);
    }

    private TFilter? GetNextFilter()
    {
        var index = Interlocked.Increment(ref _processedIndex);
        if (index >= 0 && index < Filters.Length)
        {
            var filter = Filters[index];
            Filters[index] = null!;
            return filter;
        }

        return null;
    }
}