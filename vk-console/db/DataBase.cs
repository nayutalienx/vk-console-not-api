
using System;
using vk_console.db;

namespace vk_console
{
    class DataBase
    {
        public static Cookies.CookiesTableDataTable db = new Cookies.CookiesTableDataTable();
        public const string PATH = "data.dat";

        public static void Write(string name, Object obj) {
            try
            {
                Cookies.CookiesTableRow row = db.NewCookiesTableRow();
                row["Name"] = name;
                row["Value"] = obj;
                Cookies.CookiesTableRow ctr = db.FindByName(name);
                if (ctr == null){
                    db.Rows.Add(row);
                } else
                {
                    ctr["Value"] = obj;
                }                        
            }
            catch (Exception e) {
                db.RejectChanges();
                Console.WriteLine(e.Message);
            }
        }

        public static Object Read(string name) {
            try
            {
                return db.FindByName(name)["Value"];
            }
            catch (Exception e) {
                return null;
            }
        }

        public static void Save() {
            db.saveToFile(PATH);
        }

        public static void Load() {
            db.loadFromFile(PATH);
        }

        public static void Print()
        {
            foreach (System.Data.DataRow r in db)
            {
                Console.WriteLine($"{r["Name"]} - {r["Value"]}");
            }
        }

        public static void Delete(string name) {
            db.FindByName(name).Delete();
        }
        
    }
}
