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
    [InlineData("classify_extract_accepted.json")]
    [InlineData("classify_extract_rejected.json")]
    [InlineData("classify_extract_static_models.json")]
    [InlineData("classify_extract_unreviewed.json")]
    [InlineData("classify_unbundle.json")]
    [InlineData("genai_classify_extract_summarize.json")]
    public void TestSampleFiles(string filename)
    {
        var json = File.ReadAllText(Path.Combine(SamplesFolder, filename));
        var result = Result.FromJson(JObject.Parse(json));
        var changes = result.PreReview.ToChanges(result);
        Assert.NotNull(result);
        Assert.NotNull(changes);
    }
}
