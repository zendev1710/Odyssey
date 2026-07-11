using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Events;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Tools;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Odyssey.ViewModels.Tools;

public abstract partial class MessagesViewModel : DocumentToolViewModelBase
{
    [ObservableProperty]
    protected Node? selectedItem;

    private readonly Node _root = new();

    protected Node Root { get { return _root; } }

    public ObservableCollection<Node> Items { get; } = [];

    protected MessagesViewModel(IEventAggregator? eventAggregator = null) : base(eventAggregator)
    {
        SelectedItem = Root;
        Items = Root.Children;

        // TODO: use filter on filePath to update the list only if different
        EventAggregator?.GetEvent<ReportDocumentChangedEvent>().Subscribe(OnEventActiveDocumentChanged, ThreadOption.UIThread);
    }

    protected void Clear()
    {
        Items.Clear();
        SelectedItem = Root;
    }

    /// <summary>
    /// Dispatch the select node Event using the event aggregator.
    /// Normally called by a double-click or a space key pressed on the currently selected item of this tree messages list in the corresponding view.
    /// Event is published only if the selected item is linked to a DataBlock (selection state has changed).
    /// </summary>
    /// <param name="node"></param>
    public void DispatchSelectedNode(Node? node)
    {
        RevealInExplorer(node);
    }

    private void RevealInExplorer(Node? node)
    {
        // TODO: check if an unseen region can have a messageand then should be selected in map instead of explorer 
        MessageEntry? messageEntry = node?.Entry;
        DataBlock? item = messageEntry?.Selection?.GetDefaultTarget();
        DataBlock? region = messageEntry?.Selection?.Region;
        if (item != null || region != null)
        {
            ISelection sel = new SimpleItemSelection(item, region, null);
            SendSelectionChangedEvent(sel);
        }
    }

    private void OnEventActiveDocumentChanged(CRDocument cr)
    {
        OnActiveDocumentChanged(cr);
    }

    protected override void OnActiveDocumentChanged(CRDocument cr)
    {
        SetMapFile(cr);
    }

    // new because in original code it needs to override base method
    protected Node? AddMessage(Node group, in DataBlock? msgBlock)
    {
        if (msgBlock == null)
        {
            Debug.WriteLine("!!! AddMessage: message block is null");
            return null;
        }

        bool foundChild = false;
        string section = msgBlock.Value(Strings.EN_MESSAGE_SECTION);
        if (!string.IsNullOrEmpty(section))
        {
            string text = GetMessageSection(section);
            if (!string.IsNullOrEmpty(text))
            {
                string label = text;
                foreach (var child in group.Children)
                {
                    if (child.Header == label)
                    {
                        foundChild = true;
                        group = child;
                        break;
                    }
                }
                if (!foundChild)
                {
                    group = new Node(group, label, null);
                }
            }
        }

        List<DataBlock> targets = Report.GetMessageTargets(msgBlock);
        MessageEntry messageEntry = new MessageEntry(msgBlock.Value(Strings.EN_MESSAGE_RENDERED), targets);
        DataBlock? target = messageEntry.Selection?.GetDefaultTarget();
        if (target == null)
        {
            Debug.WriteLine("!!! AddMessage: message has no target");
        }
        return target != null ? AppendItem(group, msgBlock.Value(Strings.EN_MESSAGE_RENDERED), messageEntry) : null;
    }

    private static string GetMessageSection(in string section)
    {
        // TODO: what to do about this empty method ?
        /*
        if (section == "errors")
        {
            return "Fehler";
        }
        if (section == "magic")
        {
            return "Magie";
        }
        if (section == "production")
        {
            return "Produktion";
        }
        if (section == "movement")
        {
            return "Bewegungen";
        }
        if (section == "economy")
        {
            return "Wirtschaft";
        }
        if (section == "events")
        {
            return "Ereignisse";
        }
        if (section == "study")
        {
            return "Ausbildung";
        }
        return null;
        */
        return section;
    }

    protected static Node AppendItem(Node parent, string label, MessageEntry? entry) => new(parent, label, entry);

    protected static Node InsertItem(Node parent, string label, MessageEntry? entry, int index = -1) => new(parent, label, entry, index);

    public class Node
    {
        private ObservableCollection<Node>? _children;

        public Node()
        {
            _children = null;
            Entry = null;
            Header = "";
        }

        // TODO: add Targets property as an ITargets interface (target can be a unit, region, ship and so on)
        public Node(Node parent, string header, MessageEntry? entry /*DataBlock? block*/, int index = -1)
        {
            Parent = parent;
            Header = header;
            Entry = entry;
            parent.Insert(this, index);
        }

        public int Insert(Node child, int index = -1)
        {
            int ind = index == -1 ? Children.Count : index;
            Children.Insert(ind, child);
            return ind;
        }

        public Node? Parent { get; }
        public string Header { get; }
        //public DataBlock? Block { get; }
        public MessageEntry? Entry { get; }
        public ObservableCollection<Node> Children => _children ??= [];
        public bool HasChildren() => Children.Count > 0;

#if DEBUG
        public override string ToString() => Header;
#endif
    }
}
