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
using System.Diagnostics;

namespace ServiceModel.Grpc.DesignTime
{
    /// <summary>
    /// A marker to generate the source code for client service proxy.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    [Conditional("CodeGeneration")]
    public sealed class ImportGrpcServiceAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImportGrpcServiceAttribute"/> class.
        /// </summary>
        /// <param name="serviceContract">The service contract type to map requests to.</param>
        public ImportGrpcServiceAttribute(Type serviceContract)
        {
            if (serviceContract == null)
            {
                throw new ArgumentNullException(nameof(serviceContract));
            }

            ServiceContract = serviceContract;
        }

        /// <summary>
        /// Gets a service contract type to map requests to.
        /// </summary>
        public Type ServiceContract { get; }
    }
}
