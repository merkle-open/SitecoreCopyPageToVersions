using System.Collections.Generic;
using System.Linq;
using Namics.Common.Packages.CopyPageToVersions.Pipelines;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Globalization;

namespace Namics.Common.Packages.CopyPageToVersions.Processors
{
    /// <summary>
    /// Processes the posted inputs form the dialog and prepares the pipeline args for the item aggregation. 
    /// </summary>
    public class CopyPageToVersionsInputProcessor
    {
        private const string ErrorMessagePrefix = "CopyPageToVersionsInputProcessor: ";
        private const string ErrorMessageDatabase = "Master database not available";
        private const string ErrorMessageSourceLanguage = "Failed to parse source language from selected id {0}";
        private const string ErrorMessageNoTargetLanguages = "No target languages available for version creation";
        private const string ErrorMessageNoLanguagesRoot = "No languages root id set for input processor";
        private const string ErrorMessageInvalidTargetLanguage = "Unable to parse target language from id {0}";
        private const string ErrorMessageNoPageId = "No valid page id available for version creation";
        private const string ErrorMessageNoPageItem = "No page item found with id {0}";
        private const string ErrorMessageNoSourceVersion = "Page item with id {0} has no source version for language {1} to copy";

        public string LanguagesRootId { get; set; }

        /// <summary>
        /// Processes the posted inputs form the dialog and sets the SourceLanguage, TargetLanguages and the SourcePageItem.
        /// </summary>
        /// <param name="args">the pipeline args</param>
        public void Process(CopyPageToVersionsPipelineArgs args)
        {
            Log.Info("CopyPageToVersionsProcessor: started ...", this);
            args.Stopwatch.Start();

            Database database = Database.GetDatabase("master");

            if (database == null)
            {
                AbortPipeline(args, ErrorMessageDatabase);
                return;
            }

            // get source language
            Language sourceLanguage;
            if (!GetLanguageFromLangItem(args.SourceLanguageIdString, database, out sourceLanguage))
            {
                AbortPipeline(args, string.Format(ErrorMessageSourceLanguage, args.SourceLanguageIdString));
                return;
            }
            args.SourceLanguage = sourceLanguage;

            // get target languages
            if (string.IsNullOrEmpty(args.TargetLanguageIdStrings))
            {
                AbortPipeline(args, ErrorMessageNoTargetLanguages);
                return;
            }

            // check languages root id
            if (string.IsNullOrEmpty(LanguagesRootId) || !ID.IsID(LanguagesRootId))
            {
                AbortPipeline(args, ErrorMessageNoLanguagesRoot);
                return;
            }

            List<Language> targetLanguages = new List<Language>();

            if (args.TargetLanguageIdStrings.Equals(LanguagesRootId))
            {
                targetLanguages.AddRange(LanguageManager.GetLanguages(database));
            }
            else
            {
                // get all target languages from splitted languages item list
                List<string> splittedTargetLanguages = args.TargetLanguageIdStrings.Split('|').ToList();
                foreach (string splittedTargetLanguage in splittedTargetLanguages)
                {
                    if (!ID.IsID(splittedTargetLanguage))
                    {
                        SetWarnMessage(args, string.Format(ErrorMessageInvalidTargetLanguage, splittedTargetLanguage));
                        continue;
                    }

                    Language targetLanguage;
                    if (!GetLanguageFromLangItem(splittedTargetLanguage, database, out targetLanguage))
                    {
                        SetWarnMessage(args, string.Format(ErrorMessageInvalidTargetLanguage, splittedTargetLanguage));
                        continue;
                    }

                    targetLanguages.Add(targetLanguage);
                }
            }
            args.TargetLanguages.AddRange(targetLanguages);

            // get page item
            ID pageId;
            if (string.IsNullOrEmpty(args.PageId) || !ID.TryParse(args.PageId, out pageId))
            {
                AbortPipeline(args, ErrorMessageNoPageId);
                return;
            }

            Item pageItem = database.GetItem(pageId);

            if (pageItem == null)
            {
                AbortPipeline(args, ErrorMessageNoPageItem);
                return;
            }

            args.SourcePageItem = database.GetItem(pageId, sourceLanguage);

            if (args.SourcePageItem.Versions.Count > 0)
            {
                return;
            }

            AbortPipeline(args, string.Format(ErrorMessageNoSourceVersion, args.SourcePageItem.ID, sourceLanguage.Name));
        }

        private bool GetLanguageFromLangItem(string languageItemId, Database database, out Language language)
        {
            ID itemId;
            if (string.IsNullOrEmpty(languageItemId) || !ID.TryParse(languageItemId, out itemId))
            {
                language = null;
                return false;
            }

            Item sourceLangItem = database.GetItem(itemId);

            if (sourceLangItem == null)
            {
                language = null;
                return false;
            }

            string regionalIsoCode = GetFieldValue(sourceLangItem.Fields["Regional Iso Code"]);
            string iso = GetFieldValue(sourceLangItem.Fields["Iso"]);

            string languageIso = string.IsNullOrWhiteSpace(regionalIsoCode)
                ? iso
                : regionalIsoCode;

            return Language.TryParse(languageIso, out language);
        }

        private string GetFieldValue(Field field)
        {
            if (field == null || string.IsNullOrEmpty(field.Value))
            {
                return string.Empty;
            }
            return field.Value;
        }

        private void AbortPipeline(CopyPageToVersionsPipelineArgs args, string message)
        {
            Log.Error(ErrorMessagePrefix + message, this);
            args.ErrorMessages.Add(message);
            SetErrorResultMessage(args);
            args.Stopwatch.Stop();
            args.AbortPipeline();
        }

        private void SetWarnMessage(CopyPageToVersionsPipelineArgs args, string message)
        {
            Log.Warn(ErrorMessagePrefix + message, this);
            args.ErrorMessages.Add(message);
        }

        private static void SetErrorResultMessage(CopyPageToVersionsPipelineArgs args)
        {
            args.ResultMessage = "An Error occured: \r\n" + string.Join("\r\n", args.ErrorMessages);
        }
    }
}