using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StatePrinter.OutputFormatters
{
  class IndentingStringBuilder
  {
    StringBuilder sb = new StringBuilder();

    /// <summary>
    /// Specifies how indentation is done. 
    /// </summary>
    readonly string IndentIncrement = "    ";

    string indent = "";

    public IndentingStringBuilder(string indentIncrement)
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
