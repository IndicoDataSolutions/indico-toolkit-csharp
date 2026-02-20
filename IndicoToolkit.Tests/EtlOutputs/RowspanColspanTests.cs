using IndicoToolkit.EtlOutputs;
using Xunit;

namespace IndicoToolkit.Tests.EtlOutputs;


public class RowspanColspanTests
{
    // The base directory will be IndicoToolkit.Tests/bin/Debug/net*/
    private static readonly string SamplesFolder = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "..", "..", "..", "EtlOutputs", "Samples"
    );
    private static readonly string EtlOutputFile = Path.Combine(
        SamplesFolder,
        "4725", "112731", "112257", "etl_output_rs_cs.json"
    );

    public static string ReadUri(string uri)
    {
        var storageFolderPath = uri.Split("/storage/submission/").Last();
        var filePath = Path.Combine(SamplesFolder, storageFolderPath);
        return File.ReadAllText(filePath);
    }

    /*
    The table for these tests looks like:

    |   Alfa   |  Bravo  | Charlie |  Delta  |
    |----------|-------------------|---------|
    |          |      Foxtrot      | Golf    |
    | Echo     |-------------------|---------|
    |          | Hotel   | India   | Juliett |
    |----------|-------------------|---------|
    | Kilo     |                   | Mike    |
    |----------|        Lima       |---------|
    | November |                   | Oscar   |
     ----------------------------------------
    */
    public static EtlOutput SampleEtlOutput => EtlOutput.Load(EtlOutputFile, reader: ReadUri);
    public static Table SampleTable => SampleEtlOutput.Tables.First();

    [Fact]
    public void TestCells()
    {
        var sampleCells = SampleTable.Cells.Select(cell => cell.Text);
        var expectedCells = new List<string> {
            "Alfa", "Bravo", "Charlie", "Delta",
            "Echo", "Foxtrot", "Golf",
            "Hotel", "India", "Juliett",
            "Kilo", "Lima", "Mike",
            "November", "Oscar",
        };

        Assert.Equal(expectedCells, sampleCells);
    }

    [Fact]
    public void TestRows()
    {
        var sampleRows = SampleTable.Rows.Select(row => row.Select(cell => cell.Text).ToList()).ToList();
        var expectedRows = new List<List<string>> {
            new() { "Alfa", "Bravo", "Charlie", "Delta" },
            new() { "Echo", "Foxtrot", "Foxtrot", "Golf" },
            new() { "Echo", "Hotel", "India", "Juliett" },
            new() { "Kilo", "Lima", "Lima", "Mike" },
            new() { "November", "Lima", "Lima", "Oscar" },
        };

        Assert.Equal(expectedRows, sampleRows);
    }

    [Fact]
    public void TestColumns()
    {
        var sampleColumns = SampleTable.Columns.Select(column => column.Select(cell => cell.Text).ToList()).ToList();
        var expectedColumns = new List<List<string>> {
            new() {
                "Alfa",
                "Echo",
                "Echo",
                "Kilo",
                "November",
            },
            new() {
                "Bravo",
                "Foxtrot",
                "Hotel",
                "Lima",
                "Lima",
            },
            new() {
                "Charlie",
                "Foxtrot",
                "India",
                "Lima",
                "Lima",
            },
            new() {
                "Delta",
                "Golf",
                "Juliett",
                "Mike",
                "Oscar",
            },
        };

        Assert.Equal(expectedColumns, sampleColumns);
    }

    [Theory]
    [InlineData(0, 25, 29, "Alfa")]
    [InlineData(0, 30, 35, "Bravo")]
    [InlineData(0, 36, 43, "Charlie")]
    [InlineData(0, 44, 49, "Delta")]
    [InlineData(0, 50, 54, "Echo")]
    [InlineData(0, 55, 62, "Foxtrot")]
    [InlineData(0, 64, 68, "Golf")]
    [InlineData(0, 70, 75, "Hotel")]
    [InlineData(0, 76, 81, "India")]
    [InlineData(0, 82, 89, "Juliett")]
    [InlineData(0, 90, 94, "Kilo")]
    [InlineData(0, 111, 115, "Lima")]
    [InlineData(0, 97, 101, "Mike")]
    [InlineData(0, 102, 110, "November")]
    [InlineData(0, 117, 122, "Oscar")]
    public void TestTableCellFor(int page, int start, int end, string expectedText)
    {
        var span = new Span(page, start, end);
        var tableCell = SampleEtlOutput.TableCellsFor(span).First();

        Assert.Equal(expectedText, tableCell.Cell.Text);
    }
}
