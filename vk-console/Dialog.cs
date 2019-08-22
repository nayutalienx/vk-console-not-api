using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vk_console
{
    class Dialog
    {
        public string Peer { get; set; }
        public string Talker { get; set; }
        public string Preview { get; set; }

        public Dialog(string peer, string talker, string preview) {
            Peer = peer;
            Talker = talker;
            Preview = preview;
        }

        public override string ToString() {
            return $"{Talker} - {Preview}";
        }
    }
}
