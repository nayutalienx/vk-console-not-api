﻿namespace vk_console
{


    partial class Cookies
    {
        partial class CookiesTableDataTable
        {
            public void saveToFile(string path)
            {
                AcceptChanges();
                WriteXml(path);
            }

            public void loadFromFile(string path)
            {
                ReadXml(path);
            }
        }
    }
}
