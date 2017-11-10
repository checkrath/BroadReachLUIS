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
        public async Task<Attachment> ShowPerformanceCard(IDialogContext context,string lastProgram, string lastDistrict,string term, PerformanceType performanceType)
        {
            //get the data
            string entityName = (performanceType == PerformanceType.Program) ? lastProgram : lastDistrict;
            PerformanceDBQuery query = new PerformanceDBQuery();
            List<IndicatorPerformance> indicators;
            if (performanceType == PerformanceType.Program)
                indicators = query.GetProgramPerformance(lastProgram);
            else
                indicators= query.GetDistrictPerformance(lastDistrict);

            //card title
            string listEntity = (performanceType==PerformanceType.Program)? "Program":"District";
            var title = $"{listEntity} {term} performance for {entityName}";
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
                var fact = new AdaptiveCards.Fact(indicator.Indicatorname, messageFormater.IndicatorPerformance(indicator, (term == "Annual")));
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

        //public string ShowPerformanceAsText(IDialogContext context, string lastProgram)
        //{
        //    //Build up a string describing performance:
        //    const string TOKEN_DISTRICT = "[district/prog]";
        //    string fullOutput = "The [term] [indicator]performance for [district/prog] is [performance]. [note]";

        //    //dummy output
        //    Random rnd = new Random();
        //    int target = rnd.Next(20, 120);
        //    int actual = rnd.Next(20, 120);
        //    int percentTarget = (int)(Math.Round(((double)actual / (double)target) * 100));

        //    //After [Mx_Past] months you should have spent $[LatestMonth_YTDTarget] [Indicator] but you have spent $[LatestMonth_YTDValue] which is [LatestMonth_AnnualTarget_perc]% of the annual target of $[LatestMonth_AnnualTarget]
        //    //if (this.lastProgramme == "All Programmes")

        //    //determine if it's a single indicator or a list
        //    bool allIndicators = (lastIndicator == "All indicators");

        //    //get the term
        //    fullOutput = fullOutput.Replace("[term]", lastTerm);

        //    //get the indicator
        //    if (lastIndicator == "All indicators")
        //        fullOutput = fullOutput.Replace("[indicator]", "");
        //    else
        //        fullOutput = fullOutput.Replace("[indicator]", lastIndicator + " ");

        //    //Get the district or program
        //    fullOutput = fullOutput.Replace(TOKEN_DISTRICT, GetDistrictProgramme(result));

        //    //generate the performance reposnse
        //    string performance;
        //    PerformanceDBQuery query = new PerformanceDBQuery();
        //    if (lastDistrict == "")
        //    {
        //        //get performance for the program
        //        if (lastIndicator.ToLower() == "all indicators")
        //            performance = query.GetProgramPerformanceAsString(lastProgramme, (lastTerm == "Annual"), true);
        //        else
        //            performance = query.GetProgramPerformanceAsString(lastProgramme, (lastTerm == "Annual"), true, lastIndicator);

        //    }
        //    else
        //    {
        //        //get performance for the district
        //        if (lastIndicator.ToLower() == "all indicators")
        //            performance = query.GetDistrictPerformanceAsString(lastDistrict, (lastTerm == "Annual"), true);
        //        else
        //            performance = query.GetDistrictPerformanceAsString(lastDistrict, (lastTerm == "Annual"), true, lastIndicator);
        //        //performance = $"{actual} against a target of {target} which is {percentTarget}% of target";

        //    }
        //    fullOutput = fullOutput.Replace("[performance]", performance);



        //    //Add a note if required
        //    if (percentTarget > 100)
        //        fullOutput = fullOutput.Replace("[note]", "Great work!");
        //    else
        //        fullOutput = fullOutput.Replace("[note]", "");
        //}
    }

    public enum PerformanceType
    {
        Program,District,Facility,
    }
}