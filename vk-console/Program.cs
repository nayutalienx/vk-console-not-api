
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using vk_console.process;


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

                        dialogs = Process.ParseDialogs(ReadDialogHtmlFromJson());
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
                            messages = Process.ReadMessagesFromJson(DataBase.Read("TalkerResponse").ToString());
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
                        messages = Process.ReadMessagesFromJson(DataBase.Read("TalkerResponse").ToString());
                        foreach (DialogMessage v in messages)
                            Console.WriteLine(v);

                        break;
                }
            }

            DataBase.Save();
            Console.WriteLine("Нажмите любую кнопку чтобы выйти.");
            Console.ReadKey();

        }

        public static string ReadDialogHtmlFromJson()
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

        

    }
}





