using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitterGraph.TwitterElements;

namespace TwitterGraph.DatabaseHandler {
    class Neo4jDB {

        #region Global Variables

        private string SERVER_INFO = "";
        private GraphClient graphClient = null;
        
        #endregion
        
        #region Constructors
        public Neo4jDB() {
            SERVER_INFO = ConfigurationManager.AppSettings["SERVER_INFO"];
        }

        public Neo4jDB(string update) {

        }
        #endregion

        public bool startServerConnection() {
            if (SERVER_INFO != "") {
                try {
                    graphClient = new GraphClient(new Uri(SERVER_INFO));
                    graphClient.Connect();
                    return true;
                } catch {
                    Console.WriteLine("Error a la hora de conectarme al servidor");
                }
            }
            return false;
        }

        public bool insertTweetNodes(Dictionary<string, Tweet> tweetUniverse) {

            foreach(string id in tweetUniverse.Keys) {
                graphClient.Cypher
                    .Create("(tweet:Tweet {newTweet})")
                    .WithParam("newTweet", tweetUniverse[id])
                    .ExecuteWithoutResults();
            }

            return true;
        }

        public bool insertUserNodes(Dictionary<string, User> userUniverse) {
            foreach (string id in userUniverse.Keys) {
                graphClient.Cypher
                    .Create("(user:User {newUser})")
                    .WithParam("newUser", userUniverse[id])
                    .ExecuteWithoutResults();
            }
            return true;
        }

        public bool insertTweetToCreatorRelationship(Dictionary<string, Tweet> tweetUniverse) {
            
            foreach(string tweetID in tweetUniverse.Keys) {
                string userID = tweetUniverse[tweetID].createdBy;
                graphClient.Cypher
                    .Match("(tweet:Tweet)", "(user:User)")
                    .Where((Tweet tweet) => tweet.id == tweetID)
                    .AndWhere((User user) => user.id == userID)
                    .CreateUnique("(tweet)<-[:CREATED_BY]-(user)")
                    .ExecuteWithoutResults();
            }

            return true;
        }

        public bool insertTweetToMentionedRelationship(Dictionary<string, Tweet> tweetUniverse) {

            foreach(string tweetID in tweetUniverse.Keys) {
                string[] usersID = tweetUniverse[tweetID].mentions;
                if(usersID != null) 
                    foreach (string userID in usersID) {
                        graphClient.Cypher
                            .Match("(tweet:Tweet)", "(user:User)")
                            .Where((Tweet tweet) => tweet.id == tweetID)
                            .AndWhere((User user) => user.id == userID)
                            .CreateUnique("(tweet)-[:MENTIONS]->(user)")
                            .ExecuteWithoutResults();
                    }
            }

            return true;
        }
        
        public void returnNode(string nodeName) {
            /*var node = graphClient.Cypher.Match("(person:Person)")
                .Where((Person person) => person.name == "Tom Hanks")
                .Return(person => person.As<Person>())
                .Results
                .Single();
            Console.WriteLine(string.Format("Resultado fue: {0} {1}", node.name, node.born));/**/
        }

    }
}
