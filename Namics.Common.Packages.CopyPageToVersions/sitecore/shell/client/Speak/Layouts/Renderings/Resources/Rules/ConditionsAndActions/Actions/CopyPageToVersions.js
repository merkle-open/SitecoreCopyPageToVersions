define(["sitecore", "jquery"], function (Sitecore, jQuery) {
    var action = function (context, args) {
        var option1CheckBox = context.app[args.option1CheckBox];
        var option2CheckBox = context.app[args.option2CheckBox];
        var option3CheckBox = context.app[args.option3CheckBox];
        var sourceLanguageTreeView = context.app[args.sourceLanguageTreeView];
        var targetLanguageTreeView = context.app[args.targetLanguageTreeView];
        var sourceLanguage = context.app[args.sourceLanguage];
        var pageId = context.app[args.pageId];

        var prepareValueForTransmit = function (component, targetProperty) {
            if (component) {
                return component.get(targetProperty);
            }
            return "";
        }

        var createMetaData = function () {
            return {
                option1CheckBox: prepareValueForTransmit(option1CheckBox, "isChecked"),
                option2CheckBox: prepareValueForTransmit(option2CheckBox, "isChecked"),
                option3CheckBox: prepareValueForTransmit(option3CheckBox, "isChecked"),
                sourceLanguageTreeView: prepareValueForTransmit(sourceLanguageTreeView, "selectedItemId"),
                targetLanguageTreeView: prepareValueForTransmit(targetLanguageTreeView, "checkedItemIds"),
                sourceLanguage: prepareValueForTransmit(sourceLanguage, "text"),
                pageId: prepareValueForTransmit(pageId, "text")
            };
        }

        var sendRequest = function (metadata) {
            jQuery.post("/sitecore/shell/api/copypagetoversions/copypagetoversion", metadata, null, "json").success(function (data) {
                alert(data.message);
            }).always(function () {
                context.app.closeDialog();
            });
        }

        var send = createMetaData();
        sendRequest(send);
    }
    return action;
});