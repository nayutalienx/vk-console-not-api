using System;

namespace vk_console
{
    class DialogMessage : IDialog
    {

        public string Name { get; set; }

        public string Date { get; set; }

        public string Attaches { get; set; }
        public string Text { get; set; }

        public DialogMessage(string name, string text, string date, string attaches)
        {
            Date = date;
            Attaches = attaches;
            Name = name;
            Text = text;
        }

        public override string ToString()
        {
            return String.Format("[{0,-15}] {1,-14}: {2}", Date, Name, Text + " " + Attaches);
        }
    }
}

