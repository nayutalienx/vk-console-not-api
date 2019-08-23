
using CsQuery;
using Leaf.xNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace vk_console
{

    class Program
    {
        public const string commands =
            "\n### Команды:\n" +
            "### messages [получить список диалогов]\n" +
            "### Имя Фамилия [получить сообщения диалога]\n" +
            "### ~текст сообщения [отправить сообщение в последний посещенный диалог]\n" +
            "### reset [выйти из аккаунта]\n" +
            "### exit [выйти и СОХРАНИТЬ ВСЕ ДАННЫЕ, иначе придется вводить все заново]";
        public static string currentPeer = null;
        static void Main(string[] args)
        {
            DataBase.Load();

            if (DataBase.Read("loginFlag") == null || DataBase.Read("loginFlag").Equals("false"))
            {
                Console.WriteLine("Введите логин:");
                DataBase.Write("login", Console.ReadLine());
                Console.WriteLine("Введите пароль:");
                DataBase.Write("password", Console.ReadLine());
                DataBase.Write("loginFlag", "true");
            }

            string login = DataBase.Read("login").ToString();
            string password = DataBase.Read("password").ToString();
            PGAuth auth = new PGAuth(login, password);
            bool uiFlag = true;
            List<Dialog> dialogs = null;

            if (DataBase.Read("authFlag") == null || DataBase.Read("authFlag").Equals("false"))
            {
                auth.ParsDataAuth();
                try
                {
                    auth.Auth();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Ошибка авторизации. Нажмите любую кнопку чтобы закрыть программу и ввести данные снова.");
                    DataBase.Write("loginFlag", "false");
                    DataBase.Write("authFlag", "false");
                    DataBase.Save();
                    Console.ReadKey();
                    return;
                }
                Console.WriteLine("Авторизация завершена.");
                DataBase.Write("authFlag", "true");
            }



            while (uiFlag)
            {
                Console.WriteLine($"{commands}\n\nВведите команду:");
                string command = Console.ReadLine();
                switch (command)
                {

                    case "messages":
                        try
                        {
                            auth.GetDialogs();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Перезапустите программу чтобы обновить данные.");
                            DataBase.Write("loginFlag", "false");
                            DataBase.Write("authFlag", "false");
                            uiFlag = false;
                            break;
                        }

                        dialogs = parseDialogs(readDialogHtmlFromJson());
                        foreach (Dialog v in dialogs)
                            Console.WriteLine(v);
                        break;
                    case "reset":
                        DataBase.Write("login", "");
                        DataBase.Write("password", "");
                        DataBase.Write("loginFlag", "false");
                        DataBase.Write("authFlag", "false");
                        Console.WriteLine("Данные стерты. Перезапустите программу чтобы войти заново.");
                        uiFlag = false;
                        break;
                    case "exit":
                        uiFlag = false;
                        break;
                    default:
                        List<DialogMessage> messages = null;
                        if (command[0] == '~')
                        {
                            string message = command.Substring(1);
                            if (currentPeer == null)
                            {
                                Console.WriteLine("Зайдите в нужный диалог и повторите команду.");
                                break;
                            }
                            auth.SendMessage(message, currentPeer);
                            auth.GetTalker(currentPeer);
                            messages = readMessagesFromJson(DataBase.Read("TalkerResponse").ToString());
                            foreach (DialogMessage v in messages)
                                Console.WriteLine(v);
                            break;
                        }
                        if (dialogs == null)
                        {
                            Console.WriteLine("В начале нужно получить список диалогов.");
                            break;
                        }
                        foreach (Dialog v in dialogs)
                        {
                            if (v.Talker.Trim() == command.Trim())
                            {
                                auth.GetTalker(v.Peer.Trim());
                                break;
                            }
                        }
                        messages = readMessagesFromJson(DataBase.Read("TalkerResponse").ToString());
                        foreach (DialogMessage v in messages)
                            Console.WriteLine(v);

                        break;
                }
            }

            DataBase.Save();
            Console.WriteLine("Нажмите любую кнопку чтобы выйти.");
            Console.ReadKey();

        }

        public static string readDialogHtmlFromJson()
        {
            string json = DataBase.Read("DialogResponse").ToString();
            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            while (reader.Read())
            {
                if (reader.Value != null && reader.Value.Equals("html"))
                {
                    reader.Read();
                    return reader.Value.ToString();
                }
            }
            return null;


        }

        public static List<DialogMessage> readMessagesFromJson(string json)
        {

            List<DialogMessage> result = new List<DialogMessage>();
            dynamic stuff = Newtonsoft.Json.Linq.JObject.Parse(json);

            string userId = stuff.data[0].cur.userId;
            string peerId = stuff.data[0].cur.peerId;

            //  to send
            currentPeer = peerId;
            DataBase.Write("_af", stuff.data[0].cur._af.ToString());
            string peersJson = stuff.data[0].peers.ToString();
            JObject peersObject = JObject.Parse(peersJson);
            string userJson = peersObject[peerId].ToString();
            JObject userObject = JObject.Parse(userJson);
            string hashSend = userObject["hashSend"].ToString();
            DataBase.Write("hashSend", hashSend);


            string membersJson = stuff.data[0].members.ToString();

            Dictionary<string, string> memberDict = new Dictionary<string, string>();
            JObject o = JObject.Parse(membersJson);
            foreach (KeyValuePair<string, JToken> property in o)
            {
                JObject member = JObject.Parse(property.Value.ToString());
                memberDict.Add(property.Key, member["name"].ToString());
            }
            string messageJson = stuff.data[0].msgs.ToString();
            o = JObject.Parse(messageJson);

            foreach (KeyValuePair<string, JToken> property in o)
            {
                JObject message = JObject.Parse(property.Value.ToString());
                string name = memberDict[message["authorId"].ToString()];
                string attachesText = "";
                if (message["attaches"].Type == JTokenType.Object)
                {
                    JObject attachesObject = (JObject)message["attaches"];
                    string temp = attachesObject.ToString();
                    attachesText += temp.Substring(5, temp.Length - 7);
                }

                result.Add(new DialogMessage(name, message["textInput"].ToString(), message["date"].ToString(), attachesText));
            }

            return result;
        }

        public static List<Dialog> parseDialogs(string html)
        {
            List<Dialog> result = new List<Dialog>();
            CQ dom = html;
            CQ titles = dom[".convo__title"];
            CQ previews = dom[".convo__text"];
            CQ peerContainer = dom[".convo__body"];


            List<IDomObject> titleList = titles.ToList();
            List<IDomObject> previewList = previews.ToList();
            List<string> peers = new List<string>();
            peerContainer.Each((i, e) => {
                string href = e.Attributes.GetAttribute("href");
                string peer = href.Substring(20);
                if (href.Contains("chat"))
                {

                    int temp = Convert.ToInt32(peer);
                    temp += 2000000000;
                    peer = temp.ToString();
                }
                peers.Add(peer);
            });

            for (int i = 0; i < titleList.Count; i++)
            {
                result.Add(new Dialog(peers[i], titleList[i].Cq().Text(), previewList[i].Cq().Text()));
            }
            return result;
        }

    }
}





