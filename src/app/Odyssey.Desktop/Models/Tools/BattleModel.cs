using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Odyssey.Models.Tools
{
    public class BattleModel
    {
        private string? _content;
        private string? _label;
        private DataBlock? _region;
        private readonly string? _factionName;

        /// <summary>
        /// Battle description, typically containing the rendered messages from the battle.
        /// </summary>
        public string? Content { get { return _content; } private set { _content = value; } }

        /// <summary>
        /// Label for the battle, being the region's label, followed by the faction name.
        /// </summary>
        public string? Label { get { return _label; } private set { _label = value; } }

        /// <summary>
        /// The name of the faction involved in the battle, if applicable.
        /// The battle description is from the point of view of this faction (what happened to its units...).
        /// </summary>
        public string? FactionName { get { return _factionName; } }

        /// <summary>
        /// The region where the battle took place, represented as a <see cref="DataBlock"/>.
        /// </summary>
        public DataBlock? Region { get { return _region; } private set { _region = value; } }

        /// <summary>
        /// Report document containing the battle data.
        /// </summary>
        private readonly CRDocument _report;

        public CRDocument Report { get { return _report; } }

        public BattleModel(CRDocument report, string factionName)
        {
            _report = report;
            _factionName = factionName;
            Content = string.Empty;
            Label = string.Empty;
            Region = null;
        }

        /// <summary>
        /// Collects data from the specified battle block.
        /// </summary>
        /// <remarks>It updates the <c>Region</c> and <c>Label</c> properties with the region's UI label,
        /// optionally appending the faction name if available. It also collects message content from the battle block,
        /// storing it in the <c>Content</c> property.</remarks>
        /// <param name="battleBlock">The data block representing the battle from which to collect information.</param>
        /// <returns><see langword="true"/> if the data collection is successful and content is available; otherwise, <see
        /// langword="false"/>.</returns>
        public bool CollectData(DataBlock battleBlock)
        {
            DataBlock? region = null;
            // TODO: to check if a battle can be on an unknown region
            if (Report.GetSeenRegion(ref region, battleBlock))
            {
                Region = region;
                Label = region!.GetUILabel();
                if (!string.IsNullOrEmpty(FactionName))
                {
                    Label = $"{Label} [{FactionName}]";
                }
                DataBlock? messageBlock = null;
                if (CRDocument.GetSeenChild(ref messageBlock, battleBlock, BlockType.MESSAGE))
                //if (CRDocument.GetChild(ref messageBlock, battleBlock.Node, BlockType.MESSAGE))
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
                        // No more messages, exit loop
                        break;
                    }
                    Content = contentBuilder.ToString();
                }
                return !string.IsNullOrEmpty(Content);
            }
            else
            {
                // Battle on an unknown region ?
                Debug.WriteLine("[BATTLE] WARNING | Battle on an unknown region !");
                Region = null;
                Label = !string.IsNullOrEmpty(FactionName) ? $"[?] [{FactionName}]" : "[?]";
                Content = string.Empty;
                return false;
            }
            return false;
        }

        public override string ToString()
        {
            return Label!;
        }
    }
}
