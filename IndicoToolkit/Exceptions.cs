namespace IndicoToolkit;


/*
Thrown when an error occurs in Toolkit functionality.
*/
public class IndicoToolkitException : System.Exception
{
    public IndicoToolkitException(string message) : base(message) { }
}


/*
Thrown when an error occurs accessing `EtlOutput` values.
*/
public class EtlOutputException : IndicoToolkitException
{
    public EtlOutputException(string message) : base(message) { }
}


/*
Thrown when an error occurs loading a result file.
E.g. an unsupported result file version.
*/
public class ResultException : IndicoToolkitException
{
    public ResultException(string message) : base(message) { }
}


/*
Thrown when an error occurs in traversing JSON in `Utils.Get()`.
*/
public class TraversalException : IndicoToolkitException
{
    public TraversalException(string message) : base(message) { }
}
