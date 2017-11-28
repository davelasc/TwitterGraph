using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterGraph.TwitterElements {
    class Tweet {

        /*
         * created_at,id,text,truncated,source,in_reply_to_status_id,
         * in_reply_to_user_id,in_reply_to_screen_name,geo,coordinates,
         * place,contributors,is_quote_status,retweet_count,
         * favorite_count,favorited,retweeted,possibly_sensitive,lang
         * */

        #region Global Variables
        public string text { get; set; }
        public string id { get; set; }
        public string date { get; set; }
        public int radius { get; set; }
        public string createdBy { get; set; }
        public string[] mentions { get; set; }
        public int cluster { get; set; }
        public bool process { get; set; }
        #endregion

        #region Constructors
        public Tweet(string id, string text, string date) {
            this.id = id;
            this.text = text;
            this.date = date;
            this.radius = 10;
            process = false;
        }
        #endregion
        
        public void print() {
            Console.WriteLine(string.Format("ID: {0}, TEXT: {1}, DATE: {2}", this.id, this.text, this.date));
            Console.WriteLine("----");
        }
    }
}
