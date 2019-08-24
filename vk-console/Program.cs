
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using vk_console.process;


namespace vk_console
{

    class Program
    {
        
        public static string currentPeer = null;
        static void Main(string[] args)
        {
            List<string> commands = new List<string>() {
            "\n### Команды:",
            "### dialogs [получить список диалогов]",
            "### moreDialogs [получить более старые диалоги]",
            "### Имя Фамилия [получить сообщения диалога]",
            "### more [получить более старые сообщения диалога]",
            "### ~текст сообщения [отправить сообщение в последний посещенный диалог]",
            "### doc Название документа.тип [загрузить документ в папку docs]",
            "### reset [выйти из аккаунта]",
            "### exit [выйти и СОХРАНИТЬ ВСЕ ДАННЫЕ, иначе придется вводить все заново]"
            };

            DataBase.Load();
            //  console.settings
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.SetWindowSize(120,33);
            
            if (DataBase.Read("loginFlag") == null || DataBase.Read("loginFlag").Equals("false"))
            {
                Console.WriteLine("Введите логин:");
                DataBase.Write("login", Console.ReadLine());
                Console.WriteLine("Введите пароль:");
                Console.ForegroundColor = Console.BackgroundColor;
                DataBase.Write("password", Console.ReadLine());
                DataBase.Write("loginFlag", "true");
                Console.ForegroundColor = ConsoleColor.Cyan;
            }

            string login = DataBase.Read("login").ToString();
            string password = DataBase.Read("password").ToString();
            ConnectVk connection = new ConnectVk(login, password);
            bool uiFlag = true;
            List<IDialog> dialogs = null;
            int offset = 20;

            if (DataBase.Read("authFlag") == null || DataBase.Read("authFlag").Equals("false"))
            {
                connection.ParsDataAuth();
                try
                {
                    connection.Auth();
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Ошибка авторизации. Нажмите любую кнопку чтобы закрыть программу и ввести данные снова.");
                    DataBase.Write("loginFlag", "false");
                    DataBase.Write("authFlag", "false");
                    DataBase.Save();
                    Console.ReadKey();
                    return;
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Авторизация завершена.");
                DataBase.Write("authFlag", "true");
            }


            List<IDialog> messages = null;
            while (uiFlag)
            {
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.Gray;
                foreach(string v in commands)
                    Console.WriteLine(String.Format("{0}", v));
                Console.ResetColor();
                
                Console.BufferWidth++;
                Console.BufferWidth--;

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\nВведите команду:");
                Console.SetIn(new StreamReader(Console.OpenStandardInput(),
                               Console.InputEncoding,
                               false,
                               bufferSize: 1024));
                string command = Console.ReadLine();
                switch (command)
                {

                    case "dialogs":
                        try
                        {
                            connection.GetDialogs();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Перезапустите программу чтобы обновить данные.");
                            DataBase.Write("loginFlag", "false");
                            DataBase.Write("authFlag", "false");
                            uiFlag = false;
                            break;
                        }

                        dialogs = Process.ParseDialogs(Process.ReadDialogHtmlFromJson());
                        offset = 20;
                        PrintDialogData(dialogs);
                        break;
                    case "moreDialogs":
                        connection.GetMoreDialogs(offset.ToString());

                        dialogs = Process.ParseMoreDialogsFromJson();
                        offset += 20;
                        PrintDialogData(dialogs);
                        break;
                    case "more":
                        if (DataBase.Read("OuterMessageId") != null) {
                            connection.GetMoreTalker(currentPeer);
                            messages = Process.ReadMessagesFromJson(DataBase.Read("TalkerResponse").ToString());
                            PrintDialogData(messages);
                        }
                        break;
                    case "reset":
                        DataBase.Write("login", "");
                        DataBase.Write("password", "");
                        DataBase.Write("loginFlag", "false");
                        DataBase.Write("authFlag", "false");
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Данные стерты. Перезапустите программу чтобы войти заново.");
                        uiFlag = false;
                        break;
                    case "exit":
                        uiFlag = false;
                        break;
                    default:
                        if (dialogs == null)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("В начале нужно получить список диалогов.");
                            break;
                        }

                        if (command.Substring(0, 3).Equals("doc")) {
                            if (messages == null) {
                                Console.WriteLine("В начале нужно получить список сообщений.");
                                break;
                            }
                            string fileName = command.Substring(4).Trim();
                            string fileUrl = null;
                            foreach (DialogMessage v in messages) {
                                foreach (KeyValuePair<string, string> kv in v.Docs) {
                                    if (kv.Value.Trim().Equals(fileName)) {
                                        fileUrl = kv.Key;
                                    }
                                }
                            }
                            if (fileUrl == null)
                            {
                                Console.WriteLine("Данного файла не существует.");
                                break;
                            }
                            else {
                                Console.WriteLine("Начинаю загрузку файла.\n"+fileUrl);
                                string location = connection.GetDocumentLocation(fileUrl, currentPeer);
                                connection.DownloadDocument(location, "docs\\"+fileName, currentPeer);
                                Console.Clear();
                                PrintDialogData(messages);
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine("Файл загружен в папку docs.");
                            }

                            break;
                        }

                        if (command[0] == '~')
                        {
                            string message = command.Substring(1);
                            if (currentPeer == null)
                            {
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine("Зайдите в нужный диалог и повторите команду.");
                                break;
                            }
                            connection.SendMessage(message, currentPeer);
                            connection.GetTalker(currentPeer);
                            messages = Process.ReadMessagesFromJson(DataBase.Read("TalkerResponse").ToString());
                            
                            PrintDialogData(messages);
                            break;
                        }

                        

                        foreach (Dialog v in dialogs)
                        {
                            if (v.Talker.Trim() == command.Trim())
                            {
                                connection.GetTalker(v.Peer.Trim());
                                break;
                            }
                        }
                        messages = Process.ReadMessagesFromJson(DataBase.Read("TalkerResponse").ToString());
                        
                        PrintDialogData(messages);

                        break;
                }
            }

            DataBase.Save();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Нажмите любую кнопку чтобы выйти.");
            Console.ReadKey();

        }

        
        public static void PrintDialogData(List<IDialog> dialogs)
        {
            Console.Clear();
            int counter = 1;
            Console.ForegroundColor = ConsoleColor.White;
            foreach (IDialog v in dialogs)
            {
                if (counter % 2 == 0)
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                else
                    Console.BackgroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine(String.Format("{0}", v));
                counter++;
            }
            Console.ResetColor();
            
        }


    }
}





