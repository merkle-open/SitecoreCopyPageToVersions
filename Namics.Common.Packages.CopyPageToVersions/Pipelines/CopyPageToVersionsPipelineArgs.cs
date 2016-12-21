using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Pipelines;

namespace Namics.Common.Packages.CopyPageToVersions.Pipelines
{
    [Serializable]
    public class CopyPageToVersionsPipelineArgs : PipelineArgs
    {
        public CopyPageToVersionsPipelineArgs()
        {
            TargetLanguages = new List<Language>();
            RelatedItems = new List<Item>();
            ErrorMessages = new List<string>();
            Stopwatch = new Stopwatch();
        }

        protected CopyPageToVersionsPipelineArgs(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        public bool IsDoNotCopyFieldValues { get; set; }
        public bool IsDoNotCreateRelatedItemVersions { get; set; }
        public bool IsForceCopyFieldValues { get; set; }
        public string SourceLanguageIdString { get; set; }
        public string TargetLanguageIdStrings { get; set; }
        public string PageId { get; set; }
        public string ResultMessage { get; set; }
        public Item SourcePageItem { get; set; }
        public Language SourceLanguage { get; set; }
        public List<Language> TargetLanguages { get; private set; }
        public List<Item> RelatedItems { get; private set; }
        public List<string> ErrorMessages { get; private set; }
        public Stopwatch Stopwatch { get; set; }
    }
}