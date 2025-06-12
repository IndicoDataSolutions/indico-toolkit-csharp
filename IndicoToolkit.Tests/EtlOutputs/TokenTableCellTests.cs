using IndicoToolkit.EtlOutputs;
using IndicoToolkit.Results;
using Xunit;

namespace IndicoToolkit.Tests;


public class TokenTableCellTests
{
    // The base directory will be IndicoToolkit.Tests/bin/Debug/net8.0/
    private static string SamplesFolder = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "..", "..", "..", "EtlOutputs", "Samples"
    );
    private static string EtlOutputFile = Path.Combine(
        SamplesFolder,
        "4725", "111924", "110239", "etl_output.json"
    );

    public string ReadUri(string uri)
    {
        var storageFolderPath = uri.Split("/storage/submission/").Last();
        var filePath = Path.Combine(SamplesFolder, storageFolderPath);
        return File.ReadAllText(filePath);
    }

    public Span HeaderSpan()
    {
        return new Span(1, 1281, 1285);
    }

    public Span ContentSpan()
    {
        return new Span(1, 1343, 1349);
    }

    [Fact]
    public void TestTextSlice()
    {
        var etlOutput = EtlOutput.Load(EtlOutputFile, reader: ReadUri);

        Assert.Equal(etlOutput.Text[HeaderSpan().Range], "COST");
        Assert.Equal(etlOutput.Text[ContentSpan().Range], "720.00");
    }

    [Fact]
    public void TestToken()
    {
        var etlOutput = EtlOutput.Load(EtlOutputFile, reader: ReadUri);
        var headerToken = etlOutput.TokenFor(HeaderSpan());
        var contentToken = etlOutput.TokenFor(ContentSpan());

        Assert.Equal(headerToken.Span, HeaderSpan());
        Assert.Equal(contentToken.Span, ContentSpan());

        Assert.Equal(headerToken.Text, "COST");
        Assert.Equal(contentToken.Text, "720.00");
    }

    [Fact]
    public void TestTokenNotFound()
    {
        var etlOutput = EtlOutput.Load(EtlOutputFile, reader: ReadUri);

        Assert.Throws<TokenNotFoundException>(
            () => etlOutput.TokenFor(HeaderSpan() with { Page = 3 })
        );
    }

    [Fact]
    public void TestTableCell()
    {
        var etlOutput = EtlOutput.Load(EtlOutputFile, reader: ReadUri);

        var headerToken = etlOutput.TokenFor(HeaderSpan());
        var contentToken = etlOutput.TokenFor(ContentSpan());

        var (headerTable, headerCell) = etlOutput.TableCellFor(headerToken);
        var (contentTable, contentCell) = etlOutput.TableCellFor(contentToken);

        Assert.Equal(headerCell.Span, HeaderSpan());
        Assert.Equal(contentCell.Span, ContentSpan());

        Assert.Equal(headerCell.Type, CellType.HEADER);
        Assert.Equal(contentCell.Type, CellType.CONTENT);

        Assert.Equal(headerCell.Text, "COST");
        Assert.Equal(contentCell.Text, "720.00");
    }

    [Fact]
    public void TestTableCellNotFound()
    {
        var etlOutput = EtlOutput.Load(EtlOutputFile, reader: ReadUri);

        Assert.Throws<TableCellNotFoundException>(() => {
            var token = etlOutput.TokenFor(new Span(0, 0, 8));
            etlOutput.TableCellFor(token);
        });
    }

    [Fact]
    public void TestEmptyCell()
    {
        var etlOutput = EtlOutput.Load(EtlOutputFile, reader: ReadUri);

        var table = etlOutput.Tables[2];
        var filledCell = table.Rows[1][2];
        var emptyCell = table.Rows[1][3];

        Assert.NotEqual(filledCell.Text, "");
        Assert.Equal(emptyCell.Text, "");

        Assert.False(filledCell.Span.IsNull);
        Assert.True(emptyCell.Span.IsNull);
    }
}
