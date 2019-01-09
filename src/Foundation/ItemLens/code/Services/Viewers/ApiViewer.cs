using Community.Foundation.ItemLens.Models;
using Sitecore.Data;
using System;
using System.Text;

namespace Community.Foundation.ItemLens.Services
{
    public class ApiViewer : IViewer
    {

        private IValueGrouper ValueGrouper;
        private ISmartUrl SmartUrl;

        public ApiViewer(IValueGrouper valueGrouper, ISmartUrl smartUrl)
        {
            ValueGrouper = valueGrouper;
            SmartUrl = smartUrl;
        }

        public string GetHtml(LensInput input)
        {
            return $@"
                    <div class=""card"">
                      <div class=""card-body"">
                        <h5 class=""card-title"">{input.DatabaseName} API</h5>
                        <p class=""card-text"">
                            {GetHtmlContents(input)}
                        </p>
                      </div>
                    </div>
                ";
        }

        public string GetHtmlContents(LensInput input)
        {
            if (input.Database == null)
            {
                return $"No {input.DatabaseName} database";
            }

            var db = input.Database;
            var item = db?.GetItem(input.ItemId);
            if (item == null)
                return "Item not found";

            var fieldId = input.FieldId;

            var sb = new StringBuilder();
            sb.AppendLine("<ul style=\"list-style: none;\">");

            // item info
            sb.AppendLine($"<li><b>Template:</b> {item.TemplateName}</li>");
            sb.AppendLine($"<li><b>Name:</b> {item.DisplayName}</li>");
            sb.AppendLine($"<li><b>Parent:</b> <a href=\"?{SmartUrl.ToQueryString(item.ParentID?.Guid, fieldId?.Guid, db)}\">{item.Parent?.DisplayName}</a></li>");
            sb.AppendLine($"<li><b>Path:</b> {item.Paths.ContentPath}</li>");
            sb.AppendLine($"<li><b>Language:</b> {item.Language}</li>");
            sb.AppendLine($"<li><b>Version:</b> {item.Version}</li>");
            sb.AppendLine($"<li><b>Publishable?</b> {item.Publishing.IsPublishable(DateTime.Now, true)}</li>");
            sb.AppendLine($"<li>&nbsp;</li>");
            sb.AppendLine($"<li><b>Item Created:</b> {item.Statistics.Created}</li>");
            sb.AppendLine($"<li><b>Item Created By:</b> {item.Statistics.CreatedBy}</li>");
            sb.AppendLine($"<li><b>Item Updated:</b> {item.Statistics.Updated}</li>");
            sb.AppendLine($"<li><b>Item Updated By:</b> {item.Statistics.UpdatedBy}</li>");

            // field info
            if (fieldId != (ID)null)
            {
                var fieldName = db?.GetItem(fieldId)?.Name ?? "Field not found";
                var fieldValue = item[fieldId];

                sb.AppendLine($"<li>&nbsp;</li>");
                sb.AppendLine($"<li><b>Field Name:</b> {fieldName}</li>");
                sb.AppendLine($"<li><b>Field Value:</b> <textarea class=\"form-control match-group-{ValueGrouper.GetValueMatchGroup(fieldValue)}\" rows=\"3\">{fieldValue}</textarea></li>");
            }

            sb.AppendLine("</ul>");
            return sb.ToString();
        }
    }
}