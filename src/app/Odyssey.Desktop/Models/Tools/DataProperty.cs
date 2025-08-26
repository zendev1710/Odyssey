using Avalonia;
using Odyssey.Models.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Odyssey.Models.Tools.DataProperty;
using System.Xml.Linq;

namespace Odyssey.Models.Tools
{
    public class DataProperty
    {
        /// <summary>
        /// The name of the property.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// The value of the property.
        /// </summary>
        public string Value { get; private set; }
        /// <summary>
        /// The label of the property, using if needed the translated resource string based on Name/Value pair.
        /// </summary>        
        public string Label { get; private set; } 
        /// <summary>
        /// The description of the property. Could be used for tooltips.
        /// </summary>
        public string? Description { get; private set; }
        /// <summary>
        /// The category of the property. Could be used ti customize the property item display.
        /// </summary>
        public Categories Category { get; private set; }
        /// <summary>
        /// The reference to the data block, if any.
        /// </summary>
        public DataBlock? Reference { get; private set; }

        public enum Categories
        {
            None = 0,
            Node,
            Skill,
            Item,
            Effect,
            Spell,
            CombatSpell,
            Status,
            CombatStatus,
            Building,
            Ship,
            Race,
        }
        public DataProperty(string name)
        {
            Category = Categories.Node;
            Name = name;
            Label = name;
            Value = string.Empty;
            Description = string.Empty;
        }

        public DataProperty(Categories category, string name, string value, string label = "", string? description = null, DataBlock? reference = null)
        {
            Category = category;
            Name = name;
            Value = value;
            Label = label;
            Description = description;
            Reference = reference;
        }
    }
}
