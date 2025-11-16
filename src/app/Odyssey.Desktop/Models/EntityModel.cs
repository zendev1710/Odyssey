using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Localization;
using System.Collections.Generic;
using static Odyssey.Models.Tools.DataProperty;
using Odyssey.Models.Tools;

namespace Odyssey.Models
{
    /// <summary>
    /// Represents a generic entity in the game world.
    /// It can be a region, a ship, a building or a unit.
    /// An entity is defined by its DataBlock in the CRDocument. It has properties (children DataBlocks).
    /// </summary>
    public class EntityModel
    {
        private readonly DataBlock _dataBlock;
        private readonly List<DataProperty> _dataProperties;

        protected DataBlock DataBlock { get { return _dataBlock; } }
        public List<DataProperty> DataProperties { get { return _dataProperties; } }


        public EntityModel(DataBlock dataBlock)
        {
            _dataBlock = dataBlock;
            _dataProperties = [];
        }

        protected bool AddNameValueProperty(string name, string value, DataBlock? reference = null)
        {
            string label = Labels.Localize(Categories.Node, name);
            DataProperties.Add(new DataProperty(Categories.None, name, value, label, string.Empty, reference));
            return true;
        }

        protected virtual bool AddDataProperty(CRDocument report, DataKey dk)
        {
            return AddNameValueProperty(dk.GetKeyFromType(), dk.GetValue());
        }
    }
}
