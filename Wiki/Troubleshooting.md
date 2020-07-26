# General template issues

### **Generic possible issues**

There are certain issues that can arise that are common to many of the app templates. Please check [here](https://github.com/OfficeDev/microsoft-teams-stickers-app/wiki/Troubleshooting) for reference to these.
  
### **Problems deploying to Azure**

### **1. Error when attempting to reuse a Microsoft Azure AD application ID for the bot registration**

#### Description

Bot is not valid.

```
Errors: MsaAppId is already in use.
```

Creating the resource of type Microsoft.BotService/botServices failed with status "BadRequest"

This happens when the Microsoft Azure application ID entered during the setup of the deployment has already been used and registered for a bot.

#### Fix

Either register a new Microsoft Azure AD application or delete the bot registration that is currently using the attempted Microsoft Azure application ID.

### **2. Bot is unable to create more KBs and store additional questions**

#### Description

Bot will reply to the user post with the error message if it finds that it cannot store any additional QnA pair to the Knowledge base

```
Errors: I cannot save this qna pair due to storage space limitations. Please contact your system administrator to provision additional storage space.
```

#### Fix

In case of such a scenario, system administrator or the app installer will need to update the pricing tier accordingly for QnA service in Azure Portal.

### **3. Error while deploying the ARM Template**

#### Description

This happens when the resources are already created or due to some conflicts.
```

Errors: The resource operation completed with terminal provisioning state 'Failed'

```
#### Fix

In case of such a scenario, user needs to navigate to deployment center section of failed/conflict resources through the azure portal and check the error logs to get the actual errors and fix it accordingly.

Redeploy it after fixing the issue/conflict.

### **Problems related to Messaging Extension**
### **1. Experts team members unable to see data in "Knowledge base" tab**

#### Description

This happens when the admin user does not update the knowledge base id in the configuration app or if there are no QnA pairs in the knowledge base.

#### Fix

In case of such a scenario, admin user needs to make sure that the knowledge base id is updated in the configuration app and QnA pairs are existing in the knowledge base.

If  no QnA pairs exist in knowledge base then add new ones either directly from the QnA maker portal or add it from the messaging extension in the experts team.

**Didn't find your problem here?**

Please, report the issue [here](https://github.com/OfficeDev/microsoft-teams-apps-faqplusv2/issues/new)