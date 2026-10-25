using Odyssey.Models.Data;
using Odyssey.Models.Localization;
using Odyssey.Models.Tools;
using System.Collections.Generic;
using static Odyssey.Models.Tools.DataProperty;

namespace Odyssey.Models
{
    /// <summary>
    /// Represents a generic entity in the game world.
    /// It can be a region, a ship, a building or a unit.
    /// An entity is defined by its DataBlock in the CRDocument. It has properties (children DataBlocks).
    /// </summary>
    public class EntityModel : ISelectable
    {
        public enum EntityType
        {
            Unknown,
            Region,
            Unit,
            Ship,
            Building,
            Island
        }

        private readonly DataBlock _dataBlock;
        private readonly List<DataProperty> _dataProperties;
        protected int _id;
        protected int _locationId;
        private readonly EntityType _type;
        protected string _encodedId;

        // TODO: set as protected when code will be refactored
        public DataBlock DataBlock { get { return _dataBlock; } }
        //protected DataBlock DataBlock { get { return _dataBlock; } }

        public string EncodedId { get { return _encodedId; } }
        public int Id { get { return _id; } }
        public EntityType Type { get { return _type; } }

        public int LocationId { get { return _locationId; } }

        public List<DataProperty> DataProperties { get { return _dataProperties; } }


        /*
        public EntityModel(in RegionDataBlock dataBlock, int locationId)
        {
            _dataBlock = dataBlock;
            _id = dataBlock.GetId();
            _locationId = locationId;
            _type = SetType(dataBlock);
            _dataProperties = [];
        }
        */

        public EntityModel(in DataBlock dataBlock, int locationId)
        {
            _dataBlock = dataBlock;
            _id = dataBlock.GetId();
            _locationId = locationId;
            _type = SetType(dataBlock);
            _dataProperties = [];
            _encodedId = Utils.Converters.IdToString(_id);
        }

        private static EntityType SetType(in DataBlock dataBlock)
        {
            switch (dataBlock.GetBlockType())
            {
                case BlockType.REGION:
                    return EntityType.Region;
                case BlockType.UNIT:
                    return EntityType.Unit;
                case BlockType.SHIP:
                    return EntityType.Ship;
                case BlockType.BUILDING:
                    return EntityType.Building;
                case BlockType.ISLAND:
                    return EntityType.Island;
            }
            return EntityType.Unknown;
        }

        protected bool AddNameValueProperty(string name, string value, in EntityModel? reference = null/*DataBlock? reference = null*/)
        {
            string label = Labels.Localize(Categories.Node, name);
            DataProperties.Add(new DataProperty(Categories.None, name, value, label, string.Empty, reference));
            return true;
        }

        protected virtual bool AddDataProperty(DataKey dk)
        {
            return AddNameValueProperty(dk.GetKeyFromType(), dk.GetValue());
        }

#if DEBUG
        public override string ToString()
        {
            return _dataBlock.ToString();
        }
#endif
    }
}
