using System;

public class OptionsNotFoundException : Exception
{
    public OptionsNotFoundException(){}

    public OptionsNotFoundException(string message) : base(message)
    {

    }
}
