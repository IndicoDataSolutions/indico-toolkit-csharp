namespace IndicoToolkit.EtlOutputs;


public class EtlOutputException : System.Exception
{
    public EtlOutputException(string message) : base(message) { }
}


public class TokenNotFoundException : EtlOutputException
{
    public TokenNotFoundException(string message) : base(message) { }
}


public class TableCellNotFoundException : EtlOutputException
{
    public TableCellNotFoundException(string message) : base(message) { }
}
