using System;

public class JavacCompileErrorException : Exception
{
    public JavacCompileErrorException() { }

    public JavacCompileErrorException(string message) : base(message)
    { }

}
