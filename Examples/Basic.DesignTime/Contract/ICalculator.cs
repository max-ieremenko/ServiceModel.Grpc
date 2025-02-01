using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace Contract;

[ServiceContract]
public interface ICalculator
{
    // blocking unary call
    [OperationContract]
    Number Sum(int x, int y, int z, CancellationToken cancellationToken = default);

    // async unary call
    [OperationContract]
    Task<Number> SumAsync(int x, int y, CancellationToken cancellationToken = default);

    // async client streaming call
    [OperationContract]
    Task<int?> Max(IAsyncEnumerable<Number> numbers, CancellationToken cancellationToken = default);

    // async client streaming call with external request parameters
    [OperationContract]
    Task<int?> FirstGreaterThan(IAsyncEnumerable<int> numbers, int value, CancellationToken cancellationToken = default);

    // async server streaming call
    [OperationContract]
    IAsyncEnumerable<Number> GenerateRandom(int count, CancellationToken cancellationToken = default);

    // async server streaming call with external response parameters
    [OperationContract]
    Task<(int MinValue, int MaxValue, IAsyncEnumerable<Number> Numbers)> GenerateRandomWithinRange(int minValue, int maxValue, int count, CancellationToken cancellationToken = default);

    // async duplex streaming call
    [OperationContract]
    IAsyncEnumerable<Number> MultiplyBy2(IAsyncEnumerable<int> numbers, CancellationToken cancellationToken = default);

    // async duplex streaming call with external request and response parameters
    [OperationContract]
    Task<(int Multiplier, IAsyncEnumerable<Number> Numbers)> MultiplyBy(IAsyncEnumerable<int> numbers, Number multiplier, CancellationToken cancellationToken = default);
}