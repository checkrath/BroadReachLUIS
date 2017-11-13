using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LuisBot
{
    public class MemoryField
    {
        public string Value {get; set;}
        public string Name { get; set; }
        public int MinutesToPersist { get; set; }
        public string DefaultValue { get; set; }
    }
    public class BotMemory
    {
        public void SetupField(string name, string value, int minutesToPersist = -1)
        {
        }

        public void SetFieldValue(string name, string value)
        {

        }

        public string GetFieldValue(string name)
        {
            return "";
        }

        public void UpdateFieldOrReturnDefault(string name, string value)
        {

        }
    }
}