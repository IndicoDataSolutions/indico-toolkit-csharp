using Newtonsoft.Json.Linq;
using Xunit;

namespace IndicoToolkit.Tests;


public class UtilsTests
{
    private static JObject Prediction => JObject.Parse(@"
        {
            ""label"": ""Invoice Number"",
            ""text"": ""INV12345"",
            ""confidence"": {
                ""Invoice Number"": 0.9,
            },
            ""spans"": [
                {""start"": 123, ""end"": 456, ""page_num"": 0},
            ],
        }
    ");

    [Fact]
    public void TestGetHas()
    {
        Assert.True(Utils.Has<string>(Prediction, "label"));
        Assert.Equal("Invoice Number", Utils.Get<string>(Prediction, "label"));

        Assert.True(Utils.Has<JObject>(Prediction, "confidence"));
        Assert.True(Utils.Has<double>(Prediction, "confidence", "Invoice Number"));
        Assert.Equal(0.9, Utils.Get<double>(Prediction, "confidence", "Invoice Number"));

        Assert.True(Utils.Has<JArray>(Prediction, "spans"));
        Assert.True(Utils.Has<int>(Prediction, "spans", 0, "start"));
        Assert.Equal(123, Utils.Get<int>(Prediction, "spans", 0, "start"));
    }

    [Fact]
    public void TestGetHasNot()
    {
        Assert.False(Utils.Has<string>(Prediction, "missing"));
        Assert.Throws<TraversalException>(
            () => Utils.Get<string>(Prediction, "missing")
        );

        Assert.False(Utils.Has<int>(Prediction, "label"));
        Assert.Throws<TraversalException>(
            () => Utils.Get<int>(Prediction, "label")
        );

        Assert.False(Utils.Has<double>(Prediction, "confidence", "Invoice Number", 0));
        Assert.Throws<TraversalException>(
            () => Utils.Get<double>(Prediction, "confidence", "Invoice Number", 0)
        );

        Assert.False(Utils.Has<int>(Prediction, "spans", "0", "start"));
        Assert.Throws<TraversalException>(
            () => Utils.Get<int>(Prediction, "spans", "0", "start")
        );

        Assert.False(Utils.Has<int>(Prediction, "spans", -1, "start"));
        Assert.Throws<TraversalException>(
            () => Utils.Get<int>(Prediction, "spans", -1, "start")
        );

        Assert.False(Utils.Has<int>(Prediction, "spans", -1, "start"));
        Assert.Throws<TraversalException>(
            () => Utils.Get<int>(Prediction, "spans", 1, "start")
        );
    }
}
