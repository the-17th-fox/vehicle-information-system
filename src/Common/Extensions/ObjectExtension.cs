using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Common.Utilities;

namespace Common.Extensions
{
    public static class ObjectExtension
    {
        public static string AllPropertiesValues(this object obj)
        {
            var properties = obj.GetType().GetProperties();
            var sb = new StringBuilder();

            foreach (var prop in properties)
                sb.Append($@"{prop.Name}: [{prop.GetValue(obj)}] ");

            return sb.ToString();
        }

        public static string FilledPropertiesValues(this object obj)
        {
            var properties = PropertyLookup.GetFilledProperties(obj);
            var sb = new StringBuilder();

            foreach (var prop in properties)
                sb.Append($@"{prop.Name}: [{prop.GetValue(obj, null)}] ");

            return sb.ToString();
        }

    }
}
