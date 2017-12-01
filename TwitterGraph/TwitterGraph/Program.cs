using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitterGraph.DatabaseHandler;
using TwitterGraph.Helpers;
using TwitterGraph.TwitterElements;

namespace TwitterGraph {
    class Program {
        static void Main(string[] args) {
            TestTwitterConnection();
            //TestServer();
            //TestWrittingFile();
            Console.ReadKey();
        }

        private static void TestWrittingFile() {
            JSonFileCreator jsfc = new JSonFileCreator(null, null);
            jsfc.createJSonFile("Hola mundo");
        }

        private static void TestServer() {
            Neo4jDB neoDB = new Neo4jDB();
            if(neoDB.startServerConnection()) {
                Console.WriteLine("Todo bien hasta el momento");
            } else {
                Console.WriteLine("Algo salio mal");
            }
        }

        private static void TestTwitterConnection() {
            //twitterAPI.searchFor("dlmnbvcfgyuimnbvfgyuinvcfknbvfasdiagsdiuagf91gr97g", 10);
            //twitterAPI.searchFor("twitterapi", 1);
            for(int i = 5; i <= 200000; i *=2) {
                TwitterAPI twitterAPI = new TwitterAPI();
                twitterAPI.searchFor("marvel", i);
                Console.ReadKey();
            }
                

        }
    }
}
