# General template issues

### **Generic possible issues**

Certain issues can arise that are common to many of the app templates. Please check [here](https://github.com/OfficeDev/microsoft-teams-stickers-app/wiki/Troubleshooting) for reference to these.
  
### **Problems deploying to Azure**

### **1. Error when attempting to reuse a Microsoft Azure AD application ID for the bot registration**

#### Description

The bot is not valid.

```
Errors: MsaAppId is already in use.
```

Creating the resource of type Microsoft.BotService/botServices failed with status "BadRequest"

This happens when the Microsoft Azure application ID entered during the setup of the deployment has already been used and registered for a bot.

#### Fix

Either register a new Microsoft Azure AD application or delete the bot registration that is currently using the attempted Microsoft Azure application ID.

### **2. The bot is unable to create more KBs and store additional questions**

#### Description

The bot will reply to the user post with the error message if it finds that it cannot store any additional QnA pair to the Knowledgebase

```
Errors: I cannot save this qna pair due to storage space limitations. Please contact your system administrator to provide additional storage space.
```

#### Fix

In case of such a scenario, the system administrator or the app installer will need to update the pricing tier accordingly for QnA service in Azure Portal.

### **3. Error while deploying the ARM Template**

#### Description

This happens when the resources are already created or due to some conflicts.
```

Errors: The resource operation completed with terminal provisioning state 'Failed'

```
#### Fix

In case of such a scenario, the user needs to navigate to the deployment center section of failed/conflict resources through the Azure portal and check the error logs to get the actual errors and fix them accordingly.

Redeploy it after fixing the issue/conflict.

### **Problems related to App installation and manifest**
### **1. Manifest parsing has failed**

#### Description

This happens when the admin tries to install the app to Microsoft Teams using the zip file.

![Manifest parsing has failed](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/wiki/Images/manifest_parsing_failed.png)

#### Fix

In case of such a scenario, the admin user needs to click on the "Copy error details to clipboard" and check the error details.

If the error specifies as invalid value then fix the invalid value and create the new zip folder to install the app.

If the error specifies as format issue then fix the manifest format issue and create the new zip folder to install the app.

If the error specifies as related to folder structure then make sure the that the 3 files `manifest.json`,`color.png`, and `outline.png` are the top level of the ZIP package, with no nested folders

### **Problems related to Messaging Extension**
### **1. Experts team members unable to see data in "Knowledgebase" tab**

#### Description

This happens when the admin user does not update the knowledge base id in the configuration app or if there are no QnA pairs in the knowledge base.

#### Fix

In case of such a scenario, the admin user needs to make sure that the knowledge base id is updated in the configuration app and QnA pairs are existing in the knowledge base.

If no QnA pairs exist in the knowledge base then add new ones either directly from the QnA maker portal or add it from the messaging extension in the experts' team.

**Didn't find your problem here?**

Please, report the issue [here](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/issues/new)