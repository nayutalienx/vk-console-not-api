using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace vk_console
{
    class DialogMessage : IDialog
    {

        public string Name { get; set; }

        public string Date { get; set; }

        public string Attaches { get; set; }
        public string Text { get; set; }

        public Dictionary<string,string> Docs { get; set; }

        public DialogMessage(string name, string text, string date, string attaches, Dictionary<string,string> docs)
        {
            Date = date;
            Attaches = attaches;
            Name = name;
            Text = text;
            Docs = docs;
        }

        public override string ToString()
        {
            Text = Regex.Replace(Text, @"\t|\n|\r", "");
            string documents = "";
            if (Attaches.Contains("doc")) {
                documents += "\nДокументы:\n";
                foreach (KeyValuePair<string, string> kv in Docs) {
                    documents += kv.Value+"\n";
                }
            }
            return String.Format("[{0,-15}] {1,-14}: {2}", Date, Name, Text + " " + Attaches + documents);
        }
    }
}

