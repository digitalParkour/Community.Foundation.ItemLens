using Community.Foundation.ItemLens.Helpers;
using Sitecore.Data;
using System.Web;

namespace Community.Foundation.ItemLens.Models
{
    public class LensInput
    {
        #region Constructors
        public LensInput(string db, string item, string fieldId, string[] indexes)
        {
            InitFromStrings(
                db,
                item,
                fieldId,
                indexes
            );
        }
        #endregion

        #region Properties
        public Database Database { get; set; }
        public string DatabaseName { get; set; }


        public ID ItemId { get; set; }
        public ID FieldId { get; set; }


        public string[] SolrIndexes { get; set; }
        #endregion

        #region InitHelpers

        private void InitFromStrings(string dbName, string item, string field, string[] indexes)
        {
            // db
            if (!string.IsNullOrWhiteSpace(dbName))
            {
                this.DatabaseName = HttpUtility.UrlDecode(dbName).Trim();
                if (Utils.TryGetDatabase(this.DatabaseName, out Database db))
                {
                    this.Database = db;
                }
            }
            // item
            if (!string.IsNullOrWhiteSpace(item))
            {
                ItemId = Utils.GetItemId(item, this.Database);
            }
            // field
            if (!string.IsNullOrWhiteSpace(field) && ID.TryParse(field, out ID fieldId))
            {
                FieldId = fieldId;
            }

            // Solr
            if(Configs.IsUsingSolr)
            { 
                SolrIndexes = indexes;               
            }
        }
        #endregion
    }
}