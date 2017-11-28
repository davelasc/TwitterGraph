using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TwitterGraph.DatabaseHandler;
using TwitterGraph.Helpers;

namespace TwitterGraph.TwitterElements {
    class TwitterAPI {

        #region Global Variables
        private string TWITTER_ACCESS_TOKEN = "";
        private string ACCOUNT_CREDENTIALS = "";

        private Dictionary<string, Tweet> tweetUniverse;
        private Dictionary<string, User> userUniverse;

        private JSonParser parser;
        #endregion

        #region Constructor related operations

        public TwitterAPI() {
            ACCOUNT_CREDENTIALS = CreateCredentials();
            TWITTER_ACCESS_TOKEN = CreateTwitterAccessToken();

            tweetUniverse = new Dictionary<string, Tweet>();
            userUniverse = new Dictionary<string, User>();
        }

        private string CreateCredentials() {
            string credential = "";
            try {
                string accountEmail = ConfigurationManager.AppSettings["ACCOUNT_EMAIL"];
                string accountPassword = ConfigurationManager.AppSettings["ACCOUNT_PASSWORD"];
                string authentification = string.Format("{0}:{1}", accountEmail, accountPassword);
                string encapsulate = Convert.ToBase64String(Encoding.ASCII.GetBytes(authentification));
                credential = string.Format("{0} {1}", "Basic", encapsulate);
            } catch {
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
            } catch {
                Console.WriteLine("Error while creating the account's access token");
            }
            return access_token;
        }

        #endregion

        public bool searchFor(string search, int maxCount) {
            int MAX_COUNTER = 100;

            for(int i = maxCount; i >= 0; i-=MAX_COUNTER) {
                Console.WriteLine(string.Format("i: {0}, maxCount = {1}", i, maxCount));
                string request = "https://api.twitter.com/1.1/search/tweets.json?q=";
                int count = i > 100 ? MAX_COUNTER : i;
                if (i == maxCount) {
                    request += search + "&count=" + count;
                } else {
                    request += search + "&count=" + count + "&max_id=" + tweetUniverse.Last().Value.id;
                }

                var searchRequest = WebRequest.Create(request) as HttpWebRequest;
                searchRequest.Method = WebRequestMethods.Http.Get;//"GET";
                searchRequest.ContentType = "application/json; charset=utf-8";
                searchRequest.Accept = "application/json";
                searchRequest.Headers[HttpRequestHeader.Authorization] = "Bearer " + this.TWITTER_ACCESS_TOKEN;
                Console.WriteLine("Haciendo el request a Twitter...");
                try {
                    string respbody = null;
                    using (var resp = searchRequest.GetResponse().GetResponseStream())//there request sends
                    {
                        var respR = new StreamReader(resp);
                        respbody = respR.ReadToEnd();
                    }
                    parser = new JSonParser(respbody);

                    if (!parser.wasSearchSuccessfull())
                        return false;
                    Console.WriteLine("Conseguí los nuevos elementos....");
                    if (!insertTwitterDataIntoDatabase())
                        return false;
                    //parser.printFullResult();
                    //parser.printMetaData();
                    

                } catch (Exception e) //401 (access token invalid or expired)
                  {
                    Console.WriteLine(e.Message);
                    return false;
                }

            }
            
            /*Console.WriteLine("Agregando nuevo elementos a la base de datos...");
            if (!dumpTweetsToDB())
                return false;*/

            Console.WriteLine("Creando el archivo de twitter.json");
            if (!createTwitterJsonFile(search))
                return false;

            Console.WriteLine("Listo...");
            Console.WriteLine("------------------------------------------------------------------");
            return true;
        }

        private bool insertTwitterDataIntoDatabase() {
            
            JToken tweet = parser.getNextChild();
            do {
                insertTweet(tweet);
                tweet = parser.getNextChild();
            } while (tweet != null);
            
            return true;
        }

        private bool insertTweet(JToken tweet) {
            try {
                string id = tweet.SelectToken("id_str").ToString() + "t";
                string date = tweet.SelectToken("created_at").ToString();
                string text = tweet.SelectToken("text").ToString();
                Tweet myTweet = new Tweet(id, text, date);
                
                if (tweetUniverse.ContainsKey(id))
                    return true;
                
                JToken user = tweet.SelectToken("user");
                User tweetCreator = createUser(user, true, id);
                if (tweetCreator != null) {
                    myTweet.createdBy = tweetCreator.id;
                }

                myTweet.mentions = createTweetMentioningList(myTweet, tweet.SelectToken("entities").SelectToken("user_mentions"));
                tweetUniverse.Add(id, myTweet);

                return true;
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        private User createUser(JToken user, bool full, string tweetID) {
            try {
                string id = user.SelectToken("id_str").ToString() + "u";
                string name = user.SelectToken("screen_name").ToString();
                User myUser = null;

                if(userUniverse.ContainsKey(id)) {
                    myUser = userUniverse[id];
                    myUser.radius += 1;
                    if (full) {
                        string followersCount = user.SelectToken("followers_count").ToString();
                        string friendsCount = user.SelectToken("friends_count").ToString();
                        myUser.followerCount = followersCount;
                        myUser.friendCount = friendsCount;
                        myUser.created.Add(tweetID);
                    } else {
                        myUser.mentionedIn.Add(tweetID);
                    }
                    userUniverse[id] = myUser;
                } else {
                    if (full) {
                        string followersCount = user.SelectToken("followers_count").ToString();
                        string friendsCount = user.SelectToken("friends_count").ToString();
                        myUser = new User(id, name, followersCount, friendsCount);
                        myUser.created.Add(tweetID);
                    } else {
                        myUser = new User(id, name);
                        myUser.mentionedIn.Add(tweetID);
                    }
                    userUniverse.Add(id, myUser);
                }
                
                return myUser;
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        
        private string[] createTweetMentioningList(Tweet tweet, JToken usersMentioned) {

            if (!usersMentioned.HasValues)
                return null;

            int userCount = usersMentioned.Children().Count();
            string[] users = new string[userCount];
            JSonParser myParser = new JSonParser(usersMentioned);
            User user;
            for(int i = 0; i < userCount; i++) {
                user = createUser(myParser.getNextChild(), false, tweet.id);
                if(user != null)
                    users[i] = user.id;
            }
            
            return users;
        }

        private bool dumpTweetsToDB() {
            Neo4jDB db = new Neo4jDB();
            if (!db.startServerConnection())
                return false;

            Console.WriteLine("->Agregando los nodos Tweet");
            db.insertTweetNodes(tweetUniverse);
            Console.WriteLine("->Agregando los nodos User");
            db.insertUserNodes(userUniverse);
            Console.WriteLine("->Agregando las relaciones de creación");
            db.insertTweetToCreatorRelationship(tweetUniverse);
            Console.WriteLine("->Agregando las relaciones de menciondaos");
            db.insertTweetToMentionedRelationship(tweetUniverse);
            return true;
        }

        private bool createTwitterJsonFile(string comment) {
            JSonFileCreator jsfc = new JSonFileCreator(tweetUniverse, userUniverse);
            return jsfc.createJSonFile(comment);
        }

    }
}
