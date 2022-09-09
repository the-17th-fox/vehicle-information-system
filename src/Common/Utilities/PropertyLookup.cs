using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utilities
{
    public class PropertyLookup
    {
        public static IEnumerable<PropertyInfo> GetFilledProperties(object obj)
        {
            return obj.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead)
                .Select(p => new { Property = p, Value = p.GetValue(obj, null) })
                .Where(a => a.Value != null && !string.IsNullOrEmpty(a?.Value?.ToString()))
                .Select(a => a.Property);
        }
    }
}
