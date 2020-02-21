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

- Creating the resource of type Microsoft.BotService/botServices failed with status "BadRequest"

This happens when the Microsoft Azure application ID entered during the setup of the deployment has already been used and registered for a bot.

#### Fix

Either register a new Microsoft Azure AD application or delete the bot registration that is currently using the attempted Microsoft Azure application ID.

### **2. BOT unable to create more KBs and store the question**

#### Description

BOT will reply to the user post with a generic message “I cannot save this qna pair due to storage space limitations. Please contact your system administrator to provision additional storage space”

```
Errors: I cannot save this qna pair due to storage space limitations. Please contact your system administrator to provision additional storage space.
```

#### Fix

In case of such a scenario, system administrator can update the pricing tier accordingly for QnA service in Azure Portal.

### **3. Error while deploying the ARM Template**

#### Description

This happens when the resources are already created or due to some conflicts.
```

Errors: The resource operation completed with terminal provisioning state 'Failed'

```
#### Fix

In case of such a scenario, user needs to navigate to deployment center section of failed/conflict resources through the azure portal and check the error logs to get the actual errors and can fix it accordingly.

Redeploy it after fixing the issue/conflict.

### **Problems related to Messaging Extension**
### **1. SMEuser unable to see data in "Knowledge base" tab**

#### Description

This happens when the admin user does not update the knowledge base id in the configuration app or there is no qna pair in the knowledge base.

#### Fix

In case of such a scenario, admin user needs to make sure that the knowledge base id is updated in the configuration app and qna pairs are existing in the knowledge base.

If there is no qna pair exist in knowledge base then add the qna pair either directly from the qna maker portal or add it from the messaging extension section from SME team.

**Didn't find your problem here?**

Please, report the issue [here](https://github.com/OfficeDev/microsoft-teams-<<To Do>>/issues/new)