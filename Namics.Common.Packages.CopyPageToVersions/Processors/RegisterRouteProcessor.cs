using Sitecore.Pipelines;
using System.Web.Http;

namespace Namics.Common.Packages.CopyPageToVersions.Processors
{
    /// <summary>
    /// Registeres the route for the copy page to versions api controller.
    /// </summary>
    public class RegisterRouteProcessor
    {
        /// <summary>
        /// Processes the pipeline, which registeres the route for the copy page to versions api controller:
        /// /api/copypagetoversions/copypagetoversion
        /// </summary>
        /// <param name="args"></param>
        public void Process(PipelineArgs args)
        {
            HttpConfiguration config = GlobalConfiguration.Configuration;

            config.Routes.MapHttpRoute("CopyPageToVerions",
                "sitecore/shell/api/{controller}/{action}/{id}",
                new {controller = "copypagetoversions", action = "copypagetoversion", id = RouteParameter.Optional});
        }
    }
}