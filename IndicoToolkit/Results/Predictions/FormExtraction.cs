using Newtonsoft.Json.Linq;

namespace IndicoToolkit.Results;


public enum FormExtractionType
{
    CHECKBOX,
    SIGNATURE,
    TEXT
}


public record FormExtraction : Extraction
{
    public FormExtractionType Type { get; set; }
    public Box Box { get; set; }
    public bool Checked { get; set; }
    public bool Signed { get; set; }

    public override int Page => Box.Page;

    // Determine the form extraction type of a prediction from its string representation.
    public static FormExtractionType FormExtractionTypeFromString(string formExtractionType)
    {
        if (formExtractionType == "checkbox")
            return FormExtractionType.CHECKBOX;
        else if (formExtractionType == "signature")
            return FormExtractionType.SIGNATURE;
        else if (formExtractionType == "text")
            return FormExtractionType.TEXT;
        else
            throw new ResultException($"unsupported form extraction type `{formExtractionType}`");
    }

    // Create a `FormExtraction` from a prediction JSON.
    public static new FormExtraction FromJson(Document document, Model model, Review? review, JToken json)
    {
        return new()
        {
            Document = document,
            Model = model,
            Review = review,
            Label = Utils.Get<string>(json, "label"),
            Confidences = Utils.Get<Dictionary<string, double>>(json, "confidence"),
            Text = Utils.Get<string>(json, "normalized", "formatted"),
            Accepted = Utils.Has<bool>(json, "accepted") && Utils.Get<bool>(json, "accepted"),
            Rejected = Utils.Has<bool>(json, "rejected") && Utils.Get<bool>(json, "rejected"),
            Type = FormExtractionTypeFromString(Utils.Get<string>(json, "type")),
            Box = Box.FromJson(json),
            Checked = (
                Utils.Has<bool>(json, "normalized", "structured", "checked")
                && Utils.Get<bool>(json, "normalized", "structured", "checked")
            ),
            Signed = (
                Utils.Has<bool>(json, "normalized", "structured", "signed")
                && Utils.Get<bool>(json, "normalized", "structured", "signed")
            ),
            Extras = json as JObject,
        };
    }

    // Create JSON for auto review changes.
    public override JObject ToJson()
    {
        Extras["label"] = Label;
        Extras["confidence"] = JObject.FromObject(Confidences);
        Extras["type"] = Type.ToString().ToLower();
        Extras["page_num"] = Box.Page;
        Extras["top"] = Box.Top;
        Extras["left"] = Box.Left;
        Extras["right"] = Box.Right;
        Extras["bottom"] = Box.Bottom;

        if (Type == FormExtractionType.CHECKBOX)
        {
            Extras["normalized"]["structured"] = new JObject { ["checked"] = Checked };
            var text = Checked ? "Checked" : "Unchecked";
            Extras["normalized"]["formatted"] = text;
            Extras["normalized"]["text"] = text;
            Extras["text"] = text;
        }
        else if (Type == FormExtractionType.SIGNATURE)
        {
            Extras["normalized"]["structured"] = new JObject { ["signed"] = Signed };
            var text = Signed ? "Signed" : "Unsigned";
            Extras["normalized"]["formatted"] = text;
            // Don't overwrite the text of the signature stored in these attributes.
            // Extras["normalized"]["text"] = text;
            // Extras["text"] = text;
        }
        else if (
            Type == FormExtractionType.TEXT
            && Text != Utils.Get<string>(Extras, "normalized", "formatted")
        )
        {
            Extras["normalized"]["formatted"] = Text;
            Extras["normalized"]["text"] = Text;
            Extras["text"] = Text;
        }

        if (Accepted)
            Extras["accepted"] = true;
        else if (Rejected)
            Extras["rejected"] = true;

        return Extras;
    }
}
