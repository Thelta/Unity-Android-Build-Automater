using UnityEngine;
using System.Collections;
using System.Text;

public class ArgumentBuilder
{
    StringBuilder builder;

    char delimeter;

    public ArgumentBuilder()
    {
        builder = new StringBuilder();
        int p = (int)System.Environment.OSVersion.Platform;
        if ((p == 4) || (p == 6) || (p == 128))
        {
            delimeter = '|';
        }
        else
        {
            delimeter = ';';
        }
    }

    public void AddArgument(string arg)
    {
        builder.AppendFormat("{0} ", arg);
    }
    public void AddArgument(string arg, string option)
    {
        builder.AppendFormat("{0} ", arg);
        AddWrapperOrNot(option);

    }

    public void AddArgument(string arg, string[] options)
    {
        builder.AppendFormat("{0} ", arg);
        for (int i = 0; i < options.Length - 1; i++)
        {
            AddWrapperOrNot(options[i], delimeter);
        }

        AddWrapperOrNot(options[options.Length - 1]);

    }

    void AddWrapperOrNot(string option)
    {
        if (option.IndexOf(' ') < 0)
        {
            builder.AppendFormat("{0} ", option);
        }
        else
        {
            builder.AppendFormat("\"{0}\" ", option);
        }

    }

    void AddWrapperOrNot(string option, char delimeter)
    {
        if (option.IndexOf(' ') < 0)
        {
            builder.AppendFormat("{0}{1}", option, delimeter);
        }
        else
        {
            builder.AppendFormat("\"{0}\"{1}", option, delimeter);
        }
    }


    public string ToString()
    {
        return builder.ToString();
    }
}
