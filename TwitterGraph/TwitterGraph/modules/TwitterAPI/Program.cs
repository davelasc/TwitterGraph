using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterAPI {
    class Program {
        static void Main(string[] args) {

            TwitterAPI twitterAPI = new TwitterAPI();
            twitterAPI.doSearchRun();
            Console.ReadKey();
        }
    }
}
