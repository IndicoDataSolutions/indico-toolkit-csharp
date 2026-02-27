namespace IndicoToolkit.Results;


public abstract record Extraction : Prediction
{
    public required string Text { get; set; }
    public bool Accepted { get; protected set; }
    public bool Rejected { get; protected set; }

    public abstract int Page { get; }

    public void Accept()
    {
        Accepted = true;
        Rejected = false;
    }

    public void Unaccept()
    {
        Accepted = false;
    }

    public void Reject()
    {
        Accepted = false;
        Rejected = true;
    }

    public void Unreject()
    {
        Rejected = false;
    }

    public override string ToString()
    {
        return Utils.PrettyPrint(
            GetType(),
            this,
            "Document",
            "Task",
            "Review",
            "Label",
            "Confidence",
            "Text",
            "Accepted",
            "Rejected"
        );
    }
}
