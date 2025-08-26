using AvaloniaEdit.Editing;
using Odyssey.Models.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Odyssey.Models.Documents.CRDocument;

namespace Odyssey.ViewModels
{
    public class SelectionChange : ISelectionChange
    {
        public ISelection Selection { get; private set; }
        public ISelector Selector { get; private set; }
        public ISelector? InnerSelector { get; private set; } = null;
        public SelectionChange(ISelection sel, ISelector selector, ISelector? innerSelector = null)
        {
            Selection = sel;
            Selector = selector;
            InnerSelector = innerSelector;
        }
    }
}
