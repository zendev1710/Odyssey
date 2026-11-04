using Dock.Model.Mvvm.Controls;
using System.Diagnostics;

namespace Odyssey.ViewModels
{
    /// <summary>
    /// View model for the home view.
    /// The home view is the root dock window, embeddiong all docked windows defining the layout.
    /// </summary>
    public class WorkspaceViewModel : RootDock
    {
        public WorkspaceViewModel()
        {
            Debug.WriteLine("[VM-WORKSPACE] Creation");
        }
    }
}
