define(["sitecore"], function (Sitecore) {
    var CopyPageToVersionsDialog = Sitecore.Definitions.App.extend({
        initialized: function () {

            this.SourceLanguageTreeView.on("change:selectedItemId",
                function() {
                    this.updateSourceLanguageTreeView();
                },
                this);

            this.TargetLanguageTreeView.on("change:checkedItemIds",
                function () {
                    this.updateTargetLanguageTreeView();
                },
                this);

            this.FinalConfirmationCheckBox.on("change:isChecked",
                function() {
                    this.updateCreateButton();
                },
                this);

            this.CreateTaskButton.on("click",
                function () {
                    this.CreateTaskButton.set("isEnabled", false);
                },
                this);
        },

        updateSourceLanguageTreeView: function() {
            var selectedSource = this.SourceLanguageTreeView.get("selectedItemId");

            if (selectedSource == null ||
                selectedSource === "" ||
                selectedSource === "{64C4F646-A3FA-4205-B98E-4DE2C609B60F}") {
                this.WarnMessageBar1.set("isVisible", true);
                return;
            } else {
                this.WarnMessageBar1.set("isVisible", false);
            }
        },

        updateTargetLanguageTreeView: function () {
            var checkedTargets = this.TargetLanguageTreeView.get("checkedItemIds");

            if (checkedTargets == null ||
                checkedTargets === "" ||
                checkedTargets === "{64C4F646-A3FA-4205-B98E-4DE2C609B60F}") {
                this.WarnMessageBar2.set("isVisible", true);
            } else {
                this.WarnMessageBar2.set("isVisible", false);
            }
        },

        updateCreateButton: function () {
            var isChecked = this.FinalConfirmationCheckBox.get("isChecked");
            var isWarning1 = this.WarnMessageBar1.get("isVisible");
            var isWarning2 = this.WarnMessageBar2.get("isVisible");

            if (isChecked && !isWarning1 && !isWarning2) {
                this.CreateTaskButton.set("isEnabled", true);
            } else {
                this.CreateTaskButton.set("isEnabled", false);
                if (isChecked) {
                    this.FinalConfirmationCheckBox.set("isChecked", false);
                }
            }
        }

    });

    return CopyPageToVersionsDialog;
});