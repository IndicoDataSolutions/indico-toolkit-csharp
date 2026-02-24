using IndicoToolkit.EtlOutputs;
using Xunit;

namespace IndicoToolkit.Tests.EtlOutputs;


public class TokenTableCellTests
{
    // The base directory will be IndicoToolkit.Tests/bin/Debug/net*/
    private static readonly string SamplesFolder = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "..", "..", "..", "EtlOutputs", "Samples"
    );
    private static readonly string EtlOutputFile = Path.Combine(
        SamplesFolder,
        "4725", "111924", "110239", "etl_output.json"
    );

    private static string ReadUri(string uri)
    {
        var storageFolderPath = uri.Split("/storage/submission/").Last();
        var filePath = Path.Combine(SamplesFolder, storageFolderPath);
        return File.ReadAllText(filePath);
    }

    private static Span HeaderSpan => new(1, 1281, 1285);
    private static Span ContentSpan => new(1, 1343, 1349);
    private static Span LineItemSpan => new(1, 1311, 1244);
    private static Span MultipleTableSpan => new(1, 1217, 1299);
    private static Span OutsideTableSpan => new(1, 1056, 1067);

    [Fact]
    public void TestTextSlice()
    {
        var etlOutput = EtlOutput.Load(EtlOutputFile, reader: ReadUri);

        Assert.Equal("COST", etlOutput.Text[HeaderSpan.Range]);
        Assert.Equal("720.00", etlOutput.Text[ContentSpan.Range]);
    }

    [Fact]
    public void TestToken()
    {
        var etlOutput = EtlOutput.Load(EtlOutputFile, reader: ReadUri);
        var headerToken = etlOutput.TokenFor(HeaderSpan);
        var contentToken = etlOutput.TokenFor(ContentSpan);

        Assert.Equal(HeaderSpan, headerToken.Span);
        Assert.Equal(ContentSpan, contentToken.Span);

        Assert.Equal("COST", headerToken.Text);
        Assert.Equal("720.00", contentToken.Text);
    }

    [Fact]
    public void TestTokenNotFound()
    {
        var etlOutput = EtlOutput.Load(EtlOutputFile, reader: ReadUri);

        Assert.Equal(Token.NULL_TOKEN, etlOutput.TokenFor(HeaderSpan with { Page = 3 }));
        Assert.True(etlOutput.TokenFor(Span.NULL_SPAN).IsNull);
    }

    [Fact]
    public void TestNoTokens()
    {
        var etlOutput = EtlOutput.Load(EtlOutputFile, reader: ReadUri, tokens: false);

        Assert.Equal(Token.NULL_TOKEN, etlOutput.TokenFor(HeaderSpan));
        Assert.True(etlOutput.TokenFor(HeaderSpan).IsNull);
    }

    [Fact]
    public void TestTableCell()
    {
        var etlOutput = EtlOutput.Load(EtlOutputFile, reader: ReadUri);

        var (headerTable, headerCell) = etlOutput.TableCellsFor(HeaderSpan).First();
        var (contentTable, contentCell) = etlOutput.TableCellsFor(ContentSpan).First();

        Assert.Equal(HeaderSpan, headerCell.Span);
        Assert.Equal(ContentSpan, contentCell.Span);

        Assert.Equal(CellType.HEADER, headerCell.Type);
        Assert.Equal(CellType.CONTENT, contentCell.Type);

        Assert.Equal("COST", headerCell.Text);
        Assert.Equal("720.00", contentCell.Text);
    }

    [Fact]
    public void TestTableCells()
    {
        var etlOutput = EtlOutput.Load(EtlOutputFile, reader: ReadUri);

        var tableCells = etlOutput.TableCellsFor(LineItemSpan);
        var correctTable = etlOutput.Tables[3];
        var correctRow = correctTable.Rows[1];
        var correctCells = correctRow[1..4];

        foreach (var ((table, cell), correctCell) in tableCells.Zip(correctCells))
        {
            Assert.Equal(correctTable, table);
            Assert.Equal(correctCell, cell);
        }
    }

    [Fact]
    public void TestMultipleTables()
    {
        var etlOutput = EtlOutput.Load(EtlOutputFile, reader: ReadUri);

        var tableCells = etlOutput.TableCellsFor(MultipleTableSpan);
        var cells = tableCells.Select(tableCell => tableCell.Cell);

        var correctRows = etlOutput.Tables[2].Rows.Last().Concat(etlOutput.Tables[3].Rows.First());
        var correctCells = correctRows.Where(cell => cell.Text != "");

        Assert.Equal(correctCells, cells);
    }

    [Fact]
    public void TestTableCellNotFound()
    {
        var etlOutput = EtlOutput.Load(EtlOutputFile, reader: ReadUri);

        Assert.False(etlOutput.TableCellsFor(OutsideTableSpan).Any());
        Assert.False(etlOutput.TableCellsFor(Span.NULL_SPAN).Any());
        Assert.False(etlOutput.TableCellsFor(new Span(-1, -1, -1)).Any());
    }

    [Fact]
    public void TestNoTables()
    {
        var etlOutput = EtlOutput.Load(EtlOutputFile, reader: ReadUri, tokens: false, tables: false);

        Assert.False(etlOutput.TableCellsFor(HeaderSpan).Any());
        Assert.False(etlOutput.TableCellsFor(Span.NULL_SPAN).Any());
    }

    [Fact]
    public void TestEmptyCell()
    {
        var etlOutput = EtlOutput.Load(EtlOutputFile, reader: ReadUri);

        var table = etlOutput.Tables[2];
        var filledCell = table.Rows[1][2];
        var emptyCell = table.Rows[1][3];

        Assert.NotEqual("", filledCell.Text);
        Assert.Equal("", emptyCell.Text);

        Assert.False(filledCell.Span.IsNull);
        Assert.True(emptyCell.Span.IsNull);
    }
}
