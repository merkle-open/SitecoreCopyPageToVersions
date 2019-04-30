using System.Web.Http;
using System.Web.Routing;
using Sitecore.Pipelines;

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
        /// <param name="args">The initialize pipeline arguments</param>
        public void Process(PipelineArgs args)
        {
            GlobalConfiguration.Configure(RegisterRoutes);
        }

        private static void RegisterRoutes(HttpConfiguration config)
        {
            // Registeres the copy page to versions dialog action
            RouteTable.Routes.MapHttpRoute(
                "CopyPageToVerions",
                "sitecore/shell/api/{controller}/{action}/{id}",
                new { controller = "copypagetoversions", action = "copypagetoversion", id = RouteParameter.Optional }
            );
        }
    }
}