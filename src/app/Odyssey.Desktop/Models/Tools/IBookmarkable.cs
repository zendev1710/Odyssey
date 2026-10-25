using System;
using System.Collections.Generic;
using System.Text;

namespace Odyssey.Models.Tools
{
    public interface IBookmarkable
    {
        string GetStringId();
        string GetUIName();
        string GetUILabel();
    }
}
