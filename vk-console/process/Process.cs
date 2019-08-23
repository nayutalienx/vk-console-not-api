﻿using CsQuery;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace vk_console.process
{
    class Process
    {
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

        public static List<IDialog> ReadMessagesFromJson(string json)
        {

            List<IDialog> result = new List<IDialog>();
            dynamic stuff = Newtonsoft.Json.Linq.JObject.Parse(json);

            string userId = stuff.data[0].cur.userId;
            string peerId = stuff.data[0].cur.peerId;

            //  to send
            Program.currentPeer = peerId;
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

        public static List<IDialog> ParseDialogs(string html)
        {
            List<IDialog> result = new List<IDialog>();
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
