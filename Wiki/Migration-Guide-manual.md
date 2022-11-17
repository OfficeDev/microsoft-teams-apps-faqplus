**Note**: Migration approach to v5.0(lastest) is not yet finalized. This guide helps in migrating the older versions of FAQ+ (version < 4.0.0) to version = 4.0.0.

FAQ v4.0 uses two bots - one for end user and the other for SME team.

## Step 1. Create a new Azure AD app for Expert bot.
Register an Azure AD applications in your tenant's directory: the Expert bot app.

1. Log in to the Azure Portal for your subscription and go to the "App registrations" blade [here](https://portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/RegisteredApps).

2. Click on "New registration", and create an Azure AD application.
	1. **Name**: The name of your Expert's app - if your default bot name for older deployment is FAQ Plus, name the expert bot as "FAQ Plus Expert".
	2. **Supported account types**: Select "Accounts in any organizational directory".
	3. Leave the "Redirect URL" field blank.

![Azure registration page](https://github.com/v-royavinash/microsoft-teams-apps-faqplus/wiki/Images/migration_multitenant_app_creation.png)

3. Click on the "Register" button.

4. When the app is registered, you'll be taken to the app's "Overview" page. Copy the **Application (client) ID** and **Directory (tenant) ID**; we will need it later. Verify that the "Supported account types" is set to **Multiple organizations**.

![Azure overview page](https://github.com/v-royavinash/microsoft-teams-apps-faqplus/wiki/Images/migration_multitenant_app_overview.png)

5. On the side rail in the Manage section, navigate to the "Certificates & secrets" section. In the Client secrets section, click on "+ New client secret". Add a description of the secret and select an expiry time. Click "Add".

![Azure AD overview page](https://github.com/v-royavinash/microsoft-teams-apps-faqplus/wiki/Images/multitenant_app_secret.png)

6. Once the client secret is created, copy its **Value**; we will need it later.

7. At this point you have two values.
* Application (client) ID for the expert bot.
* Client secret for the expert bot.

## Step 2. Create a new Azure Bot for Expert app.

1. Log in to the Azure portal for your subscription, search for "Azure Bot", and create one [here](https://ms.portal.azure.com/#create/Microsoft.AzureBot).

2. Fill the following fields -
* **Bot handle**: Name for Azure  bot.
* **Subscription**: Your existing Azure subscription.
* **Resource group**: Existing resource group where other resources are deployed.
* **Pricing tier**: Select the appropriate pricing tier.
* **Microsoft App ID**: Choose "Use existing app registration". Enter the app id and password of above expert app.

![Azure Bot overview page](https://github.com/v-royavinash/microsoft-teams-apps-faqplus/wiki/Images/migration_create_bot.png)

3. Add Teams channel to the bot.
![Azure Bot channel page](https://github.com/v-royavinash/microsoft-teams-apps-faqplus/wiki/Images/migration_add_channel.png)

4. Add messaging endpoint for the bot, e.g. `https://<<appDomain>>/api/messages/expert`
![Azure Bot configuration page](https://github.com/v-royavinash/microsoft-teams-apps-faqplus/wiki/Images/migration_expert_endpoint.png)

## Step 3. Update the exixting Azure Bot.
The existing Azure bot in the already deloyed resource group would act as the end user bot.

1. Update the messaging endpoint of the user bot. Append `user` to the end, e.g. e.g. `https://<<appDomain>>/api/messages/user`
![Azure Bot configuration page](https://github.com/v-royavinash/microsoft-teams-apps-faqplus/wiki/Images/migration_endpoint_user.png)

## Step 4. Update the App Service Configuration for FAQ+ app.
Go to Azure App Service for FAQ+ app. Click on "Configuration" and update the following:

1. Rename "MicrosoftAppId" to **"UserAppId"**.
2. Rename "MicrosoftAppPassword" to **"UserAppPassword"**.
3. Click "New application setting" and add **"ExpertAppId"** as expert app id from step 1.
4. Click "New application setting" and add **"ExpertAppPassword"** as expert app secret from step 1.

![Azure App Service configuration page](https://github.com/v-royavinash/microsoft-teams-apps-faqplus/wiki/Images/migration_update_configuration.png)

## Step 5. Update App service code.
Follow [Continuous Deployment](https://github.com/v-royavinash/microsoft-teams-apps-faqplus/wiki/Continuous-Deployment) for updating the app service code.

## Step 6. Generate manifest.
Create three Teams app packages: one for end-users to install personally, one to be installed to the experts' team, and one the supports legacy code.

1. Open the `Manifest\EndUser\manifest_enduser.json` file in a text editor.

2. Change the placeholder fields in the manifest to values appropriate for your organization.

* `developer.name` ([What's this?](https://docs.microsoft.com/en-us/microsoftteams/platform/resources/schema/manifest-schema#developer))

* `developer.websiteUrl`

* `developer.privacyUrl`

* `developer.termsOfUseUrl`

3. Replace all the occurrences of `<<userBotId>>` placeholder to your Azure AD end user application's ID from above. This is the same GUID that you entered in the template under "User Bot Client ID".

4. In the "validDomains" section, replace all the occurrences of `<<appDomain>>` with your Bot App Service's domain. This will be `[BaseResourceName].azurewebsites.net`. For example, if you chose "contosofaqplus" as the base name, change the placeholder to `contosofaqplus.azurewebsites.net`.

5. Save and Rename `manifest_enduser.json` file to a file named `manifest.json`.

6. Create a ZIP package with the all the files in `Manifest\EndUser` folder - `manifest.json`,`color.png` and `outline.png`, along with localization files - `ar.json`, `de.json`, `en.json`, `es.json`, `fr.json`, `he.json`, `ja.json`, `ko.json`, `pt-BR.json`, `ru.json`, `zh-CN.json`, `zh-TW.json`. The two image files are the icons for your app in Teams.
* Name this package `faqplus-enduser.zip`, so you know that this is the app for end-users.
* Make sure that the 15 files are the _top level_ of the ZIP package, with no nested folders.
![File Explorer](https://github.com/v-royavinash/microsoft-teams-apps-faqplus/wiki/Images/file-explorer-user.png)

7. Rename the `manifest.json` file to `manifest_enduser.json` for reusing the file.

8.  Open the `Manifest\SME\manifest_sme.json` file in a text editor.

9. Repeat the steps from 2 to 4 to replace all the placeholders in the file. The placeholder `<<expertBotId>>` should be replaced by your Azure AD expert application's ID from above.

10. Save and Rename `manifest_sme.json` file to a file named `manifest.json`.

11. Create a ZIP package with the all the files in `Manifest\SME` folder (except manifest_legacy) - `manifest.json`,`color.png` and `outline.png`, along with localization files - `ar.json`, `de.json`, `en.json`, `es.json`, `fr.json`, `he.json`, `ja.json`, `ko.json`, `pt-BR.json`, `ru.json`, `zh-CN.json`, `zh-TW.json`. The two image files are the icons for your app in Teams.
* Name this package `faqplus-sme.zip`, so you know that this is the app for sme.
* Make sure that the 15 files are the _top level_ of the ZIP package, with no nested folders.
![File Explorer](https://github.com/v-royavinash/microsoft-teams-apps-faqplus/wiki/Images/file-explorer-sme.png)

12. Rename the `manifest.json` file to `manifest_sme.json` for reusing the file.

13. To support legacy code, open `Manifest\SME\manifest_legacy.json` in text editor.

14. Repeat the steps from 2 to 4 to replace all the placeholders in the file. The placeholder `<<userBotId>>` should be replaced by your Azure AD end user application's ID from above.

15. Create a ZIP package with the all the files in `Manifest\SME` folder (except manifest_sme) - `manifest.json`,`color.png` and `outline.png`, along with localization files - `ar.json`, `de.json`, `en.json`, `es.json`, `fr.json`, `he.json`, `ja.json`, `ko.json`, `pt-BR.json`, `ru.json`, `zh-CN.json`, `zh-TW.json`. The two image files are the icons for your app in Teams.
* Name this package `faqplus-legacy.zip`, so you know that this is the app for sme.
* Make sure that the 15 files are the _top level_ of the ZIP package, with no nested folders.
![File Explorer](https://github.com/v-royavinash/microsoft-teams-apps-faqplus/wiki/Images/file-explorer-legacy.png)

12. Rename the `manifest.json` file to `manifest_legacy.json` for reusing the file.

**Note**: Please re-install all the three packages to make the new and legacy code working. The legacy app and sme app packages are to be installed in Expert's team. The end user app package is to be installed for 1:1 chat with end user. The legacy app handles the requests for the existing ticket cards in Expert's team whereas the new sme app handles all the fresh requests in Expert's team.