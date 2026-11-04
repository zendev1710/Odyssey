using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.Shapes;
using Odyssey.Models.Documents;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Odyssey.Models.Data
{
    public enum FactionStatus
    {
        UNKNOWN = 0,
        ANONYMOUS = -1,
        TRAITOR = -2,
        ACTIVE = 1,
        ALLIED = 2,
    }

}
