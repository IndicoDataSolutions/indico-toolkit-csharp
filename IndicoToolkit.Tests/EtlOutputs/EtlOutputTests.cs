using IndicoToolkit.EtlOutputs;
using Xunit;

namespace IndicoToolkit.Tests;


public class EtlOutputTests
{
    // The base directory will be IndicoToolkit.Tests/bin/Debug/net8.0/
    private string SamplesFolder = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "..", "..", "..", "EtlOutputs", "Samples"
    );

    public string ReadUri(string uri)
    {
        var storageFolderPath = uri.Split("/storage/submission/").Last();
        var filePath = Path.Combine(SamplesFolder, storageFolderPath);
        return File.ReadAllText(filePath);
    }

    [Theory]
    [InlineData("4723/111922/110237/etl_output.json")]
    [InlineData("4724/111923/110238/etl_output.json")]
    [InlineData("4725/111924/110239/etl_output.json")]
    public void TestSampleFiles(string filename)
    {
        var etlOutput = EtlOutput.Load(filename, reader: ReadUri);
        var pageCount = etlOutput.TextOnPage.Count;
        var charCount = etlOutput.Text.Length;
        var tokenCount = etlOutput.Tokens.Count;
        var tableCount = etlOutput.Tables.Count;

        Assert.Equal(pageCount, 2);
        Assert.InRange(charCount, 2090, 2093);
        Assert.InRange(tokenCount, 326, 331);
        Assert.True(tableCount == 0 || tableCount == 4);
    }

    [Theory]
    [InlineData("4723/111922/110237/etl_output.json")]
    [InlineData("4724/111923/110238/etl_output.json")]
    [InlineData("4725/111924/110239/etl_output.json")]
    public async Task TestSampleFilesAsync(string filename)
    {
        var etlOutput = EtlOutput.Load(filename, reader: ReadUri);
        var pageCount = etlOutput.TextOnPage.Count;
        var charCount = etlOutput.Text.Length;
        var tokenCount = etlOutput.Tokens.Count;
        var tableCount = etlOutput.Tables.Count;

        Assert.Equal(pageCount, 2);
        Assert.InRange(charCount, 2090, 2093);
        Assert.InRange(tokenCount, 326, 331);
        Assert.True(tableCount == 0 || tableCount == 4);
    }

    [Theory]
    [InlineData("4723/111922/110237/etl_output.json")]
    [InlineData("4724/111923/110238/etl_output.json")]
    [InlineData("4725/111924/110239/etl_output.json")]
    public void TestSampleFilesDisableValues(string filename)
    {
        var etlOutput = EtlOutput.Load(filename, reader: ReadUri, text: false, tokens: false, tables: false);
        var pageCount = etlOutput.TextOnPage.Count;
        var charCount = etlOutput.Text.Length;
        var tokenCount = etlOutput.Tokens.Count;
        var tableCount = etlOutput.Tables.Count;

        Assert.Equal(pageCount, 0);
        Assert.Equal(charCount, 0);
        Assert.Equal(tokenCount, 0);
        Assert.Equal(tableCount, 0);
    }

    [Theory]
    [InlineData("4723/111922/110237/etl_output.json")]
    [InlineData("4724/111923/110238/etl_output.json")]
    [InlineData("4725/111924/110239/etl_output.json")]
    public async Task TestSampleFilesDisableValuesAsync(string filename)
    {
        var etlOutput = EtlOutput.Load(filename, reader: ReadUri, text: false, tokens: false, tables: false);
        var pageCount = etlOutput.TextOnPage.Count;
        var charCount = etlOutput.Text.Length;
        var tokenCount = etlOutput.Tokens.Count;
        var tableCount = etlOutput.Tables.Count;

        Assert.Equal(pageCount, 0);
        Assert.Equal(charCount, 0);
        Assert.Equal(tokenCount, 0);
        Assert.Equal(tableCount, 0);
    }
}
