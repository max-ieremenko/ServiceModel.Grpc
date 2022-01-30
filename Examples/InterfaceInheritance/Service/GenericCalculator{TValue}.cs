using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Contract;

namespace Service
{
    public sealed class GenericCalculator<TValue> : IGenericCalculator<TValue>
    {
        private static readonly Func<int, TValue> Cast;
        private static readonly Func<TValue, TValue, TValue> DoSum;
        private static readonly Func<TValue, TValue, TValue> DoMultiply;

        static GenericCalculator()
        {
            var x = Expression.Parameter(typeof(TValue));
            var y = Expression.Parameter(typeof(TValue));

            DoSum = Expression
                .Lambda<Func<TValue, TValue, TValue>>(
                    Expression.Add(x, y),
                    x,
                    y)
                .Compile();

            DoMultiply = Expression
                .Lambda<Func<TValue, TValue, TValue>>(
                    Expression.Multiply(x, y),
                    x,
                    y)
                .Compile();

            var value = Expression.Parameter(typeof(int));
            Cast = Expression
                .Lambda<Func<int, TValue>>(
                    Expression.Convert(value, typeof(TValue)),
                    value)
                .Compile();
        }

        // POST /IGenericCalculator-TValue/Touch
        public string Touch()
        {
            return string.Format("GenericCalculator<{0}>", typeof(TValue).Name);
        }

        // POST /IGenericCalculator-TValue/Sum
        public Task<TValue> Sum(TValue x, TValue y)
        {
            var result = DoSum(x, y);
            return Task.FromResult(result);
        }

        // POST /IGenericCalculator-TValue/Multiply
        public ValueTask<TValue> Multiply(TValue x, TValue y)
        {
            var result = DoMultiply(x, y);
            return new ValueTask<TValue>(result);
        }

        // POST /IGenericCalculator-TValue/GetRandomValue
        public ValueTask<TValue> GetRandomValue()
        {
            var value = new Random(DateTime.Now.Millisecond).Next(0, 10_000);
            var result = Cast(value);
            return new ValueTask<TValue>(result);
        }
    }
}
