using System;
using System.Web.Http;
using Namics.Common.Packages.CopyPageToVersions.Models;
using Namics.Common.Packages.CopyPageToVersions.Pipelines;
using Newtonsoft.Json.Linq;
using Sitecore.Pipelines;

namespace Namics.Common.Packages.CopyPageToVersions.Controllers
{
    /// <summary>
    /// Provides an API to run the CopyPageToVersions pipeline by the posted speak request from the copy page to versions dialog.
    /// </summary>
    public class CopyPageToVersionsController : ApiController
    {
        /// <summary>
        /// Runs the CopyPageToVersions pipeline by the posted speak request from the copy page to versions dialog.
        ///  </summary>
        /// <param name="parameters">the client parameters from the posted body</param>
        /// <returns>a <see cref="T:System.Web.Http.Results.OkNegotiatedContentResult`1" /> with a json object including the result message of the processed pipeline</returns>
        [HttpPost]
        public IHttpActionResult CopyPageToVersion([FromBody] CopyPageToVersionsClientParameters parameters)
        {
            if (!Sitecore.Context.User.IsAuthenticated)
            {
                return Unauthorized();
            }

            if (!Sitecore.Context.ContentDatabase.Name.Equals("master", StringComparison.InvariantCultureIgnoreCase))
            {
                return Unauthorized();
            }

            var pipelineArgs = new CopyPageToVersionsPipelineArgs
            {
                IsDoNotCopyFieldValues = parameters.Option1CheckBox,
                IsDoNotCreateRelatedItemVersions = parameters.Option2CheckBox,
                IsForceCopyFieldValues = parameters.Option3CheckBox,
                PageId = parameters.PageId,
                SourceLanguageIdString = parameters.SourceLanguageTreeView,
                TargetLanguageIdStrings = parameters.TargetLanguageTreeView
            };

            CorePipeline.Run("copyPageToVersions", pipelineArgs);

            return Ok(new JObject(new JProperty("message", pipelineArgs.ResultMessage)));
        }
    }
}