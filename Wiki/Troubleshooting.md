# General template issues

### **Generic possible issues**

Certain issues can arise that are common to many of the app templates. Please check [here](https://github.com/OfficeDev/microsoft-teams-stickers-app/wiki/Troubleshooting) for reference to these.

### **Problems related to PowerShell script**

**1. File is not digitally signed**

While running PowerShell script, sometimes user gets an error showing 'File is not digitally signed'.

**Fix**: If this type of error occurs then run this: "Set-ExecutionPolicy -ExecutionPolicy unrestricted"

**2. Azure subscription access failed**

Connect-AzAccount : The provided account **.onmicrosoft.com does not have access to subscription ID "XXXX-". Please try logging in with different credentials or a different subscription ID.

**Fix**: User must be added as a contributor on the Azure subscription."


**3. Failed to acquire a token**

Exception calling "AcquireAccessToken" with "1" argument(s): "multiple_matching_tokens_detected: The cache contains multiple tokens satisfying the requirements

**Fix**: This means user is logged-in with multiple accounts in the current powershell session. Close the shell window and open a new one."


**4. Azure AD app permission consent error**

#### Description

![Screenshot of consent error](/Wiki/Images/ad-app-consent-error.png)

The apps created by this app template requires an admin consent for "User.Read" graph permission so it can operate correctly.

```
Errors: Forbidden({"ClassName":"Microsoft.Portal.Framework.Exceptions.ClientException","Message":"Graph call failed with httpCode=Forbidden, errorCode=Authorization_RequestDenied, errorMessage=This operation can only be performed by an administrator. Sign out and sign in as an administrator or contact one of your organization's administrators., reason=Forbidden

```

#### Fix

Please ask your tenant administrator to consent the "User.Read" permission for both apps (bot app, config app).

![Apps Admin Consent](/Wiki/Images/app-admin-consent.png)


**5. Error when attempting to reuse a Microsoft Azure AD application ID for the bot registration**

#### Description

The bot is not valid.

```
Errors: MsaAppId is already in use.
```

Creating the resource of type Microsoft.BotService/botServices failed with status "BadRequest"

This happens when the Microsoft Azure application ID entered during the setup of the deployment has already been used and registered for a bot.

#### Fix

Either register a new Microsoft Azure AD application or delete the bot registration that is currently using the attempted Microsoft Azure application ID.


**6. Error while deploying the ARM Template**

#### Description

This happens when the resources are already created or due to some conflicts.
```

Errors: The resource operation completed with terminal provisioning state 'Failed'

```
#### Fix

In case of such a scenario, the user needs to navigate to the deployment center section of failed/conflict resources through the Azure portal and check the error logs to get the actual errors and fix them accordingly.

Redeploy it after fixing the issue/conflict.
 
### **Problems deploying to Azure**

### **1. The bot is unable to create more KBs and store additional questions**

#### Description

The bot will reply to the user post with the error message if it finds that it cannot store any additional QnA pair to the Knowledge-base

```
Errors: I cannot save this qna pair due to storage space limitations. Please contact your system administrator to provide additional storage space.
```

#### Fix

In case of such a scenario, the system administrator or the app installer will need to update the pricing tier accordingly for QnA service in Azure Portal.

### **Problems related to App installation and manifest**
### **1. Manifest parsing has failed**

#### Description

This happens when the admin tries to install the app to Microsoft Teams using the zip file.

![Manifest parsing has failed](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/wiki/Images/manifest_parsing_failed.png)

#### Fix

In case of such a scenario, the admin user needs to click on the "Copy error details to clipboard" and check the error details.

If the error specifies as invalid value, then fix the invalid value and create the new zip folder to install the app.

If the error specifies as format issue, then fix the manifest format issue and create the new zip folder to install the app.

If the error specifies as related to folder structure then make sure the that the 3 files `manifest.json`,`color.png`, and `outline.png` are the top level of the ZIP package, with no nested folders

### **2. Config web app - User login/access issue**

#### Description

This happens when the user alias is not a part of the UPN admin list of the config web application.

![Screenshot of access issue](/Wiki/Images/user-not-in-upn-list.png)

#### Fix

Please add the user alias to the ConfigAdminUPNList parameter in parameters.json file. Append the user alias/email to the list using semi-colon `;` separator. e.g. adminuser@contoso.onmicrosoft.com;user2@contoso.onmicrosoft.com


### **Problems related to Messaging Extension**
### **1. Experts team members unable to see data in "Knowledgebase" tab**

#### Description

This happens when the admin user does not update the knowledge base id in the configuration app or if there are no QnA pairs in the knowledge base.

#### Fix

In case of such a scenario, the admin user needs to make sure that the knowledge base id is updated in the configuration app and QnA pairs are existing in the knowledge base.

If no QnA pairs exist in the knowledge base, then add new ones either directly from the QnA maker portal or add it from the messaging extension in the experts' team.

**Didn't find your problem here?**

Please, report the issue [here](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/issues/new)