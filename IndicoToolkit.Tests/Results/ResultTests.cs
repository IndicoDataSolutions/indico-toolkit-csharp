using IndicoToolkit.Results;
using System.Collections.Immutable;
using Xunit;

namespace IndicoToolkit.Tests.Results;


public class ResultTests
{
    [Fact]
    public void TestRejected()
    {
        var result = new Result(
            0,
            ImmutableArray<Document>.Empty,
            ImmutableArray<IndicoToolkit.Results.Tasks.Task>.Empty,
            ImmutableArray.Create(
                new Review(0, 0, "", true, ReviewType.MANUAL)
            ),
            new PredictionList<Prediction>()
        );

        Assert.True(result.Rejected);
    }

    [Fact]
    public void TestUnrejected()
    {
        var result = new Result(
            0,
            ImmutableArray<Document>.Empty,
            ImmutableArray<IndicoToolkit.Results.Tasks.Task>.Empty,
            ImmutableArray.Create(
                new Review(0, 0, "", false, ReviewType.AUTO),
                new Review(0, 0, "", true, ReviewType.MANUAL),
                new Review(0, 0, "", false, ReviewType.ADMIN)
            ),
            new PredictionList<Prediction>()
        );

        Assert.False(result.Rejected);
    }
}
