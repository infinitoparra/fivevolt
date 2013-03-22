using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace DummyLoadController.Helpers
{
    public static class XElementHelpers
    {
        public static string GetElementTextOrDefault(this XElement elem, XName name)
        {
            XElement childElem = elem.Element(name);
            if (childElem == null)
            {
                return null;
            }
            return childElem.Value;
        }
    }
}
