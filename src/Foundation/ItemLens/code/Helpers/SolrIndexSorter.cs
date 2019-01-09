using Sitecore.ContentSearch;
using System.Collections.Generic;

namespace Community.Foundation.ItemLens.Helpers
{
    public class SolrIndexSorter : IComparer<ISearchIndex>
    {
        public int Compare(ISearchIndex x, ISearchIndex y)
        {
            var xGroup = GetOptionGroup(x.Name);
            var yGroup = GetOptionGroup(y.Name);
            if (xGroup < yGroup)
                return -1;
            if (xGroup > yGroup)
                return 1;
            return string.CompareOrdinal(x.Name, y.Name);
        }


        protected int GetOptionGroup(string indexName)
        {
            indexName = indexName.ToLowerInvariant();
            if (indexName.Contains("master"))
                return 1;
            if (indexName.Contains("web"))
                return 2;
            if (indexName.Contains("preview"))
                return 3;
            return 4;
        }
    }
}