namespace IndicoToolkit.Results;


public abstract class Extraction : Prediction
{
    public string Text { get; set; }
    [NoPrint]
    public bool Accepted { get; protected set; }
    [NoPrint]
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
}
