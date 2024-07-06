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

internal sealed class ServerCallFilterHandler : CallFilterHandlerBase<IServerFilterContextInternal, IServerFilter>
{
    public ServerCallFilterHandler(IServerFilterContextInternal context, IServerFilter[] filters)
        : base(context, filters)
    {
    }

    protected override ValueTask HandleAsync(IServerFilter filter, Func<ValueTask> next) => filter.InvokeAsync(Context, next);

    protected override void Handle(IServerFilter filter, Action next) => throw new NotSupportedException();
}