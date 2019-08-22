using CsQuery;
using Leaf.xNet;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vk_console
{
    class PGAuth
    {
        private const string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.100 Safari/537.36";
        private string Login { get; set; }
        private string Password { get; set; }
        private string Action { get; set; }
        public string Cookie { get; set; }
        public PGAuth(string login, string password)
        {
            Login = login;
            Password = password;
        }
        private HttpResponse GetAuth()
        {
            HttpRequest request = new HttpRequest();
            request.UserAgent = USER_AGENT;
            var respone = request.Get("https://m.vk.com/login");
            return respone;
        }
        public void ParsDataAuth()
        {
            var GetAuths = GetAuth();
            CQ dom = GetAuths.ToString();
            CQ inputs = dom["form"];
            inputs.Each((i, e) =>
            {
                Action = e.Attributes.GetAttribute("action");  
            });

            Cookie = GetAuths.Cookies.GetCookieHeader("https://m.vk.com/login") + "; remixmdevice=1920/1080/1/!!-!!!!";
            DataBase.Write("MainCookies", Cookie);
            //Console.WriteLine(Action);
        }
        public void Auth()
        {
            //  first request via POST (remixq)
            GetRemixq();

            //  second request via GET (remixsid)
            GetRemixsid();

            

        }

        private void GetRemixq()
        {
            //  first request via POST (remixq)
            using (HttpRequest request = new HttpRequest())
            {
                request.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3");
                request.AddHeader("Accept-Encoding", "gzip, deflate");
                request.AddHeader("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
                request.AddHeader("Cache-Control", "max-age=0");
                request.KeepAlive = true;
                //request.AddHeader("Content-Length", "55");
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddHeader("Cookie", Cookie);
                request.AddHeader("Host", "login.vk.com");
                request.AddHeader("Origin", "https://m.vk.com");
                request.AddHeader("Referer", "https://m.vk.com/login");
                request.AddHeader("Sec-Fetch-Mode", "navigate");
                request.AddHeader("Sec-Fetch-Site", "same-site");
                request.AddHeader("Sec-Fetch-User", "?1");
                request.AddHeader("Upgrade-Insecure-Requests", "1");
                request.UserAgent = USER_AGENT;
                RequestParams Params = new RequestParams();
                Params["email"] = Login;
                Params["pass"] = Password;
                request.AllowAutoRedirect = false;
                string response = request.Post(new Uri(Action), Params).ToString();
                
                var cookiesCollection = request.Cookies.GetCookieHeader("https://m.vk.com/login");
                DataBase.Write("remixq", cookiesCollection);
                DataBase.Write("redirect", request.Response.RedirectAddress.ToString());
            }
        }

        private void GetRemixsid() {
            //  second request via GET (remixsid)
            using (HttpRequest request = new HttpRequest())
            {
                request.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3");
                request.AddHeader("Accept-Encoding", "gzip, deflate");
                request.AddHeader("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
                request.AddHeader("Cache-Control", "max-age=0");
                request.KeepAlive = true;
                request.AddHeader("Cookie", Cookie + "; remixff=10; " + DataBase.Read("remixq"));
                request.AddHeader("Host", "m.vk.com");
                request.AddHeader("Referer", "https://m.vk.com/login");
                request.AddHeader("Sec-Fetch-Mode", "same-origin");
                request.AddHeader("Sec-Fetch-Site", "same-origin");
                request.AddHeader("Upgrade-Insecure-Requests", "1");
                request.UserAgent = USER_AGENT;
                request.AllowAutoRedirect = false;
                var resp = request.Get(new Uri(DataBase.Read("redirect").ToString()));
                var cookiesCollection = request.Cookies.GetCookieHeader("https://m.vk.com/login");
                DataBase.Write("remixsid", cookiesCollection);
            }
        }

        private void GetFeedHtml() {
            //  third request via GET (get feed html)
            using (HttpRequest request = new HttpRequest())
            {
                request.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3");
                request.AddHeader("Accept-Encoding", "gzip, deflate");
                request.AddHeader("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
                request.AddHeader("Cache-Control", "max-age=0");
                request.KeepAlive = true;
                request.AddHeader("Cookie", Cookie + "; remixff=10; " + DataBase.Read("remixsid"));
                request.AddHeader("Host", "m.vk.com");
                request.AddHeader("Referer", "https://m.vk.com/login");
                request.AddHeader("Sec-Fetch-Mode", "same-origin");
                request.AddHeader("Sec-Fetch-Site", "same-origin");
                request.AddHeader("Upgrade-Insecure-Requests", "1");
                request.UserAgent = USER_AGENT;
                request.AllowAutoRedirect = false;
                var resp = request.Get(new Uri("https://m.vk.com/feed"));
                DataBase.Write("HTML", resp.ToString());
            }
        }

        public void GetDialogs() {
            //  GET/POST request to get dialogs (and remixmdv cookie)
            using (HttpRequest request = new HttpRequest())
            {
                request.AddHeader("Accept", "*/*");
                request.AddHeader("Accept-Encoding", "gzip, deflate");
                request.AddHeader("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
                request.AddHeader("Cache-Control", "max-age=0");
                request.KeepAlive = true;
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddHeader("Cookie", DataBase.Read("MainCookies") + "; remixff=10; " + DataBase.Read("remixsid"));
                request.AddHeader("Host", "m.vk.com");
                request.AddHeader("Origin", "https://m.vk.com");
                request.AddHeader("Referer", "https://m.vk.com/feed");
                request.AddHeader("Sec-Fetch-Mode", "cors");
                request.AddHeader("Sec-Fetch-Site", "same-origin");
                request.UserAgent = USER_AGENT;
                request.AddHeader("X-Ajax-Nav", "true");
                request.AddHeader("X-Requested-With", "XMLHttpRequest");
                RequestParams Params = new RequestParams();
                Params["_ref"] = "feed";

                request.AllowAutoRedirect = false;
                var resp = request.Post(new Uri("https://m.vk.com/mail"), Params).ToString();
                DataBase.Write("DialogResponse", resp.ToString());


                var cookiesCollection = request.Cookies.GetCookieHeader("https://m.vk.com/");
                DataBase.Write("remixmdv", cookiesCollection);
            }
        }

        public void GetTalker(string peer) {
            //  POST request to get messages with current talker
            using (HttpRequest request = new HttpRequest())
            {
                request.AddHeader("Accept", "*/*");
                request.AddHeader("Accept-Encoding", "gzip, deflate");
                request.AddHeader("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
                request.KeepAlive = true;
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddHeader("Cookie", DataBase.Read("MainCookies") + "; remixff=10; " + DataBase.Read("remixsid") +"; " + DataBase.Read("remixmdv"));
                request.AddHeader("Host", "m.vk.com");
                request.AddHeader("Origin", "https://m.vk.com");
                request.AddHeader("Referer", "https://m.vk.com/mail");
                request.AddHeader("Sec-Fetch-Mode", "cors");
                request.AddHeader("Sec-Fetch-Site", "same-origin");
                request.UserAgent = USER_AGENT;
                request.AddHeader("X-Requested-With", "XMLHttpRequest");

                request.AllowAutoRedirect = false;

                RequestParams Params = new RequestParams();
                Params["act"] = "show";
                Params["peer_id"] = peer;
                Params["direction"] = "before";
                Params["_ajax"] = "1";


                var resp = request.Post(new Uri("https://m.vk.com/mail"), Params);
                DataBase.Write("TalkerResponse", resp.ToString());

                var cookiesCollection = request.Cookies.GetCookieHeader("https://m.vk.com/");
                DataBase.Write("remixmdv", cookiesCollection);
            }
        }

        public void SendMessage(string text, string peer) {
            // POST sending message
            using (HttpRequest request = new HttpRequest())
            {
                request.AddHeader("Accept", "*/*");
                request.AddHeader("Accept-Encoding", "gzip, deflate");
                request.AddHeader("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
                request.KeepAlive = true;
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddHeader("Cookie", DataBase.Read("MainCookies") + "; remixff=10; " + DataBase.Read("remixsid") + "; " + DataBase.Read("remixmdv"));
                request.AddHeader("Host", "m.vk.com");
                request.AddHeader("Origin", "https://m.vk.com");
                request.AddHeader("Referer", "https://m.vk.com/mail?act=show&peer="+peer);
                request.AddHeader("Sec-Fetch-Mode", "cors");
                request.AddHeader("Sec-Fetch-Site", "same-origin");
                request.UserAgent = USER_AGENT;
                request.AddHeader("X-Requested-With", "XMLHttpRequest");

                request.AllowAutoRedirect = false;

                RequestParams Params = new RequestParams();
                Params["message"] = text;
                Params["_ajax"] = "1";
                


                var resp = request.Post(new Uri("https://m.vk.com/mail?act=send&to="+peer+"&from=dialog&hash="+DataBase.Read("hashSend")+"&_af="+DataBase.Read("_af")), Params);
                DataBase.Write("SendResponse", resp.ToString());

            }
        }
    }
}
