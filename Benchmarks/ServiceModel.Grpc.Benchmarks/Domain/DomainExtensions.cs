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

namespace ServiceModel.Grpc.Benchmarks.Domain
{
    internal static class DomainExtensions
    {
        public static SomeObject CreateSomeObject()
        {
            var someObject = new SomeObject
            {
                StringScalar = "some meaningful text",
                Int32Scalar = 1,
                DateScalar = DateTime.UtcNow,
                SingleScalar = 1.1f,
                Int32Array = CreateArray(100, x => x),
                SingleArray = CreateArray<float>(100, x => x),
                DoubleArray = CreateArray<double>(100, x => x)
            };

            return someObject;
        }

        public static SomeObjectProto CopyToProto(SomeObject value)
        {
            var result = new SomeObjectProto
            {
                DateScalar = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(value.DateScalar),
                Int32Scalar = value.Int32Scalar,
                SingleScalar = value.SingleScalar
            };

            if (value.StringScalar != null)
            {
                result.StringScalar = value.StringScalar;
            }

            if (value.SingleArray != null)
            {
                result.SingleArray.Add(value.SingleArray);
            }

            if (value.Int32Array != null)
            {
                result.Int32Array.Add(value.Int32Array);
            }

            if (value.DoubleArray != null)
            {
                result.DoubleArray.Add(value.DoubleArray);
            }

            return result;
        }

        private static T[] CreateArray<T>(int size, Func<int, T> value)
        {
            var result = new T[size];
            for (var i = 0; i < size; i++)
            {
                result[i] = value(i);
            }

            return result;
        }
    }
}
