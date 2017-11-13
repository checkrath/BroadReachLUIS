using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LuisBot
{
    public enum FieldSetSource
    {
        Initiation,
        UserProfile,
        ExplicitSet,
        LUISResult
    }
    [Serializable]
    public class MemoryField
    {
        public string Value {get; set;}
        public string Name { get; set; }
        public int MinutesToPersist { get; set; }
        public string DefaultValue { get; set; }
        public FieldSetSource SetSource { get; set; }
    }
    [Serializable]
    public class BotMemory
    {
        // Memory
        private static int _maxMemoryMinutes = 1440;
        private Dictionary<string, MemoryField> _memoryStore;

        public BotMemory()
        {
            _memoryStore = new Dictionary<string, MemoryField>();
        }

        public void CreateField(string name, string defautValue, string currentvalue = "", int minutesToPersist = -1)
        {
            // Exists?
            if (_memoryStore.ContainsKey(name))
                _memoryStore.Remove(name);

            // Check the timeout
            int minsToPersist = minutesToPersist;
            if (minsToPersist == -1)
                minsToPersist = _maxMemoryMinutes;

            // Add
            _memoryStore.Add(name, new MemoryField() { Name=name, DefaultValue= defautValue, MinutesToPersist= minsToPersist, Value=currentvalue, SetSource=FieldSetSource.Initiation});
        }

        public void SetFieldValue(string name, string value)
        {
            if (!_memoryStore.ContainsKey(name))
                throw new Exception($"No memory item by name {name} found in the store");
            _memoryStore[name].Value = value;
            _memoryStore[name].SetSource = FieldSetSource.ExplicitSet;
        }

        #region Accessor Code

        public string TryGetFieldValue(string name)
        {
            if (!_memoryStore.ContainsKey(name))
                return "";

            return GetFieldValue(name);
        }

        public string GetFieldValue(string name)
        {
            if (!_memoryStore.ContainsKey(name))
                throw new Exception($"No memory item by name {name} found in the store");

            MemoryField m = _memoryStore[name];

            if (String.IsNullOrEmpty(m.Value))
                return m.DefaultValue;
            else
                return m.Value;
        }

        /// <summary>
        /// This method returns the default value if a blank is passed in, or sets the value to the value passed in and then returns it
        /// </summary>
        /// <param name="name">Name of the memory store</param>
        /// <param name="newValue">Entity Value to use</param>
        /// <returns></returns>
        public string GetFieldValueWithEntityUpdate(string name, string newValue, out bool foundInLUISResults)
        {
            if (!_memoryStore.ContainsKey(name))
                throw new Exception($"No memory item by name {name} found in the store");

            // If the new value is blank, get the default
            if (String.IsNullOrEmpty(newValue))
            {
                foundInLUISResults = false;
                // Set that it found it in LUIS Results
                return GetFieldValue(name); // Returns value or default
            }
            else
            {
                foundInLUISResults = true;
                _memoryStore[name].SetSource = FieldSetSource.LUISResult;
                _memoryStore[name].Value = newValue;
                return newValue;
            }
        }

        /// <summary>
        /// This method tries to extract the entity name. If it finds it, it sets the memory to that value
        /// if not, it gets the default value of the entity
        /// </summary>
        /// <param name="name">Memory Name</param>
        /// <param name="EntityName">Entity Name</param>
        /// <param name="result">LUIS Result Object</param>
        /// <returns></returns>
        public string GetFieldValueWithEntityUpdate(string name, string entityName, LuisHelper.LuisFullResult result, out bool foundFromLUISResult)
        {
            if (!_memoryStore.ContainsKey(name))
                throw new Exception($"No memory item by name {name} found in the store");

            // Try to get it, but this time with the entity value from LUIS
            string newValue = result.GetEntityValue(entityName, "");
            return GetFieldValueWithEntityUpdate(name, newValue, out foundFromLUISResult);
        }

        // Helper
        public string this[string s]
        {
            get { return GetFieldValue(s);  }
        }

        // Get the whole object
        public MemoryField GetField(string name)
        {
            if (!_memoryStore.ContainsKey(name))
                throw new Exception($"No memory item by name {name} found in the store");

            return _memoryStore[name];
        }

        #endregion

        public void UpdateFieldOrReturnDefault(string name, string value)
        {
            throw new NotImplementedException();
        }

        public void ClearAllMemory()
        {
            throw new NotImplementedException();
        }
    }
}