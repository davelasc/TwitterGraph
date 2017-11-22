using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net;
using System.IO;

namespace TwitterAPI {

    class TwitterAPI {

        private string TWITTER_ACCESS_TOKEN = "";
        private string ACCOUNT_CREDENTIALS = "";
        
        #region Constructor related operations

        public TwitterAPI() {
            ACCOUNT_CREDENTIALS = CreateCredentials();
            TWITTER_ACCESS_TOKEN = CreateTwitterAccessToken();
        }

        private string CreateCredentials() {
            string credential = "";
            try {
                string accountEmail = ConfigurationManager.AppSettings["ACCOUNT_EMAIL"];
                string accountPassword = ConfigurationManager.AppSettings["ACCOUNT_PASSWORD"];
                string authentification = string.Format("{0}:{1}", accountEmail, accountPassword);
                string encapsulate = Convert.ToBase64String(Encoding.ASCII.GetBytes(authentification));
                credential = string.Format("{0} {1}", "Basic", encapsulate);
            }catch {
                Console.WriteLine("Error while creating the account's credentials");
            }
            return credential;
        }

        private string CreateTwitterAccessToken() {
            string access_token = "";
            var post = WebRequest.Create("https://api.twitter.com/oauth2/token") as HttpWebRequest;
            post.Method = "POST";
            post.ContentType = "application/x-www-form-urlencoded";
            post.Headers[HttpRequestHeader.Authorization] = ACCOUNT_CREDENTIALS;
            var reqbody = Encoding.UTF8.GetBytes("grant_type=client_credentials");
            post.ContentLength = reqbody.Length;
            using (var req = post.GetRequestStream()) {
                req.Write(reqbody, 0, reqbody.Length);
            }
            try {
                string respbody = null;
                using (var resp = post.GetResponse().GetResponseStream())//there request sends
                {
                    var respR = new StreamReader(resp);
                    respbody = respR.ReadToEnd();
                }
                access_token = respbody.Substring(respbody.IndexOf("access_token\":\"") + "access_token\":\"".Length, respbody.IndexOf("\"}") - (respbody.IndexOf("access_token\":\"") + "access_token\":\"".Length));
            } catch
              {
                Console.WriteLine("Error while creating the account's access token");
            }
            return access_token;
        }

        #endregion

        

        public void doSearchRun() {
            var gettimeline = WebRequest.Create("https://api.twitter.com/1.1/statuses/user_timeline.json?count=3&screen_name=twitterapi") as HttpWebRequest;
            gettimeline.Method = "GET";
            gettimeline.Headers[HttpRequestHeader.Authorization] = "Bearer " + this.TWITTER_ACCESS_TOKEN;
            try {
                string respbody = null;
                using (var resp = gettimeline.GetResponse().GetResponseStream())//there request sends
                {
                    var respR = new StreamReader(resp);
                    respbody = respR.ReadToEnd();
                }

                //TODO use a library to parse json
                Console.WriteLine(respbody);
            } catch //401 (access token invalid or expired)
              {
                //TODO
            }
        }

    }
}
