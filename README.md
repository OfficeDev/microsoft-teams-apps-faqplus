---
page_type: sample
languages:
- csharp
products:
- office-teams
description: FAQ Plus bot is a friendly Q&A bot that brings a human in the loop when it is unable to help. One can ask the bot a question and the bot responds with an answer if it's in the knowledge base. If not, the bot allows the end-user to submit a query which then gets posted in a pre-configured team of experts to enable the experts to provide support by acting upon the notifications from within their team itself. 
urlFragment: microsoft-teams-apps-faqplusv2
---
#  FAQ Plus [Version 2] App Template

| [Documentation](/wiki/Home.md) | [Deployment guide](/wiki/Deployment-Guide.md) | [Architecture](/wiki/Solution-Overview.md) |
| ---- | ---- | ---- |

Chatbots are an easy way to provide answers to frequently asked questions by end-users. However, most chatbots fail to engage with end-users in a meaningful way because there is no human in the loop when the chatbot fails. 

FAQ Plus bot is a friendly Q&A bot that brings a human in the loop when it is unable to help. One can ask the bot a question and the bot responds with an answer if it's in the knowledge base. If not, the bot allows the end-user to submit a query which then gets posted in a pre-configured team of experts to provide support by acting upon the notifications from within their team itself.

**FAQ Plus has following features:**

*	An Enduser interacting with FAQ Plus:

![FAQ Plus in action (user view)](https://github.com/OfficeDev/microsoft-teams-apps-faqplusv2/wiki/Images/FAQPlusEndUser.gif)

*	Experts team using FAQ Plus:

![FAQ Plus in action (experts view)](https://github.com/OfficeDev/microsoft-teams-apps-faqplusv2/wiki/Images/FAQPlusExperts.gif)


**FAQ Plus [Version 2] adds the following new capabilities on top of existing features**

Members of the experts team will now be able to
- Added a new QnA pair to the Knowledge base directly using the Messaging extension
- Perform CRUD operations on the QnA pairs added through the bot
- View names of individuals who updated a QnA pair
- Configure the answer to show up as a adaptive card with additional details like Title, Subtitle, Image etc instead of the usual text based answer

## **Legal Notices**

Please read the license terms applicable to this template [here](https://github.com/OfficeDev/microsoft-teams-apps-faqplusv2/blob/master/LICENSE). In addition to these terms, you agree to the following:

 - You are responsible for complying with privacy and security regulations applicable to your app.
 
 - Use, collection, and handling of any personal data by your app is your responsibility alone.  Microsoft will not have any access to data collected through your app.  Microsoft will not responsible for any data related incidents or data subject requests.
 
 - If your app is developed to be sideloaded internally within your organization, you agree to comply with all internal privacy and security policies of your organization.
 
 - Use of this template does not guarantee acceptance of your app to Teams app store.  To make this app available in the Teams app store, you will have to comply with [submission process and validation](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/deploy-and-publish/appsource/publish), and all associated requirements such as including your own privacy statement and terms of use for your app.
 
 - Any Microsoft trademarks and logos included in this repository are property of Microsoft and should not be reused, redistributed, modified, repurposed, or otherwise altered or used outside of this repository.

## **Getting** **Started**

Begin with the [Solution overview](/wiki/Solution-overview) to read about what the app does and how it works.

When you're ready to try out FAQ Plus [Version 2], or to use it in your own organization, follow the steps in the [Deployment guide](/wiki/DeployementGuide).

## **Feedback**

Thoughts? Questions? Ideas? Share them with us on [Teams UserVoice](https://microsoftteams.uservoice.com/forums/555103-public)!

Please report bugs and other code issues [here](https://github.com/OfficeDev/microsoft-teams-apps-faqplusv2/issues/new)<TODO> (Page Not Found).

## **Contributing**

This project welcomes contributions and suggestions. Most contributions require you to agree to a Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us the rights to use your contribution. For details, visit [https://cla.microsoft.com](https://cla.microsoft.com/).

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/FAQ/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
