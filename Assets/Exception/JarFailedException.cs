using System;

public class JarFailedException : Exception
{
    public JarFailedException() { }
    public JarFailedException(string message) : base(message) { }
}
