using Newtonsoft.Json.Linq;
using System.Collections.Immutable;

namespace IndicoToolkit.EtlOutputs;

public record EtlOutput
(
    string Text,
    ImmutableArray<string> TextOnPage,
    ImmutableArray<Token> Tokens,
    ImmutableArray<ImmutableArray<Token>> TokensOnPage,
    ImmutableArray<Table> Tables,
    ImmutableArray<ImmutableArray<Table>> TablesOnPage
)
{
    /*
    Lazily compute & memoize table cells sorted by span on each page.
    */
    private ImmutableArray<ImmutableArray<(Table Table, Cell Cell, Span Span)>> _TableCellSpansOnPage;
    private ImmutableArray<ImmutableArray<(Table Table, Cell Cell, Span Span)>> TableCellSpansOnPage
    {
        get
        {
            if (_TableCellSpansOnPage.IsDefaultOrEmpty)
            {
                _TableCellSpansOnPage = TablesOnPage
                    .Select(page => page
                        .SelectMany(table => table.Cells
                            .SelectMany(cell => cell.Spans
                                .Where(span => !span.IsNull)
                                .Select(span => (Table: table, Cell: cell, Span: span))))
                        .OrderBy(tuple => tuple.Span)
                        .ToImmutableArray())
                    .ToImmutableArray();
            }
            return _TableCellSpansOnPage;
        }
    }

    /*
    Load `etlOutputUri` as an `EtlOutput` record. A `reader` function must be
    supplied to read JSON and text strings from disk, storage API, or Indico client.

    Use `text`, `tokens`, and `tables` to specify what not to load.
    */
    public static EtlOutput Load(
        string etlOutputUri,
        Func<string, string> reader,
        bool text = true,
        bool tokens = true,
        bool tables = true
    )
    {
        var etlOutputJson = JObject.Parse(reader(etlOutputUri));
        var pages = Utils.Get<JArray>(etlOutputJson, "pages");

        IEnumerable<string> textPages;
        IEnumerable<JArray> tokenJsonPages;
        IEnumerable<JArray> tableJsonPages;

        if (text && Utils.Has<string>(pages, 0, "text"))
            textPages = pages.Select(page => reader(Utils.Get<string>(page, "text")));
        else
            textPages = ImmutableArray<string>.Empty;

        if (tokens && Utils.Has<string>(pages, 0, "tokens"))
            tokenJsonPages = pages.Select(page => JArray.Parse(reader(Utils.Get<string>(page, "tokens"))));
        else
            tokenJsonPages = ImmutableArray<JArray>.Empty;

        if (tables && Utils.Has<string>(pages, 0, "tables"))
            tableJsonPages = pages.Select(page => JArray.Parse(reader(Utils.Get<string>(page, "tables"))));
        else
            tableJsonPages = ImmutableArray<JArray>.Empty;

        return FromPages(textPages, tokenJsonPages, tableJsonPages);
    }

    /*
    Load `etlOutputUri` as an `EtlOutput` record. A `reader` coroutine must be
    supplied to read JSON and text strings from disk, storage API, or Indico client.

    Use `text`, `tokens`, and `tables` to specify what not to load.
    */
    public static async Task<EtlOutput> LoadAsync(
        string etlOutputUri,
        Func<string, Task<string>> reader,
        bool text = true,
        bool tokens = true,
        bool tables = true
    )
    {
        var etlOutputJson = JObject.Parse(await reader(etlOutputUri));
        var pages = Utils.Get<JArray>(etlOutputJson, "pages");

        var textPages = new List<string>();
        var tokenJsonPages = new List<JArray>();
        var tableJsonPages = new List<JArray>();

        if (text && Utils.Has<string>(pages, 0, "text"))
            foreach (var page in pages)
                textPages.Add(await reader(Utils.Get<string>(page, "text")));

        if (tokens && Utils.Has<string>(pages, 0, "tokens"))
            foreach (var page in pages)
                tokenJsonPages.Add(JArray.Parse(await reader(Utils.Get<string>(page, "tokens"))));

        if (tables && Utils.Has<string>(pages, 0, "tables"))
            foreach (var page in pages)
                tableJsonPages.Add(JArray.Parse(await reader(Utils.Get<string>(page, "tables"))));

        return FromPages(textPages, tokenJsonPages, tableJsonPages);
    }

    private static EtlOutput FromPages(
        IEnumerable<string> textPages,
        IEnumerable<JArray> tokenJsonPages,
        IEnumerable<JArray> tableJsonPages
    )
    {
        var tokenPages = tokenJsonPages
            .Select(page => page
                .Select(Token.FromJson)
                .OrderBy(token => token.Span)
                .ToImmutableArray())
            .ToImmutableArray();

        var tablePages = tableJsonPages
            .Select(page => page
                .Select(Table.FromJson)
                .OrderBy(table => table.Box)
                .ToImmutableArray())
            .ToImmutableArray();

        return new(
            string.Join("\n", textPages),
            textPages.ToImmutableArray(),
            tokenPages.SelectMany(page => page).ToImmutableArray(),
            tokenPages,
            tablePages.SelectMany(page => page).ToImmutableArray(),
            tablePages
        );
    }

    /*
    Return a `Token` that contains every character from `span`
    or `NULL_TOKEN` if one doesn't exist.
    */
    public Token TokenFor(Span span)
    {
        ImmutableArray<Token> tokens;

        try
        {
            tokens = TokensOnPage[span.Page];
            var first = BisectRight<Token>(tokens, span.Start, key: token => token.Span.End);
            var last = BisectLeft<Token>(tokens, span.End, key: token => token.Span.Start, low: first);
            tokens = tokens[first..last];
            tokens.First();  // Raise an exception if there are no tokens.
        }
        catch
        {
            return Token.NULL_TOKEN;
        }

        return new Token(
            Text[span.Range],
            new Box(
                span.Page,
                tokens.Select(token => token.Box.Top).Min(),
                tokens.Select(token => token.Box.Left).Min(),
                tokens.Select(token => token.Box.Right).Max(),
                tokens.Select(token => token.Box.Bottom).Max()
            ),
            span
        );
    }

    /*
    Yield the table cells that overlap with `span`.

    Note: a single span may overlap the same cell multiple times causing it to be
    yielded multiple times. Deduplication in `DocumentExtraction.TableCells`
    accounts for this when OCR is assigned with `PredictionList.AssignOcr()`.
    */
    public IEnumerable<(Table Table, Cell Cell)> TableCellsFor(Span span)
    {
        ImmutableArray<(Table Table, Cell Cell, Span Span)> tableCellSpans;
        try
        {
            var pageTableCellSpans = TableCellSpansOnPage[span.Page];
            var first = BisectRight(pageTableCellSpans, span.Start, key: tuple => tuple.Span.End);
            var last = BisectLeft(pageTableCellSpans, span.End, key: tuple => tuple.Span.Start, low: first);
            tableCellSpans = pageTableCellSpans[first..last];
        }
        catch
        {
            tableCellSpans = ImmutableArray<(Table Table, Cell Cell, Span Span)>.Empty;
        }

        foreach (var (table, cell, _) in tableCellSpans)
            yield return (Table: table, Cell: cell);
    }

    private static int BisectLeft<T>(ImmutableArray<T> tokens, int search, Func<T, int> key, int low = 0)
    {
        int high = tokens.Length;

        while (low < high)
        {
            int mid = (low + high) / 2;

            if (key(tokens[mid]) < search)
                low = mid + 1;
            else
                high = mid;
        }

        return low;
    }

    private static int BisectRight<T>(ImmutableArray<T> tokens, int search, Func<T, int> key)
    {
        int low = 0;
        int high = tokens.Length;

        while (low < high)
        {
            int mid = (low + high) / 2;

            if (search < key(tokens[mid]))
                high = mid;
            else
                low = mid + 1;
        }

        return low;
    }

    public override string ToString()
    {
        return Utils.PrettyPrint(
            GetType(),
            this,
            "Text",
            "Tokens",
            "Tables"
        );
    }
}
