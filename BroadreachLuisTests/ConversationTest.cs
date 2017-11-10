using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Connector.DirectLine;
using System.Threading.Tasks;
using System.Web;
using System.Net.Configuration;
using System.Configuration;

namespace BroadreachLuisTests
{
    /// <summary>
    /// Summary description for ConversationTest
    /// </summary>
    [TestClass]
    public class ConversationTest
    {
        public ConversationTest()
        {
            //
            // TODO: Add constructor logic here
            //
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
        public void TestConversation()
        {
            // Bind to the conversation endpoint
            // Start the conversation
            DirectLineClient client = new DirectLineClient(ConfigurationManager.AppSettings["BotSecret"]);
            var conversation = client.Conversations.StartConversation();
            Activity userMessage = new Activity
            {
                From = new ChannelAccount("User1"),
                Text = "Hello",
                Type = ActivityTypes.Message
            };

            client.Conversations.PostActivity(conversation.ConversationId, userMessage);
            var watermark = "";
            var response = client.Conversations.GetActivities(conversation.ConversationId, watermark);
            watermark = response.Watermark;
        }
        
    }
}
