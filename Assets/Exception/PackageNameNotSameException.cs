using System;

public class PackageNameNotSameException : Exception
{
    public PackageNameNotSameException() { }

    public PackageNameNotSameException(string message) : base(message) { }
}
