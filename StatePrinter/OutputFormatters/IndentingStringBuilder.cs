using System;
using System.Text;
using StatePrinter.Configurations;

namespace StatePrinter.OutputFormatters
{
  class IndentingStringBuilder
  {
    readonly StringBuilder sb = new StringBuilder();
    readonly string IndentIncrement;

    string indent = "";

    public IndentingStringBuilder(string indentIncrement = Configuration.DefaultIndention)
    {
      IndentIncrement = indentIncrement;
    }

    public void Indent()
    {
      indent += IndentIncrement;
    }

    public void DeIndent()
    {
      indent = indent.Substring(IndentIncrement.Length);
    }

    public void AppendFormatLine(string format, params object[] args)
    {
      sb.Append(indent);
      sb.AppendFormat(format, args);
      sb.Append(Environment.NewLine);
    }

    public override string ToString()
    {
      return sb.ToString();
    }
  }
}
