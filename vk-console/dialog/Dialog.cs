

using System;
using System.Text.RegularExpressions;

namespace vk_console
{
    class Dialog : IDialog
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
            if (Preview.Length > 90) {
                Preview = Preview.Substring(0, 90) + "...";
            }
            Preview = Regex.Replace(Preview, @"\t|\n|\r", "");
            return String.Format("{0} - {1}",Talker, Preview);  
        }
    }

}
