using Community.Foundation.ItemLens.Models;
using Sitecore.Data;
using System;
using System.Net;
using System.Text;

namespace Community.Foundation.ItemLens.Services
{
    public class SolrViewer : IViewer
    {
        public string GetHtml(LensInput input)
        {
            var indexNames = input.SolrIndexes;
            var itemId = input.ItemId;

            var sb = new StringBuilder();
            foreach (var indexName in indexNames)
            {
                // Get Solr Response
                var json = GetSolrResponse(indexName, itemId);

                var html = $@"               
                    <div class=""card"">
                        <div class=""card-body"">
                            <h5 class=""card-title"">{indexName} Index</h5>
                            <p class=""card-text"">
                                <textarea rows=""11"" cols=""20"" readonly=""readonly"" class=""form-control"">{json}</textarea>
                            </p>
                        </div>
                    </div>
                ";
                sb.AppendLine(html);
            }

            return sb.ToString();
        }


        private string GetSolrResponse(string indexName, ID itemId)
        {
            var solrEndpoint = Sitecore.Configuration.Settings.GetSetting("ContentSearch.Solr.ServiceBaseAddress")?.TrimEnd('/');
			// Sitecore 9.1+ moved solr connstring
			if (string.IsNullOrWhiteSpace(solrEndpoint))
            {
                solrEndpoint = System.Configuration.ConfigurationManager.ConnectionStrings["solr.search"]?.ToString().TrimEnd('/');
            }
            if (
                string.IsNullOrWhiteSpace(solrEndpoint)
                || string.IsNullOrWhiteSpace(indexName)
                || itemId == (ID)null
                )
                return string.Empty;

            var normId = itemId.Guid.ToString().Replace("-", string.Empty);

            var sb = new StringBuilder();
            var solrRequest = $"{solrEndpoint}/{indexName}/select?indent=on&q=_group:{normId}&wt=json";

            // Append request string for reference
            sb.AppendLine(solrRequest);
            try
            {
                using (var client = new WebClient())
                {
                    string result = client.DownloadString(solrRequest);
                    // Append json response
                    sb.AppendLine(result);
                }
            }
            catch (Exception ex)
            {
                // Append error
                sb.AppendLine(ex.Message);
            }

            return sb.ToString();
        }

    }
}