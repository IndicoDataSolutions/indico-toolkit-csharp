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
        var structured = Utils.Get<JObject>(json, "normalized", "structured");

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
            Checked = Utils.Has<bool>(structured, "checked") && Utils.Get<bool>(structured, "checked"),
            Signed = Utils.Has<bool>(structured, "signed") && Utils.Get<bool>(structured, "signed"),
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
            Extras["normalized"]["structured"]["checked"] = Checked;
            Extras["normalized"]["formatted"] = Checked ? "Checked" : "Unchecked";
        }
        else if (Type == FormExtractionType.SIGNATURE)
        {
            Extras["normalized"]["structured"]["signed"] = Signed;
            Extras["normalized"]["formatted"] = Signed ? "Signed" : "Unsigned";
        }
        else if (Type == FormExtractionType.TEXT)
        {
            Extras["normalized"]["formatted"] = Text;
        }

        if (Accepted)
            Extras["accepted"] = true;
        else if (Rejected)
            Extras["rejected"] = true;

        return Extras;
    }
}
