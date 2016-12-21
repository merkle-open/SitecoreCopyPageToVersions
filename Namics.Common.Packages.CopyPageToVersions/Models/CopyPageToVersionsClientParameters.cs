namespace Namics.Common.Packages.CopyPageToVersions.Models
{
    /// <summary>
    /// The client parameters model for the posted parameters from the copy page to versions dialog.
    /// </summary>
    public class CopyPageToVersionsClientParameters
    {
        public bool Option1CheckBox { get; set; }
        public bool Option2CheckBox { get; set; }
        public bool Option3CheckBox { get; set; }
        public string SourceLanguageTreeView { get; set; }
        public string TargetLanguageTreeView { get; set; }
        public string SourceLanguage { get; set; }
        public string PageId { get; set; }
    }
}