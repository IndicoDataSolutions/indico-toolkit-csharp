using IndicoToolkit.EtlOutputs;
using Xunit;

namespace IndicoToolkit.Tests.EtlOutputs;


public class EtlOutputTests
{
    // The base directory will be IndicoToolkit.Tests/bin/Debug/net*/
    private static readonly string SamplesFolder = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "..", "..", "..", "EtlOutputs", "Samples"
    );

    public static string ReadUri(string uri)
    {
        var storageFolderPath = uri.Split("/storage/submission/").Last();
        var filePath = Path.Combine(SamplesFolder, storageFolderPath);
        return File.ReadAllText(filePath);
    }

    public static async Task<string> ReadUriAsync(string uri)
    {
        var storageFolderPath = uri.Split("/storage/submission/").Last();
        var filePath = Path.Combine(SamplesFolder, storageFolderPath);
        return await File.ReadAllTextAsync(filePath);
    }

    [Theory]
    [InlineData("4723/111922/110237/etl_output.json")]
    [InlineData("4724/111923/110238/etl_output.json")]
    [InlineData("4725/111924/110239/etl_output.json")]
    public void TestFileLoad(string filename)
    {
        var etlOutput = EtlOutput.Load(filename, reader: ReadUri);
        var pageCount = etlOutput.TextOnPage.Length;
        var charCount = etlOutput.Text.Length;
        var tokenCount = etlOutput.Tokens.Length;
        var tableCount = etlOutput.Tables.Length;

        Assert.Equal(2, pageCount);
        Assert.InRange(charCount, 2090, 2093);
        Assert.InRange(tokenCount, 326, 331);
        Assert.True(tableCount == 0 || tableCount == 4);
    }

    [Theory]
    [InlineData("4723/111922/110237/etl_output.json")]
    [InlineData("4724/111923/110238/etl_output.json")]
    [InlineData("4725/111924/110239/etl_output.json")]
    public async Task TestFileLoadAsync(string filename)
    {
        var etlOutput = await EtlOutput.LoadAsync(filename, reader: ReadUriAsync);
        var pageCount = etlOutput.TextOnPage.Length;
        var charCount = etlOutput.Text.Length;
        var tokenCount = etlOutput.Tokens.Length;
        var tableCount = etlOutput.Tables.Length;

        Assert.Equal(2, pageCount);
        Assert.InRange(charCount, 2090, 2093);
        Assert.InRange(tokenCount, 326, 331);
        Assert.True(tableCount == 0 || tableCount == 4);
    }

    [Theory]
    [InlineData("4723/111922/110237/etl_output.json")]
    [InlineData("4724/111923/110238/etl_output.json")]
    [InlineData("4725/111924/110239/etl_output.json")]
    public void TestFileLoadDisableValues(string filename)
    {
        var etlOutput = EtlOutput.Load(filename, reader: ReadUri, text: false, tokens: false, tables: false);
        var pageCount = etlOutput.TextOnPage.Length;
        var charCount = etlOutput.Text.Length;
        var tokenCount = etlOutput.Tokens.Length;
        var tableCount = etlOutput.Tables.Length;

        Assert.Equal(0, pageCount);
        Assert.Equal(0, charCount);
        Assert.Equal(0, tokenCount);
        Assert.Equal(0, tableCount);
    }

    [Theory]
    [InlineData("4723/111922/110237/etl_output.json")]
    [InlineData("4724/111923/110238/etl_output.json")]
    [InlineData("4725/111924/110239/etl_output.json")]
    public async Task TestFileLoadDisableValuesAsync(string filename)
    {
        var etlOutput = await EtlOutput.LoadAsync(filename, reader: ReadUriAsync, text: false, tokens: false, tables: false);
        var pageCount = etlOutput.TextOnPage.Length;
        var charCount = etlOutput.Text.Length;
        var tokenCount = etlOutput.Tokens.Length;
        var tableCount = etlOutput.Tables.Length;

        Assert.Equal(0, pageCount);
        Assert.Equal(0, charCount);
        Assert.Equal(0, tokenCount);
        Assert.Equal(0, tableCount);
    }
}
