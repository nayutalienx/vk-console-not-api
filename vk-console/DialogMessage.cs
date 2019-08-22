using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vk_console
{
    class DialogMessage
    {
        public string UserId { get; set; }
        public string PeerId { get; set; }
        public string Name { get; set; }

        public string Text { get; set; }

        public DialogMessage(string userId, string peerId, string name, string text) {
            UserId = userId;
            PeerId = peerId;
            Name = name;
            Text = text;
        }
    }
}
