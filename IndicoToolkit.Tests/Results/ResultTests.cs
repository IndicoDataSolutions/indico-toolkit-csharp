using IndicoToolkit.Results;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using Xunit;

namespace IndicoToolkit.Tests;


public class ResultTests
{
    // The base directory will be IndicoToolkit.Tests/bin/Debug/net8.0/
    private string SamplesFolder = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "..", "..", "..", "Results", "Samples"
    );

    [Theory]
    [InlineData("v3_classify_extract_unreviewed.json")]
    [InlineData("v3_classify_extract_accepted.json")]
    [InlineData("v3_classify_extract_rejected.json")]
    public void TestSampleFiles(string filename)
    {
        var json = File.ReadAllText(Path.Combine(SamplesFolder, filename));
        var result = Result.FromJson(JObject.Parse(json));
        var changes = result.PreReview.ToChanges(result);
        Assert.NotNull(result);
        Assert.NotNull(changes);
    }
}
