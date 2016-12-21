define(["sitecore", "/-/speak/v1/ExperienceEditor/ExperienceEditor.js"], function (Sitecore, ExperienceEditor) {
    Sitecore.Commands.CopyPageToVersions =
    {
        canExecute: function (context) {
            return true;
            //return context.app.canExecute("ExperienceEditor.Publish.CanPublish", context.currentContext);
        },
        execute: function (context) {
            ExperienceEditor.modifiedHandling(true, function (isOk) {
                ExperienceEditor.PipelinesUtil.executePipeline(context.app.CopyPageToVersionsPipeline, function () {
                    ExperienceEditor.PipelinesUtil.executeProcessors(Sitecore.Pipelines.CopyPageToVersions, context);
                });
            });
        }
    };
});