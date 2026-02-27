namespace IndicoToolkit;


/*
Thrown when an error occurs in Toolkit functionality.
*/
public class IndicoToolkitException(string message) : Exception(message) { }

/*
Thrown when an error occurs accessing `EtlOutput` values.
*/
public class EtlOutputException(string message) : IndicoToolkitException(message) { }

/*
Thrown when an error occurs loading a result file.
E.g. an unsupported result file version.
*/
public class ResultException(string message) : IndicoToolkitException(message) { }

/*
Thrown when an error occurs in traversing JSON in `Utils.Get()`.
*/
public class TraversalException(string message) : IndicoToolkitException(message) { }
