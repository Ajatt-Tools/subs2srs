/********************************************************************
 *
 *  PropertyBag.cs
 *  --------------
 *  Copyright (C) 2002  Tony Allowatt
 *  Copyright (C) 2026  fkzys (GTK4/.NET 10 port)
 *  Last Update: 12/14/2002
 * 
 *  THE SOFTWARE IS PROVIDED BY THE AUTHOR "AS IS", WITHOUT WARRANTY
 *  OF ANY KIND, EXPRESS OR IMPLIED. IN NO EVENT SHALL THE AUTHOR BE
 *  LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OF THIS
 *  SOFTWARE.
 * 
 *  Public types defined in this file:
 *  ----------------------------------
 *  namespace Flobbster.Windows.Forms
 *     class PropertySpec
 *     class PropertySpecEventArgs
 *     delegate PropertySpecEventHandler
 *     class PropertyBag
 *        class PropertyBag.PropertySpecCollection
 *     class PropertyTable
 *
 ********************************************************************/

using System;
using System.Collections.Generic;

namespace subs2srs
{
    /// <summary>
    /// Represents a single property specification with metadata.
    /// </summary>
    public class PropertySpec
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public object DefaultValue { get; set; }

        public PropertySpec(string name, Type type) : this(name, type, null, null, null) { }
        public PropertySpec(string name, Type type, string category) : this(name, type, category, null, null) { }
        public PropertySpec(string name, Type type, string category, string description) : this(name, type, category, description, null) { }

        public PropertySpec(string name, Type type, string category, string description, object defaultValue)
        {
            Name = name;
            Category = category;
            Description = description;
            DefaultValue = defaultValue;
        }
    }


    /// <summary>
    /// A table of named properties with metadata and values.
    /// Simplified from the original PropertyBag/ICustomTypeDescriptor
    /// (which was needed for WinForms PropertyGrid, unused in GTK).
    /// </summary>
    public class PropertyTable
    {
        private readonly List<PropertySpec> _properties = new();
        private readonly Dictionary<string, object> _values = new();

        public List<PropertySpec> Properties => _properties;

        public object this[string key]
        {
            get => _values.TryGetValue(key, out var v) ? v : null;
            set => _values[key] = value;
        }
    }
}
