using IndicoToolkit.Results;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;

namespace IndicoToolkit.EtlOutputs;

public record EtlOutput
(
    string Text,
    ImmutableList<string> TextOnPage,
    ImmutableList<Token> Tokens,
    ImmutableList<ImmutableList<Token>> TokensOnPage,
    ImmutableList<Table> Tables,
    ImmutableList<ImmutableList<Table>> TablesOnPage
)
{
    /*
    Load `etlOutputUri` as an `EtlOutput` record. A `reader` function must be
    supplied to read JSON and text strings from disk, storage API, or Indico client.

    Use `text`, `tokens`, and `tables` to specify what to load.
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
            textPages = ImmutableList<string>.Empty;

        if (tokens && Utils.Has<string>(pages, 0, "tokens"))
            tokenJsonPages = pages.Select(page => JArray.Parse(reader(Utils.Get<string>(page, "tokens"))));
        else
            tokenJsonPages = ImmutableList<JArray>.Empty;

        if (tables && Utils.Has<string>(pages, 0, "tables"))
            tableJsonPages = pages.Select(page => JArray.Parse(reader(Utils.Get<string>(page, "tables"))));
        else
            tableJsonPages = ImmutableList<JArray>.Empty;

        return FromPages(textPages, tokenJsonPages, tableJsonPages);
    }

    /*
    Load `etlOutputUri` as an `EtlOutput` record. A `reader` coroutine must be
    supplied to read JSON and text strings from disk, storage API, or Indico client.

    Use `text`, `tokens`, and `tables` to specify what to load.
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

    public static EtlOutput FromPages(
        IEnumerable<string> textPages,
        IEnumerable<JArray> tokenJsonPages,
        IEnumerable<JArray> tableJsonPages
    )
    {
        var tokenPages = tokenJsonPages
            .Select(page => page
                .Select(Token.FromJson)
                .OrderBy(token => token.Span)
                .ToImmutableList())
            .ToImmutableList();

        var tablePages = tableJsonPages
            .Select(page => page
                .Select(Table.FromJson)
                .OrderBy(table => table.Box)
                .ToImmutableList())
            .ToImmutableList();

        return new(
            string.Join("\n", textPages),
            textPages.ToImmutableList(),
            tokenPages.SelectMany(page => page).ToImmutableList(),
            tokenPages,
            tablePages.SelectMany(page => page).ToImmutableList(),
            tablePages
        );
    }
}
