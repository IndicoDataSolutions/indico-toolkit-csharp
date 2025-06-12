namespace IndicoToolkit.EtlOutputs;


/*
Thrown when an error occurs accessing `EtlOutput` values.
*/
public class EtlOutputException : System.Exception
{
    public EtlOutputException(string message) : base(message) { }
}


/*
Thrown when a `Token` can't be found for a `Span`.
*/
public class TokenNotFoundException : EtlOutputException
{
    public TokenNotFoundException(string message) : base(message) { }
}


/*
Thrown when a `Table` and `Cell` can't be found for a `Token`.
*/
public class TableCellNotFoundException : EtlOutputException
{
    public TableCellNotFoundException(string message) : base(message) { }
}
