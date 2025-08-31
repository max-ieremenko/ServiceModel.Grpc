using Contract;
using System.Threading.Tasks;
using System.Threading;
using Client.Tools;

namespace Client;

internal sealed class ClientTestFixture
{
    private readonly ICalculator _calculator;

    public ClientTestFixture(ICalculator calculator)
    {
        _calculator = calculator;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        await TestCreateRectangleAsync(cancellationToken);
        await TestCreateInvalidRectangleAsync(cancellationToken);
        await TestShiftAsync(cancellationToken);
        await TestGetVerticesAsync(cancellationToken);
        await TestGetNumbersAsync(cancellationToken);
    }

    private async Task TestCreateRectangleAsync(CancellationToken cancellationToken)
    {
        var actual = await _calculator.CreateRectangleAsync((0, 2), (2, 2), (2, 0), (0, 0), cancellationToken);

        actual.ShouldBe(new Rectangle((0, 2), 2, 2));
    }

    private async Task TestCreateInvalidRectangleAsync(CancellationToken cancellationToken)
    {
        var ex = await Should.ThrowAsync<InvalidRectangleException>(
            () => _calculator.CreateRectangleAsync((0, 2), (2, 1), (2, 0), (0, 0), cancellationToken));

        ex.Message.ShouldContain("invalid rectangle");
        ex.Points.ShouldBe([(0, 2), (2, 1), (2, 0), (0, 0)]);
    }

    private async Task TestShiftAsync(CancellationToken cancellationToken)
    {
        var points = AsyncEnumerable.AsAsync<Point>((0, 2), (2, 2));
        
        var actual = await _calculator.ShiftAsync(points, 1, 2, cancellationToken);

        actual.ShouldBe([(1, 4), (3, 4)]);
    }

    private async Task TestGetVerticesAsync(CancellationToken cancellationToken)
    {
        var actual = await _calculator.GetVerticesAsync(new Rectangle((0, 2), 2, 2), cancellationToken);

        actual.Count.ShouldBe(4);
        var points = await actual.Points.ToArrayAsync(cancellationToken);
        points.ShouldBe([(0, 2), (2, 2), (2, 0), (0, 0)]);
    }

    private async Task TestGetNumbersAsync(CancellationToken cancellationToken)
    {
        var points = AsyncEnumerable.AsAsync<Point>((0, 2), (2, 2));

        var actual = await _calculator.GetNumbersAsync(points, cancellationToken).ToArrayAsync(cancellationToken);

        actual.ShouldBe([0, 2, 2, 2]);
    }
}