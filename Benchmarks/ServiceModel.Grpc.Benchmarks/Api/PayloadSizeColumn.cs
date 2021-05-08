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
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace ServiceModel.Grpc.Benchmarks.Api
{
    internal sealed class PayloadSizeColumn : IColumn
    {
        public string Id => nameof(PayloadSizeColumn);

        public string ColumnName => "Message size";

        public bool AlwaysShow => true;

        public ColumnCategory Category => ColumnCategory.Custom;

        public int PriorityInCategory => int.MaxValue;

        public bool IsNumeric => false;

        public UnitType UnitType => UnitType.Size;

        public string Legend => "Grpc message size";

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
        {
            return GetValue(summary, benchmarkCase, SummaryStyle.Default);
        }

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style)
        {
            var methodName = benchmarkCase.Descriptor.WorkloadMethod.GetCustomAttribute<PayloadSizeColumnAttribute>()?.MethodName;
            if (string.IsNullOrEmpty(methodName))
            {
                return "-";
            }

            var benchmark = Activator.CreateInstance(benchmarkCase.Descriptor.Type);
            var method = benchmark
                .GetType()
                .GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .CreateDelegate<Func<ValueTask<long>>>(benchmark);

            var size = method().Result;
            return LengthToString(size);
        }

        public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase)
        {
            return false;
        }

        public bool IsAvailable(Summary summary)
        {
            return true;
        }

        private static string LengthToString(long length)
        {
            string units;
            double value;
            if (length >= 0x400)
            {
                units = " KB";
                value = length;
                value = value / 1024;
            }
            else
            {
                units = " B";
                value = length;
            }

            return value.ToString("0.##", CultureInfo.InvariantCulture) + units;
        }
    }
}
