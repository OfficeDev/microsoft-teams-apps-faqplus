---
page_type: sample
languages:
- csharp
products:
- office-teams
description: FAQ Plus bot is a friendly Q&A bot that brings a human in the loop when it is unable to help with an answer from the knowledge base.
urlFragment: microsoft-teams-apps-faqplus
---
#  FAQ Plus App Template

| [Documentation](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/wiki/Home) | [Deployment guide](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/wiki/Deployment-Guide) | [Architecture](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/wiki/Solution-Overview) |
| ---- | ---- | ---- |

Chatbots on Microsoft Teams are an easy way to provide answers to frequently asked questions by users. However, most chatbots fail to engage with users in a meaningful way because there is no human in the loop when the chatbot fails to answer a question well. 

FAQ Plus bot is a friendly Q&A bot that brings a human in the loop when it is unable to help. A user can ask the bot a question and the bot responds with an answer if it's in the knowledge base. If not, the bot offers  the user an option to "Ask an expert", which posts the question to a pre-configured team of experts to provide support. An expert can assign the question to themself, chat with the user to gain more context and add the question to the knowledge base from using a messaging extention so that the next user to ask that same question will get an answer from the chatbot!

**The July 2020 (version 3) release of FAQ Plus includes a multi-turn feature to the end user experience. With the multi-turn feature, users will be presented with follow-up options along with an answer to their question. This enables the FAQ Plus bot to answer the user's question with more relevance. Multi-turn follow-up options are programmed directly into the QnA Maker when the tenant admin uploads the Q&A pairs into the knowledge base.**

**FAQ Plus provides features to the expert team such as:**
* Adding/editing/deleting/previewing QnA
* Viewing update history of QnA
* View all the existing QnA
* View the original version of the edited QnA
* View details of manually added QnA

**Here are some screenshots showing FAQ Plus in action:**

*	A user interacting with FAQ Plus through chat:

![FAQ Plus in action (user view)](https://github.com/OfficeDev/microsoft-teams-faqplusplus-app/wiki/images/FAQPlusEndUser.gif)


*	Expert using FAQ Plus:

![FAQ Plus in action (experts view)](https://github.com/OfficeDev/microsoft-teams-faqplusplus-app/wiki/images/FAQPlusExperts.gif)


*	Expert invoking the task module to add QnA pair:

![Invoking_taskmodule1](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/wiki/Images/Invoking_taskmodule1.png)

More screenshots and tips on how to use the app are in the [Wiki](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/wiki/Home) of this repository.

## Legal Notice

Please read the license terms applicable to this template [here](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/blob/master/LICENSE). In addition to these terms, you agree to the following:

- You are responsible for complying with all applicable privacy and security regulations, as well as all internal privacy and security policies of your company. You must also include your own privacy statement and terms of use for your app if you choose to deploy or share it broadly.
- This template includes functionality to provide all with required information, and it is your responsibility to ensure the data is presented accurately.
- Use and handling of any personal data collected by your app is your responsibility. Microsoft will not have any access to data collected through your app, and therefore is not responsible for any data related incidents.
- Any Microsoft trademarks and logos included in this repository are property of Microsoft and should not be reused, redistributed, modified, repurposed, or otherwise altered or used outside of this repository.

## Getting Started

Begin with the [Solution overview](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/wiki/Solution-Overview) to read about what the app does and how it works.

When you're ready to try out FAQ Plus, or to use it in your own organization, follow the steps in the [Deployment guide](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/wiki/Deployment-Guide).

## Feedback

Thoughts? Questions? Ideas? Share them with us on [Teams UserVoice](https://microsoftteams.uservoice.com/forums/555103-public)!

Please report bugs and other code issues [here](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/issues/new).

## Contributing

This project welcomes contributions and suggestions. Most contributions require you to agree to a Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us the rights to use your contribution. For details, visit [https://cla.microsoft.com](https://cla.microsoft.com/).

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/FAQ/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
