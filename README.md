---
page_type: sample
languages:
- csharp
products:
- office-teams
description: FAQ Plus bot is a friendly Q&A bot that brings a human in the loop when it is unable to help with an answer from the knowledge base.
urlFragment: microsoft-teams-apps-faqplusv2
---
#  FAQ Plus [Version 2] App Template

| [Documentation](https://github.com/OfficeDev/microsoft-teams-apps-faqplusv2/wiki/Home) | [Deployment guide](https://github.com/OfficeDev/microsoft-teams-apps-faqplusv2/wiki/Deployment-Guide) | [Architecture](https://github.com/OfficeDev/microsoft-teams-apps-faqplusv2/wiki/Solution-Overview) |
| ---- | ---- | ---- |

Chatbots are an easy way to provide answers to frequently asked questions by users. However, most chatbots fail to engage with users in a meaningful way because there is no human in the loop when the chatbot fails.  

FAQ Plus bot is a friendly Q&A bot that brings a human in the loop when it is unable to help. One can ask the bot a question and the bot responds with an answer if it's in the knowledge base. If not, the bot allows the user to submit a query which then gets posted in a pre-configured team of experts who are help to provide support by acting upon the notifications from within their Team itself. 

**FAQ Plus has following features:**

An end-user interacting with FAQ Plus:

![FAQ Plus in action (user view)](https://github.com/OfficeDev/microsoft-teams-apps-faqplusv2/wiki/Images/FAQPlusEndUser.gif)

Experts team using FAQ Plus:

![FAQ Plus in action (experts view)](https://github.com/OfficeDev/microsoft-teams-apps-faqplusv2/wiki/Images/FAQPlusExperts.gif)


### FAQ Plus [Version 2] adds the following new capabilities

Members of the experts team will now be able to
- add a new QnA pair to the knowledge base directly using messaging extension
- edit and delete QnA pairs added through the bot
- track history of experts updating QnA pair
- configure the answer to show up as an adaptive card with additional details like title, subtitle, image, etc. instead of usual text based answer

## Legal Notice
This app template is provided under the [MIT License](https://github.com/OfficeDev/microsoft-teams-apps-faqplusv2/blob/master/LICENSE) terms.  In addition to these terms, by using this app template you agree to the following:

-	You are responsible for complying with all applicable privacy and security regulations related to use, collection and handling of any personal data by your app.  This includes complying with all internal privacy and security policies of your organization if your app is developed to be sideloaded internally within your organization.

-	Where applicable, you may be responsible for data related incidents or data subject requests for data collected through your app.

-	Any trademarks or registered trademarks of Microsoft in the United States and/or other countries and logos included in this repository are the property of Microsoft, and the license for this project does not grant you rights to use any Microsoft names, logos or trademarks outside of this repository.  Microsoft’s general trademark guidelines can be found [here](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general.aspx).

-	Use of this template does not guarantee acceptance of your app to the Teams app store.  To make this app available in the Teams app store, you will have to comply with the [submission and validation process](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/deploy-and-publish/appsource/publish), and all associated requirements such as including your own privacy statement and terms of use for your app.


## Getting started

Begin with the [Solution overview](https://github.com/OfficeDev/microsoft-teams-apps-faqplusv2/wiki/Solution-overview) to read about what the app does and how it works.

When you're ready to try out FAQ Plus [Version 2], or to use it in your own organization, follow the steps in the [Deployment guide](https://github.com/OfficeDev/microsoft-teams-apps-faqplusv2/wiki/DeployementGuide).

## Feedback

Thoughts? Questions? Ideas? Share them with us on [Teams UserVoice](https://microsoftteams.uservoice.com/forums/555103-public)!

Please report bugs and other code issues [here](https://github.com/OfficeDev/microsoft-teams-apps-faqplusv2/issues/new)

## Contributing

This project welcomes contributions and suggestions. Most contributions require you to agree to a Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us the rights to use your contribution. For details, visit [https://cla.microsoft.com](https://cla.microsoft.com/).

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/FAQ/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
