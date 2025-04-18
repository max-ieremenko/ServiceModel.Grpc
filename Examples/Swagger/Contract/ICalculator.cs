using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace Contract;

[ServiceContract]
public interface ICalculator
{
    /// <summary>
    /// Generates a non-negative random integer.
    /// </summary>
    /// <remarks>
    /// Returns a 32-bit signed integer that is greater than or equal to 0 and less than <see cref="int.MaxValue"/>.
    /// </remarks>
    [OperationContract]
    Task<int> GetRandomNumber();

    /// <summary>
    /// Computes the sum.
    /// </summary>
    /// <remarks>
    /// Returns a 64-bit signed integer that is the sum of <paramref name="x"/>, <paramref name="y"/>, and <paramref name="z"/>.
    /// </remarks>
    [OperationContract]
    Task<long> Sum(long x, int y, int z, CancellationToken token = default);

    /// <summary>
    /// Computes the product.
    /// </summary>
    /// <remarks>
    /// Projects each element of the sequence <paramref name="values"/> into a product of the element and <paramref name="multiplier"/>.
    /// </remarks>
    [OperationContract]
    ValueTask<(int Multiplier, IAsyncEnumerable<int> Values)> MultiplyBy(IAsyncEnumerable<int> values, int multiplier, CancellationToken token = default);
}