using System;

public class HaveDuplicateClassFile : Exception
{
    public HaveDuplicateClassFile() { }

    public HaveDuplicateClassFile(string message) : base(message) { }

}
