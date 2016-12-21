define(["sitecore", "/-/speak/v1/ExperienceEditor/ExperienceEditor.js"], function (Sitecore, ExperienceEditor) {
    return {
        priority: 2,
        execute: function (context) {
            var dialogPath = "/sitecore/client/Applications/Dialogs/CopyPageToVersionsDialog?id=" + context.currentContext.itemId + "&lang=" + context.currentContext.language;
            var dialogFeatures = "dialogHeight: 600px;dialogWidth: 500px;";
            ExperienceEditor.Dialogs.showModalDialog(dialogPath, '', dialogFeatures);
        }
    };
});