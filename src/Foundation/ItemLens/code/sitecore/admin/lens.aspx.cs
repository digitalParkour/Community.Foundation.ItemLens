using Community.Foundation.ItemLens.Helpers;
using Community.Foundation.ItemLens.Models;
using Community.Foundation.ItemLens.Services;
using Sitecore.ContentSearch.SolrProvider;
using Sitecore.Data;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;

namespace Community.Foundation.ItemLens
{
    public partial class ItemView : Sitecore.sitecore.admin.AdminPage
    {
        protected IValueGrouper GrouperService { get; set; }
        protected ISmartUrl SmartUrl { get; set; }
        protected ApiViewer ApiViewer { get; set; }
        protected SqlViewer SqlViewer { get; set; }
        protected SolrViewer SolrViewer { get; set; }

        protected override void OnInit(EventArgs arguments)
        {
            Assert.ArgumentNotNull(arguments, "arguments");
            if (!ByPassAuthCheck())
            {
                base.CheckSecurity(true);
            }
            this.InitComponent();
            base.OnInit(arguments);
        }

        protected bool ByPassAuthCheck()
        {
            return !string.IsNullOrWhiteSpace(Configs.CookieName)
                && !string.IsNullOrWhiteSpace(Configs.CookieValue)
                && Request.Cookies[Configs.CookieName] != null
                && Request.Cookies[Configs.CookieName].Value.Equals(Configs.CookieValue);        
        }

        protected void InitComponent() {
            GrouperService = new ValueGrouper();
            SmartUrl = new SmartUrl();

            ApiViewer = new ApiViewer(GrouperService, SmartUrl);
            SqlViewer = new SqlViewer(GrouperService, SmartUrl);
            if (Configs.IsUsingSolr)
                SolrViewer = new SolrViewer();

            InitDropDowns();
        }
              

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack) {
                InitFormByUrl();
            }
        }
        
        protected void InitFormByUrl()
        {
            var input1 = SmartUrl.ExtractInput(Request.Params, 1);
            var input2 = SmartUrl.ExtractInput(Request.Params, 2);

            // Sync form controls
            if (!string.IsNullOrWhiteSpace(input1.DatabaseName))
            {
                if (ddlDatabase1.Items.FindByValue(input1.DatabaseName) != null)
                    ddlDatabase1.SelectedValue = input1.DatabaseName;
            }
            if (!string.IsNullOrWhiteSpace(input2.DatabaseName))
            {
                if (ddlDatabase2.Items.FindByValue(input2.DatabaseName) != null)
                    ddlDatabase2.SelectedValue = input2.DatabaseName;
            }

            // Item
            txtItemId.Text = input1.ItemId?.ToString();

            // Solr
            if (Configs.IsUsingSolr)
            {
                if(!string.IsNullOrWhiteSpace(input1.SolrIndexes?[0])) ddlIndex1.SelectedValue = input1.SolrIndexes?[0];
                if(!string.IsNullOrWhiteSpace(input2.SolrIndexes?[0])) ddlIndex2.SelectedValue = input2.SolrIndexes?[0];
                if(!string.IsNullOrWhiteSpace(input1.SolrIndexes?[1])) ddlIndex3.SelectedValue = input1.SolrIndexes?[1];
                if(!string.IsNullOrWhiteSpace(input2.SolrIndexes?[1])) ddlIndex4.SelectedValue = input2.SolrIndexes?[1];
            }

            if (input1.ItemId != (ID)null)
            {                
                // Field
                LoadFields(input1.ItemId, input1.Database ?? input2.Database);
                ddlField.SelectedValue = input1.FieldId.ToGuid().ToString();

                // and load item into viewer
                LoadItem(input1, input2);
            }
        }

        #region Event Handlers

        public void btnSubmit_Click(Object sender, EventArgs e)
        {
            var input1 = new LensInput(
                ddlDatabase1.SelectedValue,
                txtItemId.Text,
                ddlField.SelectedValue,
                new[] { ddlIndex1.SelectedValue, ddlIndex3.SelectedValue }
            );

            var input2 = new LensInput(
                ddlDatabase2.SelectedValue,
                txtItemId.Text,
                ddlField.SelectedValue,
                new[] { ddlIndex2.SelectedValue, ddlIndex4.SelectedValue }
            );
            
            LoadItem(input1, input2);
        }

        public void sourceInput_Change(Object sender, EventArgs e)
        {
            Utils.TryGetFirstDatabase(new[] { ddlDatabase1.SelectedValue, ddlDatabase2.SelectedValue }, out Database db);
            LoadFields(Utils.GetItemId(txtItemId.Text, db), db);
        }

        #endregion

        protected void LoadItem(params LensInput[] input)
        {
            // Init values comparison
            GrouperService.Reset();

            // Populate API/Cached results
            ResultApiMaster.Text = ApiViewer.GetHtml(input?[0]);
            ResultApiWeb.Text = ApiViewer.GetHtml(input?[1]);

            // Populate SQL results
            ResultSqlMaster.Text = SqlViewer.GetHtml(input?[0]);
            ResultSqlWeb.Text = SqlViewer.GetHtml(input?[1]);

            // Populate Solr results
            if(Configs.IsUsingSolr)
            { 
                litIndexesLeft.Text = SolrViewer.GetHtml(input?[0]);
                litIndexesRight.Text = SolrViewer.GetHtml(input?[1]);
            }
            // Set Share
            litShare.Text = $"<a href=\"?{SmartUrl.ToQueryString(input)}\" class=\"btn btn-secondary\">Share Link</a>";
        }


        protected void InitDropDowns()
        {
            // INIT Database Dropdown options
            var textInfo = new CultureInfo("en-US", false).TextInfo;
            var databases = Sitecore.Configuration.Factory.GetDatabaseNames();

            foreach (var dbName in databases) {
                var lowerDbName = dbName.ToLowerInvariant();
                ddlDatabase1.Items.Add(new ListItem { Text = textInfo.ToTitleCase(dbName), Value = lowerDbName, Selected = lowerDbName.Equals(Configs.Default.DatabaseLeft) });
            }
            foreach (var dbName in databases)
            {
                var lowerDbName = dbName.ToLowerInvariant();
                ddlDatabase2.Items.Add(new ListItem { Text = textInfo.ToTitleCase(dbName), Value = lowerDbName, Selected = lowerDbName.Equals(Configs.Default.DatabaseRight) });
            }


            // INIT Index drop down options
            ddlIndex1.Items.Add(new ListItem { Text = "none", Value = "" });
            ddlIndex2.Items.Add(new ListItem { Text = "none", Value = "" });
            ddlIndex3.Items.Add(new ListItem { Text = "none", Value = "" });
            ddlIndex4.Items.Add(new ListItem { Text = "none", Value = "" });
            var customOrder = new SolrIndexSorter();
            foreach (var index in Sitecore.ContentSearch.ContentSearchManager.Indexes.OrderBy(x => x, customOrder))
            {
                var solrIndex = index as SolrSearchIndex;
                if (solrIndex == null)
                    continue;

                ddlIndex1.Items.Add(new ListItem { Text = index.Name, Value = solrIndex.Core, Selected = index.Name.Equals(Configs.Default.SolrIndexLeft1) });
                ddlIndex2.Items.Add(new ListItem { Text = index.Name, Value = solrIndex.Core, Selected = index.Name.Equals(Configs.Default.SolrIndexRight1) });
                ddlIndex3.Items.Add(new ListItem { Text = index.Name, Value = solrIndex.Core, Selected = index.Name.Equals(Configs.Default.SolrIndexLeft2) });
                ddlIndex4.Items.Add(new ListItem { Text = index.Name, Value = solrIndex.Core, Selected = index.Name.Equals(Configs.Default.SolrIndexRight2) });

            }
            
        }        
                

        protected void LoadFields(ID itemId, Database db) {
            var oldValue = ddlField.SelectedValue;
            ddlField.Items.Clear();
            ddlField.Items.Add(new ListItem { Text = "none", Value = string.Empty, Selected = string.IsNullOrWhiteSpace(oldValue) });

            if (itemId == (ID)null)
                return;

            // Default database
            db = db ?? Sitecore.Context.ContentDatabase ?? Sitecore.Context.Database;
            var item = db?.GetItem(itemId);
            if (item == null)
                return;

            var template = TemplateManager.GetTemplate(item);
            if (template == null)
                return;

            var lang = Sitecore.Context.Language;
            var fields = template.GetFields(true).Select(x => {
                var title = x.GetTitle(lang);
                if (string.IsNullOrWhiteSpace(title))
                {
                    title = x.Name;
                }
                else {
                    if (title != x.Name)
                        title += $" ({x.Name})";
                }
                var value = x.ID.Guid.ToString();
                return new ListItem { Text = title, Value = value, Selected = oldValue == value };
            }).OrderBy(x => x.Text).ToArray();
                
            ddlField.Items.AddRange(fields);
        }


       
    }
}