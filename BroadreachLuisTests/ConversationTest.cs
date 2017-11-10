using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Connector.DirectLine;
using System.Threading.Tasks;
using System.Web;
using System.Net.Configuration;
using System.Configuration;
using System.IO;
using System.Web.Script.Serialization;

namespace BroadreachLuisTests
{
    /// <summary>
    /// Summary description for ConversationTest
    /// </summary>
    [TestClass]
    [DeploymentItem("ConversationTest.json ")]
    public class ConversationTest
    {
        #region JSON Structure for Tests

        public class TestJSON
        {
            public string botSecret { get; set; }
            public Test[] tests { get; set; }
        }

        public class Test
        {
            public string name { get; set; }
            public string description { get; set; }
            public ConversationScript[] conversationScript { get; set; }
        }

        public class ConversationScript
        {
            public string query { get; set; }
            public string responseContains { get; set; }
        }

        #endregion

        #region Global Variables for Conversation
        // Globals for the conversation
        private string _watermark;
        private string _userName = "User233";
        private DirectLineClient _client;
        private Conversation _conversation;
        // To make things easier
        private Activity _userMessage;
        #endregion

        private TestJSON _testObject;


        public ConversationTest()
        {
            //
            // TODO: Add constructor logic here
            //
            // Add Logic here
            // Todo: Not sure what will happen here once this is in Azure?
            string fileLocation = System.Environment.CurrentDirectory + "/" + "ConversationTest.json";
            // Load the file
            TextReader tr = File.OpenText(fileLocation);
            string json = tr.ReadToEnd();
            // Load the JSON Object up
            _testObject = new JavaScriptSerializer().Deserialize<TestJSON>(json);

        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestPerformance()
        {
            // Bind to the conversation endpoint
            // Start the conversation
            TestConversationFromJSON("Performance1");
        }

        [TestMethod]
        public void TestBestWorst1()
        {
            // Bind to the conversation endpoint
            // Start the conversation
            TestConversationFromJSON("BestWorst");
        }

        // Get the response for a message
        public string IssueTextAndGetResponse(string textToSay)
        {
            if (_userMessage == null)
            {
                _userMessage = new Activity
                {
                    From = new ChannelAccount(_userName),
                    Text = textToSay,
                    Type = ActivityTypes.Message
                };
            }
            else
                _userMessage.Text = textToSay;
            ActivitySet response = null;
            // Say hello?
            if (_client == null)
            {
                //_client = new DirectLineClient(ConfigurationManager.AppSettings["BotSecret"]);
                _client = new DirectLineClient(_testObject.botSecret);
                _conversation = _client.Conversations.StartConversation();
                // Post to start
                //_userMessage.Text = "Hello";
                //_client.Conversations.PostActivity(_conversation.ConversationId, _userMessage);
                response = _client.Conversations.GetActivities(_conversation.ConversationId, _watermark);
                _watermark = response.Watermark;
                _userMessage.Text = textToSay;
            }

            // Send message
            var postObject = _client.Conversations.PostActivity(_conversation.ConversationId, _userMessage);

            // Wait for answer
            response = _client.Conversations.GetActivities(_conversation.ConversationId, _watermark);
            _watermark = response.Watermark;
            string answer = ""; //Todo: Cards?
            // Find correct response
            foreach (Activity a in response.Activities)
            {
                // Check - the activity will tell us which is the post and which is the reply
                if ((a.ReplyToId != null) && (a.ReplyToId == postObject.Id))
                    answer = a.Text;
            }

            return answer;
        }

        // Method to test a conversation
        private void TestConversationFromJSON(string conversationID)
        {
            foreach (var test in _testObject.tests)
            {
                if (test.name == conversationID)
                {
                    // Run through each query
                    foreach (var interaction in test.conversationScript)
                    {
                        string answer = IssueTextAndGetResponse(interaction.query);
                        // Tests can contain a "contains" string, or others like "length" or something
                        if (interaction.responseContains != null)
                        {
                            Assert.IsTrue(answer.ToLower().Contains(interaction.responseContains.ToLower()));
                        }
                    }
                }
            }
        }
    }


}
