using System;
using System.Collections.Generic;

namespace Community.Foundation.ItemLens.Services
{
    public interface IValueGrouper {

        void Reset();
        int GetValueMatchGroup(string value);
    }


    public class ValueGrouper : IValueGrouper
    {
        public ValueGrouper() {
            Reset();
        }

        struct Constants
        {
            public const int MaxMatchGroup = 5; // this should match highest number of css styles (.match-group-5)
        }

        // Storage of unique field values for color coding exact matches
        private List<string> Values;


        public void Reset() {
            Values = new List<string>();
        }

        /// <summary>
        /// Returns which group match belongs with (1-based) up to Constants.MaxMatchGroup
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int GetValueMatchGroup(string value)
        {
            var match = Values.IndexOf(value) + 1;
            if (match == 0)
            {
                var numGroups = Values.Count;
                // this value starts a new group
                match = numGroups + 1;
                if (numGroups < Constants.MaxMatchGroup) // no need to add it if we already have too many unique values
                {
                    Values.Add(value);
                }
            }
            return Math.Min(match, Constants.MaxMatchGroup);
        }

    }
}