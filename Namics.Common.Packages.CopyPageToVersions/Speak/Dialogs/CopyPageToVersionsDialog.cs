using System;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Managers;
using Sitecore.Mvc.Presentation;
using Sitecore.Speak.Applications;
using Sitecore.Web;
using Sitecore.Web.PageCodes;

namespace Namics.Common.Packages.CopyPageToVersions.Speak.Dialogs
{
    /// <summary>
    /// Page code of the copy page to versions dialog, defined on the PageCode Speak rendering.
    /// </summary>
    public class CopyPageToVersionsDialog : PageCodeBase
    {
        public Rendering Option1CheckBox { get; set; }
        public Rendering SourceLanguageTreeView { get; set; }
        public Rendering TargetLanguageTreeView { get; set; }
        public Rendering PageId { get; set; }
        public Rendering SourceLanguage { get; set; }

        public string CurrentLanguage { get; set; }

        /// <summary>
        /// Initializes the dialog renderings
        /// </summary>
        public override void Initialize()
        {
            SelectMediaDialog.RedirectOnItembucketsDisabled(
                ClientHost.Items.GetItem(
                    Sitecore.Configuration.Settings.GetSetting("Namics.Common.Packages.CopyPageToVersions.DialogId")));
            ReadQueryParamsAndUpdatePlaceholders();
        }

        private void ReadQueryParamsAndUpdatePlaceholders()
        {
            InitPageId();
            InitTreeViews();
        }

        /// <summary>
        /// Initializes the page id from the delivered dialog url parameter
        /// </summary>
        private void InitPageId()
        {
            var idString = WebUtil.GetQueryString("id");
            PageId.Parameters["Text"] = idString;
        }

        private void InitTreeViews()
        {
            InitLanguageTreeViews();
        }

        /// <summary>
        /// Initializes the source and target language treeview, 
        /// from the delivered context language in the dialog url parameter.
        /// </summary>
        private void InitLanguageTreeViews()
        {
            CurrentLanguage = WebUtil.GetQueryString("lang");
            var lang = LanguageManager.GetLanguage(CurrentLanguage) ?? Context.Language;

            if (lang == null)
            {
                return;
            }

            var languageItemId = LanguageManager.GetLanguageItemId(lang, Context.ContentDatabase);

            if (ID.IsNullOrEmpty(languageItemId))
            {
                PreloadRootItem(SourceLanguageTreeView);
                PreloadRootItem(TargetLanguageTreeView);
                return;
            }

            SourceLanguage.Parameters["Text"] = languageItemId.ToString();
            InitTreeView(languageItemId.ToString(), SourceLanguageTreeView);
            PreloadRootItem(TargetLanguageTreeView);
        }

        private static void InitTreeView(string idString, Rendering treeView)
        {
            if (string.IsNullOrWhiteSpace(idString))
            {
                return;
            }

            string rootItemId;
            if (!TryGetRootItemId(treeView, out rootItemId))
            {
                return;
            }

            string idPath;
            if (!TryGetIdPath(idString, rootItemId, out idPath))
            {
                if (!TryGetIdPath(rootItemId, rootItemId, out idPath))
                {
                    return;
                }
            }

            treeView.Parameters["PreLoadPath"] = idPath;
        }

        private static bool TryGetIdPath(string id, string rootItemId, out string idPath)
        {
            idPath = string.Empty;

            var selectionItem = SelectMediaDialog.GetMediaItemFromQueryString(id);

            if (selectionItem == null)
            {
                return false;
            }

            idPath = selectionItem.Paths.LongID.Substring(1);
            idPath = idPath.Substring(idPath.IndexOf(rootItemId, StringComparison.InvariantCultureIgnoreCase));

            return true;
        }

        private static bool TryGetRootItemId(Rendering treeView, out string rootItemId)
        {
            rootItemId = treeView.Parameters["RootItem"];

            if (string.IsNullOrEmpty(rootItemId))
            {
                return false;
            }

            if (ID.IsID(rootItemId))
            {
                return true;
            }

            var item = Client.ContentDatabase.GetItem(rootItemId);

            if (item != null)
            {
                rootItemId = item.ID.ToString();
            }

            return true;
        }

        private static void PreloadRootItem(Rendering treeView, string rootItemId)
        {
            string idPath;
            if (!TryGetIdPath(rootItemId, rootItemId, out idPath))
            {
                return;
            }

            treeView.Parameters["PreLoadPath"] = idPath;
        }

        private static void PreloadRootItem(Rendering treeView)
        {
            string rootItemId;
            if (!TryGetRootItemId(treeView, out rootItemId))
            {
                return;
            }

            PreloadRootItem(treeView, rootItemId);
        }
    }
}