using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace LuisBot.LuisHelper
{
    /// <summary>
    /// Represents the json result from a LUIS call
    /// </summary>
    public class LuisFullResult
    {
        public LuisIntent TopIntent;
        public LuisIntent[] Intents;
        public LuisEntity[] Entities;

        //todo: create constructor that takes an app details and loads and populates trhis object

        public LuisFullResult(string json)
        {
            Newtonsoft.Json.Linq.JObject luisOutput = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json);

            //get the intents
            Intents = new LuisIntent[luisOutput["intents"].Count()];
            for (int i = 0; i < luisOutput["intents"].Count(); i++)
            {
                Newtonsoft.Json.Linq.JToken intentName = luisOutput["intents"][i]["intent"];
                Newtonsoft.Json.Linq.JToken intentScore = luisOutput["intents"][i]["score"];
                LuisIntent luisIntent = new LuisIntent(intentName.ToString(), double.Parse(intentScore.ToString()));
                Intents[i] = luisIntent;

                //set the top intent
                if (i == 0) TopIntent = luisIntent;
            }

            //get the entities
            Entities = new LuisEntity[luisOutput["entities"].Count()];
            for (int i = 0; i < luisOutput["entities"].Count(); i++)
            {
                LuisEntity luisEntity = LuisEntity.CreateLuisEntityFromJson(luisOutput, i);
                Entities[i] = luisEntity;
            }       


        }

        /// <summary>
        /// Try find an entity of a specific type
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="luisEntity"></param>
        /// <returns></returns>
        public bool TryFindEntity(string entityName, out LuisEntity luisEntity)
        {
            //loop though all entities till you find it
            foreach (LuisEntity entity in this.Entities)
            {
                if (entity.Name==entityName)
                {
                    luisEntity = entity;
                    return true;
                }
            }
            //If here, nothing was found
            luisEntity = null;
            return false;
        }

    }

    public class LuisIntent
    {
        /// <summary>
        /// Name of the intent.
        /// </summary>
        public string Name { get;  }
        /// <summary>
        /// Confidence score of the intent match.
        /// </summary>
        public double Score { get;  }

        internal LuisIntent(string name, double score)
        {
            this.Name = name;
            this.Score = score;
        }

    }

    /// <summary>
    /// Represents a LUIS entity
    /// </summary>
    /// <remarks>It's abstract as only fields are relevant to subclasses e.g. date </remarks>
    public abstract class LuisEntity
    {
        /// <summary>
        /// The name of the type of Entity, e.g. "Topic", "Person", "Location".
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The entity value, e.g. "Latest scores", "Alex", "Cambridge".
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// Confidence score that LUIS matched the entity, the higher the better.
        /// </summary>
        public double Score { get; set; }
        /// <summary>
        /// The index of the first character of the entity within the given text
        /// </summary>
        public int StartIndex { get; set; }
        /// <summary>
        /// The index of the last character of the entity within the given text
        /// </summary>
        public int EndIndex { get; set; }

        

        public static LuisEntity CreateLuisEntityFromJson(Newtonsoft.Json.Linq.JObject luisOutput, int entityNumber)
        {
            LuisEntity luisEntity;
            
            //Get entity and determine type
            string entityType=luisOutput["entities"][entityNumber]["type"].ToString();
            switch(entityType)
            {
                case ("builtin.datetimeV2.daterange"):
                    luisEntity = new LuisDatetimeEntity(
                        luisOutput["entities"][entityNumber]["resolution"]["values"][0]["timex"].ToString(),
                        luisOutput["entities"][entityNumber]["resolution"]["values"][0]["type"].ToString(),
                        luisOutput["entities"][entityNumber]["resolution"]["values"][0]["start"].ToString(),
                        luisOutput["entities"][entityNumber]["resolution"]["values"][0]["end"].ToString());                 
                    break;                    
                default:
                    luisEntity = new LuisStandardEntity();
                    ((LuisStandardEntity)luisEntity).TrueValue = luisOutput["entities"][entityNumber]["resolution"]["values"][0].ToString();
                    break;

            }
            //if (entityType!="DateRange")
            //{
            //    luisEntity = new LuisStandardEntity();
            //    ((LuisStandardEntity)luisEntity).TrueValue = luisOutput["entities"][entityNumber]["resolution"]["values"][0].ToString();
            //}
            //else
            //{
            //    throw new NotImplementedException();
            //}

            //Load the standard fields
            luisEntity.Name = entityType;
            luisEntity.Value = luisOutput["entities"][entityNumber]["entity"].ToString();
            luisEntity.Score= 0; //TODO: fix
            luisEntity.StartIndex =  int.Parse( luisOutput["entities"][entityNumber]["startIndex"].ToString());
            luisEntity.EndIndex = int.Parse(luisOutput["entities"][entityNumber]["endIndex"].ToString());

            return luisEntity; 
        }


    }

    public class LuisStandardEntity:LuisEntity
    {
        public string TrueValue { get; set; }
    }

    public class LuisDatetimeEntity : LuisEntity
    {
        public string Timex { get;  }
        public string SubType { get; }

        public DateTime StartDate { get;  }
        public DateTime EndDate { get; }

        public LuisDatetimeEntity(string timex, string subType, string startDate, string endDate )
        {
            this.EndDate = DateTime.Parse(endDate);
            this.StartDate = DateTime.Parse(startDate);
            this.Timex = timex;
            this.SubType = subType;
        }
    }
}