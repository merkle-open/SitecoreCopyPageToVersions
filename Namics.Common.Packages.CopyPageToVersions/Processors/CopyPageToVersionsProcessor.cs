using System;
using System.Collections.Generic;
using System.Linq;
using Namics.Common.Packages.CopyPageToVersions.Pipelines;
using Sitecore;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Layouts;

namespace Namics.Common.Packages.CopyPageToVersions.Processors
{
    /// <summary>
    /// Copies the source page item version to the target languages.
    /// </summary>
    public class CopyPageToVersionsProcessor
    {
        private const string ErrorMessagePrefix = "CopyPageToVersionsProcessor: ";
        private const string ErrorMessageNoTargetVersion = "Page item with id {0} has no target version for language {1}";
        private const string ErrorMessageNoRenderingField = "Final rendering field not available on source- or final-page item";
        private const string WarnMessageSourceEqualsTarget = "Source language version {0} is equals target language version {1}. Skipping copy process for this language.";

        /// <summary>
        /// Creates a page version for each target language and creates or updates the final renderings.
        /// </summary>
        /// <param name="args">the pipeline args</param>
        public void Process(CopyPageToVersionsPipelineArgs args)
        {
            List<string> targetLanguagesStrings = new List<string>();
            foreach (Language targetLanguage in args.TargetLanguages)
            {
                if (args.SourceLanguage.Equals(targetLanguage))
                {
                    SetWarnMessage(args, string.Format(WarnMessageSourceEqualsTarget, args.SourceLanguage.Name, targetLanguage.Name));
                    continue;
                }

                var targetPageItem = GetOrCreateItemLanguageVersion(args.SourcePageItem, targetLanguage, !args.IsDoNotCopyFieldValues, args.IsForceCopyFieldValues);
                if (targetPageItem == null)
                {
                    SetWarnMessage(args, string.Format(ErrorMessageNoTargetVersion, args.SourcePageItem.ID, targetLanguage));
                    continue;
                }

                CreateOrUpdateFinalRenderings(args.SourcePageItem, targetPageItem, args);

                if (!args.IsDoNotCreateRelatedItemVersions)
                {
                    foreach (Item relatedItem in args.RelatedItems)
                    {
                        GetOrCreateItemLanguageVersion(relatedItem, targetLanguage, !args.IsDoNotCopyFieldValues, args.IsForceCopyFieldValues);
                    }
                }

                targetLanguagesStrings.Add(targetLanguage.Name);
            }

            SetSuccessMessage(args, targetLanguagesStrings, args.SourceLanguage.Name);
            Log.Info(string.Format("{0}copied page language version {1} to language versions {2}", ErrorMessagePrefix, args.SourceLanguage.Name, string.Join(", ", targetLanguagesStrings)), this);
            args.Stopwatch.Stop();
            Log.Info(string.Format("{0}ended in {1}", ErrorMessagePrefix, args.Stopwatch.Elapsed), this);
        }

        private Item GetOrCreateItemLanguageVersion(Item sourceItem, Language targetLanguage, bool copyFieldValues, bool forceCopy)
        {
            if (sourceItem == null || targetLanguage == null)
            {
                return null;
            }

            Item targetItem = sourceItem.Database.GetItem(sourceItem.ID, targetLanguage);

            if (targetItem == null)
            {
                return null;
            }

            if (!targetItem.IsFallback && targetItem.Versions.Count > 0)
            {
                if (copyFieldValues)
                {
                    CopyFieldValues(sourceItem, targetItem, forceCopy);
                }
                return targetItem;
            }

            using (new EditContext(targetItem))
            {
                var localizedItemVersion = targetItem.Versions.AddVersion();
                if (copyFieldValues)
                {
                    CopyFieldValues(sourceItem, localizedItemVersion, forceCopy);
                }
                return localizedItemVersion;
            }
        }

        private void CopyFieldValues(Item source, Item target, bool forceCopy)
        {
            if (source == null || target == null)
            {
                return;
            }
            // copy all non standard fields for this language version
            var sourceFields = source.Fields;
            sourceFields.ReadAll();

            var targetFields = target.Fields;
            targetFields.ReadAll();

            foreach (Field field in targetFields)
            {
                if (field.Shared || string.IsNullOrEmpty(field.Name) ||
                    field.Name.StartsWith("__", StringComparison.InvariantCultureIgnoreCase) ||
                    (!string.IsNullOrEmpty(field.Value) && !forceCopy)) // skip copy of field value to target, if target field already has a value and force copy isn't selected
                {
                    continue;
                }
                var sourcefield = sourceFields[field.ID];
                if (sourcefield == null)
                {
                    continue;
                }
                using (new EditContext(target))
                {
                    field.Value = sourcefield.Value;
                }
            }
        }

        private void CreateOrUpdateFinalRenderings(Item sourcePageItem, Item targetPageItem, CopyPageToVersionsPipelineArgs args)
        {
            if (sourcePageItem == null || targetPageItem == null)
            {
                return;
            }

            var sourceFinalRenderingsField = sourcePageItem.Fields[FieldIDs.FinalLayoutField];
            var targetFinalRenderingsField = targetPageItem.Fields[FieldIDs.FinalLayoutField];
            if (sourceFinalRenderingsField == null || targetFinalRenderingsField == null)
            {
                SetWarnMessage(args, ErrorMessageNoRenderingField);
                return;
            }
            if (string.IsNullOrWhiteSpace(targetFinalRenderingsField.Value))
            {
                CreateFinalRenderings(targetPageItem, targetFinalRenderingsField, sourceFinalRenderingsField);
            }
            else
            {
                UpdateFinalRenderings(sourcePageItem, targetPageItem, sourceFinalRenderingsField, targetFinalRenderingsField);
            }
        }

        private static void UpdateFinalRenderings(Item sourcePageItem, Item targetPageItem, Field sourceFinalRenderingsField, Field targetFinalRenderingsField)
        {
            // get source renderings
            var sourceDefaultDevice = sourcePageItem.Database.Resources.Devices.GetAll().First(d => d.IsDefault);
            var sourceLayoutField = new LayoutField(sourceFinalRenderingsField);
            var sourceLayoutDefinition = LayoutDefinition.Parse(sourceLayoutField.Value);
            var sourceDeviceDefinition = sourceLayoutDefinition.GetDevice(sourceDefaultDevice.ID.ToString());

            // get target renderings
            var targetDefaultDevice = targetPageItem.Database.Resources.Devices.GetAll().First(d => d.IsDefault);
            var targetLayoutField = new LayoutField(targetFinalRenderingsField);
            var targetLayoutDefinition = LayoutDefinition.Parse(targetLayoutField.Value);
            var targetDeviceDefinition = targetLayoutDefinition.GetDevice(targetDefaultDevice.ID.ToString());

            foreach (RenderingDefinition sourceRendering in sourceDeviceDefinition.Renderings)
            {
                var duplicateRendering =
                    targetDeviceDefinition.Renderings.Cast<RenderingDefinition>()
                        .FirstOrDefault(r => r.UniqueId.Equals(sourceRendering.UniqueId));

                // Rendering isn't already on target renderings
                if (duplicateRendering == null)
                {
                    targetDeviceDefinition.Renderings.Add(sourceRendering);
                }
            }

            // save changes
            using (new EditContext(targetPageItem))
            {
                targetFinalRenderingsField.Value = targetLayoutDefinition.ToXml();
            }
        }

        private static void CreateFinalRenderings(Item targetPageItem, Field targetFinalRenderingsField, Field sourceFinalRenderingsField)
        {
            using (new EditContext(targetPageItem))
            {
                targetFinalRenderingsField.Value = sourceFinalRenderingsField.Value;
            }
        }

        private void SetWarnMessage(CopyPageToVersionsPipelineArgs args, string message)
        {
            Log.Warn(ErrorMessagePrefix + message, this);
            args.ErrorMessages.Add(message);
        }

        private static void SetSuccessMessage(CopyPageToVersionsPipelineArgs args, List<string> targetLanguagesStrings, string sourceLanguage)
        {
            if (args.ErrorMessages.Count > 0)
            {
                args.ResultMessage = string.Format("Successfully copied page language versoin {0} to versions {1} with errors: {2} \r\n", sourceLanguage, string.Join(", ", targetLanguagesStrings), string.Join("\r\n", args.ErrorMessages));
            }
            args.ResultMessage = string.Format("Successfully copied page language versoin {0} to versions {1}", sourceLanguage, string.Join(", ", targetLanguagesStrings));
        }
    }
}