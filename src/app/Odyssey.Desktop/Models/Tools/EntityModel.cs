using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Localization;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Odyssey.Models.Tools.DataProperty;

namespace Odyssey.Models.Tools
{
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
