# SitecoreCopyPageToVersions

## Management Summary

## Installation

## Configuration
The whitelisting of translatable templates is taken from the "Copy Page To Versions Configuration" under _/sitecore/system/Modules/Copy Page To Versions/Copy Page To Versions Configuration_.


![Image of CopyPageToVersions Configuration](Namics.Common.Packages.CopyPageToVersions/Files/Images/CopyPageToVersions_Configuration.PNG)


| Field Name  | Field Type  | Description  | 
|---|---|---|
| Filter Whitelisted Templates  | Checkbox  | Choose whether to filter/include the selected templates for the item creation, or not.  |
| Template Whitelist  | Treelist  | Only the selected templates are included for the version creation, if the "Filter Whitelisted Templates"-Checkbox is checked.  |

## Usage
A user is able to copy a page in a specific version to a selectable list of language versions form experience editor and content editor. Therefore open the Copy Page To Versions dialog via the provided buttons:
![Image of CopyPageToVersions button in Content Editor](Namics.Common.Packages.CopyPageToVersions/Files/Images/CopyPageToVersions_Content_Editor_Nav.PNG)
![Image of CopyPageToVersions button in Experience Editor](Namics.Common.Packages.CopyPageToVersions/Files/Images/CopyPageToVersions_Experience_Editor_Nav.PNG)
In the content editor, the button is only enabled, if the selected item is a content item and has a defined layout.


### Copy Page To Versions Dialog
![Image of CopyPageToVersions Dialog](Namics.Common.Packages.CopyPageToVersions/Files/Images/CopyPageToVersions_Dialog_Options.PNG)


| Dialog Input  | Description  |
|---|---|
| Create only versions without copying field values  | If this checkbox is selected, only the language versions of all related page items get created and the final renderings are merged to the target language versions. This can be useful, if you only want to create all page related item versions without field values, i.e. manual translation from English to German, where you don't want the english content on the german versions.  |
| Do not create versions for related items  | If checked, only the langage versions of the page item get created and the final renderings get copied to the target language versions. This can be useful, if you only want to create page versions and copy the final renderings.  |
| Force copy field values (this will overwrite already existing field values)  | If checked, all field values are getting overwritten with the source language field values. This means, if a version of a selected target language already exists, with different/modified field values, they got lost. Default behaviour is, that fields, with already exitsting content are not getting overwritten. Only use this flag, if you're absolutely sure that you'll overwrite existing versions with the field values from the source lanugage.  |
| Source Language  | Source Language of the translation. The current language of the page in the Experience Editor is selected per default. If you manually change it, make sure, the current page in this source language exists.  |
| Target Language  | Target Languages of the translation. If the Languages root is selected, the translation will be requested for all languages (except source language).   |
| Final Confirmation  | This flag needs to be set as final confirmation, that the Copy-Button gets enabled  |

## Dependent items and components


## Security
CopyPageToVersionsController checks if a user is authenticated and only runs the processor if current context is master. 

## Performance
The copy process runs in a pipeline context. To copy an average page to 10 languages takes about 500ms
Information about the elapsed time during a copy process can be viewed in the logfile, i.e. "CopyPageToVersionsProcessor: ended in 00:00:00.5277924"
