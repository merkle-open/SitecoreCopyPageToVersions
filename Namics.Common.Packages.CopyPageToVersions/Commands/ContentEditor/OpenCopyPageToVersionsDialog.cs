using System;
using System.Collections.Specialized;
using Sitecore;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;
using Sitecore.Web.UI.Sheer;

namespace Namics.Common.Packages.CopyPageToVersions.Commands.ContentEditor
{
    /// <summary>
    /// Command to open the copy page to versions dialog from content editor
    /// </summary>
    [Serializable]
    public class OpenCopyPageToVersionsDialog : Command
    {
        /// <summary>
        /// Command execution, which runs the pipeline, which opens the copy page to versions dialog
        /// </summary>
        /// <param name="context">The context</param>
        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            if (context.Items.Length != 1)
            {
                return;
            }

            NameValueCollection parameters = new NameValueCollection();
            parameters["items"] = SerializeItems(context.Items);

            ClientPipelineArgs args = new ClientPipelineArgs(parameters);
            Context.ClientPage.Start(this, "Run", args);
        }

        /// <summary>
        /// Runs the pipeline, which opens the copy page to versions dialog
        /// </summary>
        /// <param name="args">The pipeline args.</param>
        protected void Run(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");

            Item[] items = DeserializeItems(args.Parameters["items"]);
            Item selectedItem = items[0];
            if (items.Length == 0)
            {
                SheerResponse.Alert("Unable to get the selected item for the copy to version dialog! Maybe it was deleted by another user, or it is not accessible.");
                Context.ClientPage.SendMessage(this, "item:refresh");
                return;
            }

            selectedItem.Fields.ReadAll();

            var layoutField = new LayoutField(selectedItem.Fields[FieldIDs.LayoutField]);
            if (string.IsNullOrEmpty(layoutField.Value))
            {
                SheerResponse.Alert("Unable to get the selected item for the copy to version dialog! Maybe it was deleted by another user, or it is not accessible.");
                Context.ClientPage.SendMessage(this, "item:refresh");
                return;
            }

            if (!SheerResponse.CheckModified())
            {
                return;
            }

            if (args.IsPostBack)
            {
                return;
            }

            if (selectedItem.Appearance.ReadOnly)
            {
                SheerResponse.Alert("You cannot edit the '{0}' item because it is protected.", selectedItem.DisplayName);
                return;
            }

            if (!selectedItem.Access.CanWrite())
            {
                SheerResponse.Alert("You cannot edit this item because you do not have write access to it.");
                return;
            }

            UrlString urlString = new UrlString("/sitecore/client/Applications/Dialogs/CopyPageToVersionsDialog");
            selectedItem.Uri.AddToUrlString(urlString);
            urlString["id"] = selectedItem.ID.ToString();
            urlString["lang"] = selectedItem.Language.Name;

            bool flag = string.Equals(Context.Language.Name, "ja-JP", StringComparison.InvariantCultureIgnoreCase);
            SheerResponse.ShowModalDialog(urlString.ToString(), flag ? "650px" : "500px", flag ? "690px" : "650px", string.Empty, true);

            args.WaitForPostBack();
        }

        /// <summary>
        /// Queries the state of the command. 
        /// The command should only be triggered if a valid item is selected, 
        /// which has a version and a defined layout.
        /// </summary>
        /// <param name="context">The context</param>
        /// <returns>The state of the command</returns>
        public override CommandState QueryState(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            if (context.Items.Length != 1)
            {
                return CommandState.Hidden;
            }

            Item selectedItem = context.Items[0];
            if (selectedItem.TemplateID == TemplateIDs.Template ||
                selectedItem.TemplateID == TemplateIDs.TemplateSection ||
                selectedItem.TemplateID == TemplateIDs.TemplateField)
            {
                return CommandState.Hidden;
            }

            Sitecore.Data.Version[] versionNumbers = selectedItem.Versions.GetVersionNumbers(false);
            if (versionNumbers == null || versionNumbers.Length == 0 || selectedItem.Appearance.ReadOnly)
            {
                return CommandState.Disabled;
            }

            if (!selectedItem.Access.CanWrite() || !selectedItem.Access.CanRemoveVersion() ||
                IsLockedByOther(selectedItem))
            {
                return CommandState.Disabled;
            }

            selectedItem.Fields.ReadAll();
            LayoutField layoutField = new LayoutField(selectedItem.Fields[FieldIDs.LayoutField]);
            return string.IsNullOrEmpty(layoutField.Value) ? CommandState.Disabled : base.QueryState(context);
        }
    }
}