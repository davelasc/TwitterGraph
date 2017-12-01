using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterGraph.TwitterElements {
    
    class User {

        #region Global Variables
        
        public string name { get; set; }
        public string id { get; set; }
        public int timesMentioned { get; set; }
        public string followerCount { get; set; }
        public string friendCount { get; set; }
        public int radius { get; set; }
        public int cluster { get; set; }
        public List<string> mentionedIn { get; set; }
        public List<string> created { get; set; }
        public bool process { get; set; }

        #endregion

        #region Constructor 

        public User(string id, string name) {
            startGeneric(id, name);
            this.followerCount = "NA";
            this.friendCount = "NA";

        }

        public User(string id, string name, string followerCount, string friendCount) {
            startGeneric(id, name);
            this.followerCount = followerCount;
            this.friendCount = friendCount;
        }

        private void startGeneric(string id, string name) {
            this.id = id;
            this.name = name;
            this.radius = 15;
            this.process = false;
            mentionedIn = new List<string>();
            created = new List<string>();
        }
        
        #endregion

        public void print() {
            Console.WriteLine(string.Format("ID: {0}, NAME: {1}, FOLLOWER_COUNT: {2}, FRIEND_COUNT: {3}", this.id, this.name, this.followerCount, this.friendCount));
            Console.WriteLine("----");
        }

    }
}
