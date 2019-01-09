namespace Community.Foundation.ItemLens.Helpers
{
    public class Configs
    {
        public readonly static string CookieName = Sitecore.Configuration.Settings.GetSetting("ItemLens.SecurityPass.CookieName", "ItemLensKey");
        public readonly static string CookieValue = Sitecore.Configuration.Settings.GetSetting("ItemLens.SecurityPass.CookieValue", "");

        public readonly static bool IsUsingSolr = Sitecore.Configuration.Settings.GetBoolSetting("ItemLens.IsUsingSolr", true);

        public struct Default
        { 
            public readonly static string DatabaseLeft = Sitecore.Configuration.Settings.GetSetting("ItemLens.Default.DatabaseLeft", "master");
            public readonly static string DatabaseRight = Sitecore.Configuration.Settings.GetSetting("ItemLens.Default.DatabaseRight", "web");

            public readonly static string SolrIndexLeft1 = Sitecore.Configuration.Settings.GetSetting("ItemLens.Default.SolrIndexLeft1", "sitecore_master_index");
            public readonly static string SolrIndexLeft2 = Sitecore.Configuration.Settings.GetSetting("ItemLens.Default.SolrIndexLeft2", "");
            public readonly static string SolrIndexRight1 = Sitecore.Configuration.Settings.GetSetting("ItemLens.Default.SolrIndexRight1", "sitecore_web_index");
            public readonly static string SolrIndexRight2 = Sitecore.Configuration.Settings.GetSetting("ItemLens.Default.SolrIndexRight2", "");
        }
    }
}