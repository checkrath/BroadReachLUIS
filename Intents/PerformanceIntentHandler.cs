using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using global::AdaptiveCards;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using LuisBot.Data;
using System.Configuration;

namespace LuisBot.Intents
{
    /// <summary>
    /// Will generate a return message for a specific intent
    /// </summary>
    public class PerformanceIntentHandler
    {
        /// <summary>
        /// Generate a card showing indicator performance against programs or districts
        /// </summary>
        /// <param name="context"></param>
        /// <param name="lastProgram"></param>
        /// <returns></returns>
        public async Task<Attachment> ShowPerformanceCard(IDialogContext context,string lastProgram)
        {
            //get the data
            PerformanceDBQuery query = new PerformanceDBQuery();
            List<IndicatorPerformance> indicators = query.GetProgramPerformance(lastProgram);

            //card title
            string listEntity = "program";
            var title = $"{listEntity} performance for {lastProgram}";
            var titleBlock = new TextBlock()
            {
                Text = title,
                Size = TextSize.ExtraLarge,
                Speak = $"<s>{title}</s>"
            };

            //create the list of indicators
            var facts = new FactSet();
            foreach (IndicatorPerformance indicator in indicators )
            {
                //create info string
                MessageFormater messageFormater = new MessageFormater();
                var fact = new AdaptiveCards.Fact(indicator.Indicatorname, messageFormater.IndicatorPerformance(indicator, true));
                facts.Facts.Add(fact);
            }

            //create content block
            var factsBlock = new List<CardElement>()
            {
                new TextBlock()
                {
                    Text="Indicator performance is as follows:",
                    Size=TextSize.Normal
                },
                facts
            };

            //create the image
            var imageBlock = new List<CardElement>()
            {
                new Image()
                    {
                        Size = ImageSize.Large,
                        Url = ConfigurationManager.AppSettings["ImagePath"] + "/images/exampleGraph.jpg"
                    }
            };

            //big layout
            var bodyLayout = new List<CardElement>()
            {          
               titleBlock,
               new Container()
               {Items=factsBlock },
               new Container()
               {Items=imageBlock}
                //new ColumnSet
                //{
                //    Columns=new List<Column>()
                //    {
                //        new Column()
                //        {
                //            Size=ColumnSize.Auto,
                //            Items=factsBlock
                //        },
                //        new Column()
                //        {
                //            Items=imageBlock
                //        }
                //    }
                //}
            };

            var card = new AdaptiveCard()
            {
                Body = bodyLayout
            };

            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };

            //var reply = context.MakeMessage();
            return attachment;
            //(await message).Attachments.Add(attachment);
            //await context.PostAsync((await message));





        }
    }
}