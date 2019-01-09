using Sitecore;
using Sitecore.Data;
using System;
using System.Web;

namespace Community.Foundation.ItemLens.Helpers
{
    public static class Utils
    {
        /// <summary>
        /// Given user input return sitecore database object else return null if input does not match name in sitecore configuration
        /// </summary>
        /// <param name="dbName">Name of database</param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static bool TryGetDatabase(string dbName, out Database db)
        {
            if (string.IsNullOrWhiteSpace(dbName))
            {
                db = null;
                return false;
            }

            try
            {
                db = Database.GetDatabase(dbName?.Trim());
            }
            // error: database does not exist (ie: master on CD)
            catch (InvalidOperationException ex)
            {
                db = null;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Support list of dbnames, return first valid
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="dbName2"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static bool TryGetFirstDatabase(string[] dbNames, out Database db)
        {
            foreach (var dbName in dbNames)
            {
                if (TryGetDatabase(dbName, out db))
                    return true;
            }

            db = null;
            return false;
        }

        /// <summary>
        /// Given ID, Guid, Content Path, or URL, return Item ID
        /// </summary>
        /// <param name="input">ID, Guid, Content Path, or URL</param>
        /// <param name="db">optional but recommended</param>
        /// <returns></returns>
        public static ID GetItemId(string input, Database db)
        {
            input = input?.ToLowerInvariant().Trim();

            // Check if is ID
            if (ID.TryParse(input, out ID id))
            {
                return id;
            }

            // Default DB:
            db = db ?? Sitecore.Context.ContentDatabase ?? Sitecore.Context.Database;

            // Check if is full path
            var item = db?.GetItem(input);
            if (item != null)
                return item.ID;

            // Check by site relative path
            var path = StringUtil.EnsurePrefix('/', Sitecore.Context.Site?.StartPath.TrimEnd('/') ?? string.Empty);
            if (path.Length <= 1)
                return null;
            // strip domain name
            if (input.StartsWith("http"))
            {
                input = input.Substring(input.IndexOf('/', "https://x".Length));
            }
            // strip params
            if (input.Contains("?"))
                input = input.Substring(0, input.IndexOf('?'));
            if (input.Contains("#"))
                input = input.Substring(0, input.IndexOf('#'));
            // strip file suffixes
            if (input.Contains("."))
            {
                var pos = input.LastIndexOf('.');
                if (pos >= input.Length - 5)
                {
                    input = input.Substring(0, pos);
                }
            }
            path += StringUtil.EnsurePrefix('/', HttpUtility.UrlDecode(input.Trim()));
            item = db.GetItem(path);
            if (item != null)
                return item.ID;

            return null;
        }
    }
}