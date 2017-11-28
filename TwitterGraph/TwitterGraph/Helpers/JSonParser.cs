using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitterGraph.TwitterElements;

namespace TwitterGraph.Helpers {
    class JSonParser {

        private JArray JSON_ARRAY = null;
        private int currentChild = 0;
        private JToken STATUSES = null, SEARCH_META_DATA = null;


        public JSonParser(JArray jArray) {
            this.JSON_ARRAY = jArray;
        }

        public JSonParser(JToken jToken) {
            this.STATUSES = jToken;
        }

        public JSonParser(string jSonString) {
            JToken jToken = JToken.Parse(jSonString);
            try {
                STATUSES = jToken.Children().ElementAt(0).ElementAt(0);
                SEARCH_META_DATA = jToken.Children().ElementAt(1).ElementAt(0);
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                STATUSES = null;
                SEARCH_META_DATA = null;
            }
            
        }

        public bool wasSearchSuccessfull() {
            return STATUSES.First != null && STATUSES.First.Count() > 0 ? true : false;
        }
        
        public void printFullResult() {
            Console.WriteLine(STATUSES);
        }

        public void printMetaData() {
            Console.WriteLine(SEARCH_META_DATA);
        }

        public int attributeCount() {
            return this.STATUSES.Children().Count();
        }

        public string getAttributeValue(string attributeName) {
            try {
                return STATUSES.SelectToken(attributeName).ToString();
            } catch {
                return "";
            }
        }

        public JToken getNextChild() {
            if (currentChild >= STATUSES.Children().Count())
                return null;

            JToken child = STATUSES.ElementAt(currentChild);
            currentChild++;
            return child;
        }
    }
}
