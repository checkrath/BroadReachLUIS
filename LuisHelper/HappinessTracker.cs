using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LuisBot.LuisHelper
{
    /// <summary>
    /// Will track the users average happiness when each interaction is rated from -5 (fail) to 5 (success)
    /// </summary>
    [Serializable]
    public class HappinessTracker
    {
        public int[] LastRatings;
        public int RatingsCount { get; set; }

        public HappinessTracker()
        {
            LastRatings = new int[5];
            RatingsCount = 0;
        }

        //Add a rating from 
        public void AddRating(int rating)
        {
            //Limit values
            if (rating > 5 || rating < -5) throw new Exception("Rating must be in the range -5 to 5");

            int historySize = 4;
            if (RatingsCount < 4) historySize = RatingsCount;
            //shift values      
            for (int i=0; i< historySize; i++ )  
                LastRatings[i + 1] = LastRatings[i];

            //add new rating
            LastRatings[0] = rating;

            //increase rating counter
            this.RatingsCount += 1;
        }

        public void ResetRatings()
        {
            LastRatings = new int[5];
            RatingsCount = 0;
        }

        public int AverageRating            
        {
            get
            {
                int historySize = 5;
                if (RatingsCount < 5) historySize = RatingsCount;
                if (historySize == 0) return 0;

                //Loop and create total
                int total = 0;
                for (int i = 0; i < historySize; i++)
                {
                    total += LastRatings[i];
                }

                //return average
                return total / historySize;
            }
        }


    }
}