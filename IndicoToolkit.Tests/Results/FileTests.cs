using IndicoToolkit.Results;
using Newtonsoft.Json.Linq;
using Xunit;

namespace IndicoToolkit.Tests;


public class ResultTests
{
    // The base directory will be IndicoToolkit.Tests/bin/Debug/net8.0/
    private string SamplesFolder = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "..", "..", "..", "Results", "Samples"
    );

    public string ReadUri(string uri)
    {
        var filePath = Path.Combine(SamplesFolder, uri);
        return File.ReadAllText(filePath);
    }

    public async Task<string> ReadUriAsync(string uri)
    {
        var filePath = Path.Combine(SamplesFolder, uri);
        return File.ReadAllText(filePath);
    }

    [Theory]
    [InlineData("classify_extract_accepted.json")]
    [InlineData("classify_extract_rejected.json")]
    [InlineData("classify_extract_static_models.json")]
    [InlineData("classify_extract_unreviewed.json")]
    [InlineData("classify_unbundle.json")]
    [InlineData("genai_classify_extract_summarize.json")]
    public void TestSampleFiles(string filename)
    {
        var result = Result.Load(filename, reader: ReadUri);
        var changes = result.PreReview.ToChanges(result);
        Assert.NotNull(result);
        Assert.NotNull(changes);
    }

    [Theory]
    [InlineData("classify_extract_accepted.json")]
    [InlineData("classify_extract_rejected.json")]
    [InlineData("classify_extract_static_models.json")]
    [InlineData("classify_extract_unreviewed.json")]
    [InlineData("classify_unbundle.json")]
    [InlineData("genai_classify_extract_summarize.json")]
    public async Task TestSampleFilesAsync(string filename)
    {
        var result = await Result.LoadAsync(filename, reader: ReadUriAsync);
        var changes = result.PreReview.ToChanges(result);
        Assert.NotNull(result);
        Assert.NotNull(changes);
    }
}
