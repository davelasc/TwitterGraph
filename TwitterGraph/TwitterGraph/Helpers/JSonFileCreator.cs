using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitterGraph.TwitterElements;

namespace TwitterGraph.Helpers {
    class JSonFileCreator {

        #region Global Variables

        private Dictionary<string, Tweet> tweetUniverse = null;
        private Dictionary<string, User> userUniverse = null;

        private FileStream fs = null;

        #endregion

        #region Constructors

        public JSonFileCreator(Dictionary<string, Tweet> tweetUniverse, Dictionary<string, User> userUniverse) {

            this.tweetUniverse = tweetUniverse;
            this.userUniverse = userUniverse;

        }

        #endregion

        public bool createJSonFile(string commentContent) {

            try {
                Console.WriteLine("Creating the twitter.json file");
                string path = ConfigurationManager.AppSettings["DATA_PATH"].ToString();

                if(File.Exists(path)) {
                    File.Delete(path);
                }

                fs = File.Create(path);

                Console.WriteLine("Setting clusters");
                if (!setCluster())
                    return false;

                Console.WriteLine("Adding the search criteria in the comment section");
                if (!this.writeToFile("{\"comment\":\"" + commentContent + "\""))
                    return false;
                
                if (!this.writeToFile(", \"nodes\": ["))
                    return false;

                Console.WriteLine("Adding the tweet nodes");
                if (!addTweetNode())
                    return false;

                Console.WriteLine("Adding the user nodes");
                if (!addUserNode())
                    return false;

                if (!this.writeToFile("],"))
                    return false;
                
                Console.WriteLine("Creating user -> created -> tweet");

                if (!this.writeToFile("\"edges\": ["))
                    return false;

                if (!createTweetCreatedByConnection())
                    return false;

                Console.WriteLine("Creating tweet -> mentions -> user");

                if (!createUsersMentioned())
                    return false;


                if (!this.writeToFile("]"))
                    return false;

                if (!this.writeToFile("}"))
                    return false;

                return true;

            } catch(Exception e) {
                Console.WriteLine(e.Message);
                return false;
            }
            
        }

        private bool addTweetNode() {
            try {
                for(int i = 0; i < tweetUniverse.Count(); i++) {
                    Tweet tweet = tweetUniverse.Values.ElementAt(i);
                    string cleanText;
                    using (var writer = new StringWriter()) {
                        using (var provider = CodeDomProvider.CreateProvider("CSharp")) {
                            provider.GenerateCodeFromExpression(new CodePrimitiveExpression(tweet.text), writer, null);
                            cleanText = writer.ToString();
                        }
                    }
                    cleanText = cleanText.Replace(System.Environment.NewLine, "");
                    cleanText = cleanText.Replace("\" +    \"", "");
                    cleanText = cleanText.Replace("\\'", "'");

                    string tweetNode = "{\"id\": \"" + tweet.id + "\"," +
                        "\"caption\": " + cleanText + "," +
                        "\"date\": \"" + tweet.date + "\" , " +
                        "\"cluster\": "+ tweet.cluster+", " +
                        "\"radius\" : " + tweet.radius + "},";

                    if (!this.writeToFile(tweetNode))
                        return false;
                }

                return true;
            }catch(Exception e) {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        private bool addUserNode() {
            try {
                for (int i = 0; i < userUniverse.Count(); i++) {
                    User user = userUniverse.Values.ElementAt(i);

                    string userNode = "{\"id\": \"" + user.id + "\"," +
                        "\"caption\": \"" + user.name + "\"," +
                        "\"cluster\": "+user.cluster+", " +
                        "\"radius\" : "+user.radius+"}";
                    if (i >= 0 && i < userUniverse.Count()-1) {
                        userNode += ",";
                    }

                    if (!this.writeToFile(userNode))
                        return false;
                }

                return true;
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        private bool setCluster() {
            try {
                //Setting the stage
                List<string> pendingToProcess = new List<string>();
                int cluster = 0;
                foreach(string tweetID in tweetUniverse.Keys) {
                    if (tweetUniverse[tweetID].process)
                        continue;

                    pendingToProcess.Add(tweetID);
                    cluster++;

                    while(pendingToProcess.Count() > 0) {
                        string currentID = pendingToProcess[0];
                        pendingToProcess.RemoveAt(0);

                        if (tweetUniverse[currentID].process)
                            continue;

                        tweetUniverse[currentID].cluster = cluster;
                        tweetUniverse[currentID].process = true;

                        string creatorID = tweetUniverse[currentID].createdBy;
                        userUniverse[creatorID].cluster = cluster;
                        List<string> tempList = userUniverse[creatorID].mentionedIn;
                        if(tempList.Count() > 0)
                            pendingToProcess.AddRange(tempList);

                        string[] usersMentioned = tweetUniverse[currentID].mentions;
                        if(usersMentioned != null) {
                            foreach (string userMentionedID in usersMentioned) {
                                userUniverse[userMentionedID].cluster = cluster;
                                tempList = userUniverse[userMentionedID].created;
                                if (tempList.Count() > 0)
                                    pendingToProcess.AddRange(tempList);
                                tempList = userUniverse[userMentionedID].mentionedIn;
                                if (tempList.Count() > 0)
                                    pendingToProcess.AddRange(tempList);
                            }
                        }
                    }
                }
                createHexTable(cluster);
                 return true;
            }catch(Exception e) {
                Console.WriteLine(e.Message);
                return false;
            }
            
        }

        private bool createTweetCreatedByConnection() {
            try {
                foreach(string tweetID in this.tweetUniverse.Keys) {
                    string userID = tweetUniverse[tweetID].createdBy;
                    string connection = "" +
                        "{\"source\": \"" + userID + "\"," +
                        "\"target\": \"" + tweetID + "\"," +
                        "\"caption\": \"CREATED\"},";

                    if (!this.writeToFile(connection))
                        return false;

                }
                return true;
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        private bool createUsersMentioned() {
            try {
                for (int i = 0; i < tweetUniverse.Count(); i++) {
                    string tweetID = tweetUniverse.Keys.ElementAt(i);
                    Tweet currentTweet = tweetUniverse[tweetID];
                    string connection = "";
                    string[] usersID = currentTweet.mentions;
                    if (usersID != null) {
                        for (int j = 0; j < usersID.Count(); j++) {
                            connection +=
                            "{\"source\": \"" + tweetID + "\"," +
                            "\"target\": \"" + currentTweet.mentions[j] + "\"," +
                            "\"caption\": \"MENTIONS\"}";
                            if (j >= 0 && j < usersID.Count() - 1) {
                                connection += ",";
                            }
                        }
                        if (i >= 0 && i < tweetUniverse.Count() - 1) {
                            connection += ",";
                        }

                        if (!this.writeToFile(connection))
                            return false;
                    }
                }

                return true;
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        private bool writeToFile(string value) {
            try {
                Byte[] info = new UTF8Encoding(true).GetBytes(value);
                fs.Write(info, 0, info.Length);
                return true;
            } catch(Exception e) {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        private void createHexTable(int max) {

            int hex = 0xFFFFFF;
            for(int i = 0; i < max; i++) {
                hex -= 0x0010A0;
                Console.Write("\"#{0:X}\",",hex);
            }
            Console.WriteLine();
        }

    }
}
