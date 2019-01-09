using Community.Foundation.ItemLens.Models;
using Sitecore.Data;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Community.Foundation.ItemLens.Services
{
    public class SqlViewer : IViewer
    {
        private IValueGrouper ValueGrouper;
        private ISmartUrl SmartUrl;
        public SqlViewer(IValueGrouper valueGrouper, ISmartUrl smartUrl) {
            ValueGrouper = valueGrouper;
            SmartUrl = smartUrl;
        }

        public string GetHtml(LensInput input)
        {
            return $@"
                    <div class=""card"">
                      <div class=""card-body"">
                        <h5 class=""card-title"">{input.DatabaseName} SQL</h5>
                        <p class=""card-text"">
                            {GetHtmlContents(input)}
                        </p>
                      </div>
                    </div>
                ";
        }

        public string GetHtmlContents(LensInput input)
        {
            var connName = input.DatabaseName;
            if (string.IsNullOrWhiteSpace(connName))
                return "No connection string";

            var connString = ConfigurationManager.ConnectionStrings[connName]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(connString))
            {
                return $"No {connName} connection";
            }

            var db = input.Database;
            var itemId = input.ItemId;
            var fieldId = input.FieldId;

            var sb = new StringBuilder();

            using (var sqlConn = new SqlConnection(connString))
            {

                sqlConn.Open();

                // Item and dups

                sb.AppendLine("<table class=\"table\">");
                sb.AppendLine("<thead><tr>");
                sb.AppendLine("<th scope=\"col\">Template</th>");
                sb.AppendLine("<th scope=\"col\">Item Name</th>");
                sb.AppendLine("<th scope=\"col\">Item Id</th>");
                sb.AppendLine("<th scope=\"col\">Updated</th>");
                sb.AppendLine("<th scope=\"col\">Created</th>");
                sb.AppendLine("</tr></thead>");
                sb.AppendLine("<tbody>");

                if (itemId != (ID)null)
                {
                    // No risk of SQL injection as we are using ID types for input
                    // UNION dups by name and location
                    var iquery = $@"SELECT t.[Name] as Template, i.[Name], i.ID as ItemId, i.Created, i.Updated
                                    FROM [Items] i
                                    LEFT JOIN Items t on t.ID = i.TemplateID
                                    WHERE i.ID = '{itemId}'
                                    UNION
                                    SELECT t2.[Name] as Template, d.[Name], d.ID as ItemId, d.Created, d.Updated
                                    FROM [Items] x
                                    INNER JOIN [Items] d on( d.[Name] = x.[Name] and d.ParentID = x.ParentID and d.ID != x.ID)
                                    LEFT JOIN Items t2 on t2.ID = d.TemplateID
                                    WHERE x.ID = '{itemId}'";

                    var icmd = new SqlCommand
                    {
                        CommandText = iquery,
                        CommandType = CommandType.Text,
                        Connection = sqlConn
                    };

                    using (var reader = icmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var template = reader["Template"].ToString();
                            var name = reader["Name"].ToString();
                            var id = reader["ItemId"].ToString();
                            var created = reader["Created"].ToString();
                            var updated = reader["Updated"].ToString();
                            sb.AppendLine("<tr>");
                            sb.AppendLine($"<td>{template}</td>");
                            sb.AppendLine($"<th scope=\"row\">{name}</th>");
                            if (id == itemId.Guid.ToString())
                                sb.AppendLine($"<td>{id}</td>");
                            else
                                sb.AppendLine($"<td><span class=\"duplicate-item\">DUP!</span> <a href=\"?{SmartUrl.ToQueryString(Guid.Parse(id), fieldId?.Guid, db)}\">{id}</a></td>");
                            sb.AppendLine($"<td>{updated}</td>");
                            sb.AppendLine($"<td>{created}</td>");
                            sb.AppendLine("</tr>");
                        }
                    }
                }

                sb.AppendLine("</tbody>");
                sb.AppendLine("</table>");

                // Field Values

                if (fieldId != (ID)null)
                {
                    sb.AppendLine("<table class=\"table\">");
                    sb.AppendLine("<thead><tr>");
                    sb.AppendLine("<th scope=\"col\">Language</th>");
                    sb.AppendLine("<th scope=\"col\">Version</th>");
                    sb.AppendLine("<th scope=\"col\">Value</th>");
                    sb.AppendLine("<th scope=\"col\">Value Updated</th>");
                    sb.AppendLine("<th scope=\"col\">Value Created</th>");
                    sb.AppendLine("</tr></thead>");
                    sb.AppendLine("<tbody>");

                    // No risk of SQL injection as we are using ID types for input
                    var vquery = $@"SELECT Language, Version, Value, Created, Updated 
                                    FROM VersionedFields
                                    Where itemId = '{itemId}' and fieldId = '{fieldId}'
                               UNION
                               SELECT Language, 0 as Version, Value, Created, Updated 
                                    FROM UnVersionedFields
                                    Where itemId = '{itemId}' and fieldId = '{fieldId}'
                                UNION
                                SELECT 'shared' as Language, 0 as Version, Value, Created, Updated
                                    FROM SharedFields
                                    Where itemId = '{itemId}' and fieldId = '{fieldId}'
                                ORDER by Language, Version DESC";

                    var vcmd = new SqlCommand
                    {
                        CommandText = vquery,
                        CommandType = CommandType.Text,
                        Connection = sqlConn
                    };

                    using (var reader = vcmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var language = reader["Language"].ToString();
                            var version = reader["Version"].ToString();
                            var value = reader["Value"].ToString();
                            var created = reader["Created"].ToString();
                            var updated = reader["Updated"].ToString();
                            sb.AppendLine("<tr>");
                            sb.AppendLine($"<td>{language}</td>");
                            sb.AppendLine($"<th scope=\"row\">{version}</th>");
                            sb.AppendLine($"<td><textarea class=\"form-control match-group-{ValueGrouper.GetValueMatchGroup(value)}\" rows=\"10\">{value}</textarea></td>");
                            sb.AppendLine($"<td>{updated}</td>");
                            sb.AppendLine($"<td>{created}</td>");
                            sb.AppendLine("</tr>");
                        }
                    }

                    sb.AppendLine("</tbody>");
                    sb.AppendLine("</table>");
                }

            }
            return sb.ToString();
        }

    }
}