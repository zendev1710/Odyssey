using System;
using System.Collections.Generic;
using System.Text;
using static Odyssey.Models.EntityModel;

namespace Odyssey.Models
{
    public interface ISelectable
    {
        public int Id { get; }
        public int LocationId { get; }
        public EntityType Type { get; }
    }
}
