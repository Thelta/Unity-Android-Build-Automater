using UnityEngine;
using System.Collections;
using System.Text;

public class ArgumentBuilder
{
    StringBuilder builder;

    string delimeter;

    public ArgumentBuilder()
    {
        int p = (int)System.Environment.OSVersion.Platform;
        if ((p == 4) || (p == 6) || (p == 128))
        {
            delimeter = "|";
        }
        else
        {
            delimeter = ";";
        }
    }

    public void AddArgument(string arg)
    {
        builder.AppendFormat("{0} ", arg);
    }
    public void AddArgument(string arg, string option)
    {
        builder.AppendFormat("{0} ", arg);
        builder.AppendFormat("{0} ", option);
    }

    public void AddArgument(string arg, string[] options)
    {
        builder.AppendFormat("{0} ", arg);
        for (int i = 0; i < options.Length; i++)
        {
            builder.AppendFormat("{0} {1} ", options[i], delimeter);
        }
    }
    public string ToString()
    {
        return builder.ToString();
    }
}
