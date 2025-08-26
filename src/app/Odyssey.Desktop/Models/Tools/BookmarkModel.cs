using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using System;
using System.Collections.Generic;
using System.Text;

namespace Odyssey.Models.Tools
{
    public class BookmarkModel
    {
        private string? _content;
        private string? _label;
        private DataBlock? _region;
        private readonly string? _factionName;

        public string? Content { get { return _content; } private set { _content = value; } }
        public string? Label { get { return _label; } private set { _label = value; } }
        public string? FactionName { get { return _factionName; } }
        public DataBlock? Region { get { return _region; } private set { _region = value; } }

        private readonly CRDocument _report;

        public CRDocument Report { get { return _report; } }

        public BookmarkModel(CRDocument report, string factionName)
        {
            _report = report;
            _factionName = factionName;
            Content = string.Empty;
            Label = string.Empty;
            Region = null;
        }

        public bool CollectData(DataBlock battleBlock)
        {
            DataBlock? region = null;
            // TODO: what about abookmark on an entity located in a unknown region ?
            // TODO: check how it behaviours in Magellan : 
            // - possible to bookmark an unknown region ?
            // - is bookmark usable when region becomes unknown ?
            // TODO
            /*
            if (Report.GetRegion(ref region, battleBlock))
            {
                Region = region;
                Label = region!.GetUILabel();
                if (!string.IsNullOrEmpty(FactionName))
                {
                    Label = $"{Label} [{FactionName}]";
                }
                DataBlock? messageBlock = null;
                if (CRDocument.GetChild(ref messageBlock, battleBlock.Node, BlockType.MESSAGE))
                {
                    StringBuilder contentBuilder = new();
                    LinkedListNode<DataBlock>? node = messageBlock!.Node;
                    while (node != null)
                    {
                        contentBuilder.Append(messageBlock.Value(Strings.EN_MESSAGE_RENDERED));
                        contentBuilder.Append(Environment.NewLine);
                        LinkedListNode<DataBlock>? nextNode = node;
                        if (CRDocument.GetNext(ref nextNode, BlockType.MESSAGE))
                        {
                            node = nextNode;
                            messageBlock = nextNode!.Value;
                            continue;
                        }
                        // no more messages, exit loop
                        break;
                    }
                    Content = contentBuilder.ToString();
                }
                return !string.IsNullOrEmpty(Content);
            }
        */
            return false;
        }

        public override string ToString()
        {
            return Label!;
        }
    }
}
