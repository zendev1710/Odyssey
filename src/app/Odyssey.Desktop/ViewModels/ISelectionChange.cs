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
    /// <summary>
    /// Represents an object that can be selected and provides information about its selection state and associated
    /// selectors.
    /// </summary>
    /// <remarks>This interface defines properties to access the current selection state and the selectors
    /// responsible for managing selection.</remarks>
    public interface ISelectionChange
    {
        /// <summary>
        /// Gets the current selection.
        /// </summary>
        public ISelection Selection { get; }

        /// <summary>
        /// Gets the selector used to determine the appropriate strategy or behavior.
        /// </summary>
        public ISelector Selector { get; }

        /// <summary>
        /// Gets the inner selector used for additional selection logic.
        /// </summary>
        public ISelector? InnerSelector { get; }

    }
}
