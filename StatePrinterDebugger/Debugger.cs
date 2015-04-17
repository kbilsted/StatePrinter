using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StatePrinterDebugger.Gui;

namespace StatePrinterDebugger
{
    public class Debugger
    {
        TabAdder tabs;

        internal Debugger(TabAdder tabs)
        {
        }

        public void AddTab(string outerTabName, string innerTabName, string contentHeader, string content)
        {
            if (outerTabName == null) throw new ArgumentNullException("outerTabName");
            if (innerTabName == null) throw new ArgumentNullException("innerTabName");
            if (content == null) throw new ArgumentNullException("content");
        }
    }
}
