using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odyssey.Events
{
    public class EventMessage
    {
        private readonly MessageType _type;
        private readonly MessageId _id;
        private readonly object? _data;
        public MessageType Type { get { return _type;  } }
        public MessageId Id { get { return _id; } }
        public object? Data { get { return _data; } }

        public EventMessage(MessageType type, MessageId Id, object ? data)
        {
            _type = type;
            _id = Id;
            _data = data;
        }

    }
}
