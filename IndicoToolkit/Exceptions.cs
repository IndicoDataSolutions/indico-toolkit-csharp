namespace IndicoToolkit;


/*
Thrown when an error occurs in Toolkit functionality.
*/
public class IndicoToolkitException : System.Exception
{
    public IndicoToolkitException(string message) : base(message) { }
}


/*
Thrown when an error occurs in traversing JSON in `Utils.Get()`.
*/
public class TraversalException : IndicoToolkitException
{
    public TraversalException(string message) : base(message) { }
}
