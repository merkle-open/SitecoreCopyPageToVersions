using System.Collections.Generic;
using System.Linq;
using Namics.Common.Packages.CopyPageToVersions.Pipelines;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Layouts;

namespace Namics.Common.Packages.CopyPageToVersions.Processors
{
    /// <summary>
    /// Processes the item aggregation for the source page item and all datasources defined on the renderings.
    /// </summary>
    public class PageItemsAggregationProcessor
    {
        private const string ErrorMessagePrefix = "PageItemsAggregationProcessor: ";
        private const string ErrorMessageNoServiceConfiguration = "Service configuration is not available";

        public string CopyPageToVersionsConfigurationPath { get; set; }

        /// <summary>
        /// Aggregates all related items for the copy process, depending on the configured template whitelist
        /// </summary>
        /// <param name="args">the pipeline args</param>
        public void Process(CopyPageToVersionsPipelineArgs args)
        {
            Item configuration = args.SourcePageItem.Database.GetItem(CopyPageToVersionsConfigurationPath);

            if (configuration == null)
            {
                AbortPipeline(args, ErrorMessageNoServiceConfiguration);
                args.Stopwatch.Stop();
                return;
            }

            List<ID> templateIdWhitelist = GetIdListFromField(configuration.Fields["Template Whitelist"]);
            bool isFilterWhitelistedTemplates = GetCheckboxStateFromField(configuration.Fields["Filter Whitelisted Templates"]);

            List<Item> relatedItems = GetRelatedItems(args.SourcePageItem, args.SourceLanguage, templateIdWhitelist,
                isFilterWhitelistedTemplates);

            args.RelatedItems.AddRange(relatedItems);
        }

        private List<ID> GetIdListFromField(Field field)
        {
            MultilistField multilistField = field;
            if (multilistField == null)
            {
                return new List<ID>();
            }
            return multilistField.GetItems().Select(i => i.ID).ToList();
        }

        private static bool GetCheckboxStateFromField(Field field)
        {
            CheckboxField checkboxField = field;
            return checkboxField != null && checkboxField.Checked;
        }

        public List<Item> GetRelatedItems(Item pageItem, Language language, IList<ID> allowedTemplateIds, bool filterAllowedTemplates)
        {
            var itemList = new List<Item>();

            if (pageItem == null)
            {
                return itemList;
            }

            var langItem = pageItem.Database.GetItem(pageItem.ID, language);

            if (langItem != null)
            {
                itemList.Add(pageItem);
            }

            if (filterAllowedTemplates && allowedTemplateIds != null && !allowedTemplateIds.Any())
            {
                return itemList;
            }

            var defaultDevice = pageItem.Database.Resources.Devices.GetAll().First(d => d.IsDefault);

            var renderings = pageItem.Visualization.GetRenderings(defaultDevice, false).Where(IncludeRendering);

            foreach (var rendering in renderings)
            {
                var item = pageItem.Database.GetItem(rendering.Settings.DataSource, pageItem.Language);

                if (item != null)
                {
                    GetSubItems(item, itemList, language, allowedTemplateIds, filterAllowedTemplates);
                }
            }

            return itemList;
        }

        private void GetSubItems(Item item, ICollection<Item> list, Language language, IList<ID> allowedTemplateIds, bool filterAllowedTemplates)
        {
            if (item.HasChildren)
            {
                AddItemToList(item, list, language, allowedTemplateIds, filterAllowedTemplates);

                foreach (var child in item.Children)
                {
                    GetSubItems((Item)child, list, language, allowedTemplateIds, filterAllowedTemplates);
                }
            }
            else
            {
                AddItemToList(item, list, language, allowedTemplateIds, filterAllowedTemplates);
            }
        }

        private void AddItemToList(Item item, ICollection<Item> list, Language language, IList<ID> allowedTemplateIds, bool filterAllowedTemplates)
        {
            if (!IncludeItem(item, allowedTemplateIds, filterAllowedTemplates))
            {
                return;
            }

            var langItem = item.Database.GetItem(item.ID, language);
            if (langItem.Versions.Count > 0)
            {
                list.Add(item);
            }
        }

        private static bool IncludeRendering(RenderingReference rendering)
        {
            if (rendering == null)
            {
                return false;
            }

            return !string.IsNullOrEmpty(rendering.Settings.DataSource);
        }

        private bool IncludeItem(Item item, IList<ID> allowedTemplateIds, bool filterAllowedTemplates)
        {
            return !filterAllowedTemplates ||
                   (item != null &&
                    allowedTemplateIds.Contains(item.TemplateID) &&
                    !item.Paths.LongID.Contains(ItemIDs.LayoutRoot.ToString()));
        }

        private void AbortPipeline(CopyPageToVersionsPipelineArgs args, string message)
        {
            Log.Error(ErrorMessagePrefix + message, this);
            args.ErrorMessages.Add(message);
            SetErrorResultMessage(args);
            args.Stopwatch.Stop();
            args.AbortPipeline();
        }

        private static void SetErrorResultMessage(CopyPageToVersionsPipelineArgs args)
        {
            args.ResultMessage = "An Error occured: \r\n" + string.Join("\r\n", args.ErrorMessages);
        }
    }
}