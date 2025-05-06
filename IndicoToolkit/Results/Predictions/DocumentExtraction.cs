using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace IndicoToolkit.Results;


public class DocumentExtraction : Extraction
{
    public int Start { get; set; }
    public int End { get; set; }
    public HashSet<Group> Groups { get; set; }

    // Create an `DocumentExtraction` from a prediction object.
    public static DocumentExtraction FromJson(Document document, ModelGroup model, Review? review, JToken json)
    {
        var normalized = Utils.Get<JObject>(json, "normalized");
        var span = Utils.Get<JArray>(json, "spans").First;

        return new DocumentExtraction
        {
            Document = document,
            Model = model,
            Review = review,
            Label = Utils.Get<string>(json, "label"),
            Confidences = Utils.Get<Dictionary<string, double>>(json, "confidence"),
            Accepted = Utils.Has<bool>(json, "accepted") && Utils.Get<bool>(json, "accepted"),
            Rejected = Utils.Has<bool>(json, "rejeted") && Utils.Get<bool>(json, "rejeted"),
            Text = Utils.Get<string>(normalized, "formatted"),
            Page = Utils.Get<int>(span, "page_num"),
            Start = Utils.Get<int>(span, "start"),
            End = Utils.Get<int>(span, "end"),
            Groups = new HashSet<Group>(
                Utils.Get<JArray>(json, "groupings")
                    .Select(value => Group.FromJson(value))
            ),
            Extras = json as JObject,
        };
    }

    // Create JSON for auto review changes.
    public override JObject ToJson()
    {
        Extras["label"] = Label;
        Extras["confidence"] = JObject.FromObject(Confidences);
        Extras["normalized"]["formatted"] = Text;
        Extras["spans"][0]["page_num"] = Page;
        Extras["spans"][0]["start"] = Start;
        Extras["spans"][0]["end"] = End;
        Extras["groupings"] = new JArray(Groups.Select(group => group.ToJson()));

        if (Accepted)
            Extras["accepted"] = true;
        else if (Rejected)
            Extras["rejeted"] = true;

        return Extras;
    }
}
