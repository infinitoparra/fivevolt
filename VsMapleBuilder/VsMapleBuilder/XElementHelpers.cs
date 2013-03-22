using System.Xml.Linq;

namespace VsMapleBuilder
{
    public static class XElementHelpers
    {
        public static string AttributeValue(this XElement elem, XName attrName)
        {
            var attr = elem.Attribute(attrName);
            if (attr == null)
            {
                return null;
            }
            return attr.Value;
        }
    }
}
