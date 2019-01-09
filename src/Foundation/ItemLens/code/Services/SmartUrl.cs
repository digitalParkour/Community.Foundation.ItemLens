using Community.Foundation.ItemLens.Helpers;
using Community.Foundation.ItemLens.Models;
using Sitecore.Data;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Community.Foundation.ItemLens.Services
{
    public interface ISmartUrl
    {
        string ToQueryString(LensInput[] inputs);
        string ToQueryString(Guid? item, Guid? field, Database db);
        LensInput ExtractInput(NameValueCollection queryString, int group);
        
    }
    public class SmartUrl : ISmartUrl
    {        

        public string ToQueryString(Guid? item, Guid? field, Database db)
        {
            return $"item={item}&field={field}&db1={db}";
        }


        public string ToQueryString(LensInput[] inputs)
        {
            var qs = new StringBuilder();
            if (!inputs.Any())
            {
                return null;
            }

            var o = inputs.FirstOrDefault();
            qs.Append($"item={o.ItemId?.Guid}&field={o.FieldId?.Guid}");

            for (var group = 1; group <= inputs.Length; group++) {
                var x = inputs[group - 1];
                qs.Append($"&db{group}={x.DatabaseName}");
                if(x.SolrIndexes != null)
                    qs.Append($"&indexes{group}={string.Join(",", x.SolrIndexes)}");
            }
            return qs.ToString();
        }

        public LensInput ExtractInput(NameValueCollection queryString, int group)
        {
            string[] arIndexes = null;

            // Solr
            if (Configs.IsUsingSolr)
            {
                // indexes
                var indexes = queryString[$"indexes{group}"];
                if (!string.IsNullOrWhiteSpace(indexes))
                {
                    arIndexes = indexes.Trim().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                }
            }

            return new LensInput(
                queryString[$"db{group}"]?.ToLowerInvariant().Trim(),
                queryString["item"]?.Trim(),
                queryString["field"]?.Trim(),
                arIndexes
            );
        }
    }
}