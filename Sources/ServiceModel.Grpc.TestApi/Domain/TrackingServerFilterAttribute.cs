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
using ServiceModel.Grpc.Filters;

namespace ServiceModel.Grpc.TestApi.Domain
{
    public sealed class TrackingServerFilterAttribute : ServerFilterRegistrationAttribute
    {
        private readonly string _name;

        public TrackingServerFilterAttribute(int order, string name)
            : base(order)
        {
            _name = name;
        }

        public override IServerFilter CreateFilter(IServiceProvider serviceProvider)
        {
            return new TrackingServerFilter(_name);
        }
    }
}