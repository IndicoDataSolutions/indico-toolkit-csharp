using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace IndicoToolkit.Results;


public enum FormExtractionType
{
    CHECKBOX,
    SIGNATURE,
    TEXT
}


public class FormExtraction : Extraction
{
    public FormExtractionType Type { get; set; }
    public bool Checked { get; set; }
    public bool Signed { get; set; }

    public int Top { get; set; }
    public int Left { get; set; }
    public int Right { get; set; }
    public int Bottom { get; set; }

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
    public static FormExtraction FromJson(Document document, ModelGroup model, Review? review, JToken json)
    {
        var normalized = Utils.Get<JObject>(json, "normalized");
        var structured = Utils.Get<JObject>(normalized, "structured");

        return new FormExtraction
        {
            Document = document,
            Model = model,
            Review = review,
            Label = Utils.Get<string>(json, "label"),
            Confidences = Utils.Get<Dictionary<string, double>>(json, "confidence"),
            Accepted = Utils.Has<bool>(json, "accepted") && Utils.Get<bool>(json, "accepted"),
            Rejected = Utils.Has<bool>(json, "rejected") && Utils.Get<bool>(json, "rejected"),
            Text = Utils.Get<string>(normalized, "formatted"),
            Page = Utils.Get<int>(json, "page_num"),
            Type = FormExtractionTypeFromString(Utils.Get<string>(json, "type")),
            Checked = Utils.Has<bool>(structured, "checked") && Utils.Get<bool>(structured, "checked"),
            Signed = Utils.Has<bool>(structured, "signed") && Utils.Get<bool>(structured, "signed"),
            Top = Utils.Get<int>(json, "top"),
            Left = Utils.Get<int>(json, "left"),
            Right = Utils.Get<int>(json, "right"),
            Bottom = Utils.Get<int>(json, "bottom"),
            Extras = json as JObject,
        };
    }

    // Create JSON for auto review changes.
    public override JObject ToJson()
    {
        Extras["label"] = Label;
        Extras["confidence"] = JObject.FromObject(Confidences);
        Extras["type"] = Type.ToString().ToLower();
        Extras["page_num"] = Page;
        Extras["top"] = Top;
        Extras["left"] = Left;
        Extras["right"] = Right;
        Extras["bottom"] = Bottom;

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
