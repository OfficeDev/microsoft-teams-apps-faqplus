---
page_type: sample
languages:
- csharp
products:
- office-teams
description: FAQ Plus bot is one of the best friendly Q&A bots that bring a human in the loop when it is unable to help with an answer from the knowledge base.
urlFragment: microsoft-teams-apps-faqplus
---

#  Note

> Beginning 1st October, 2022, you won’t be able to create new QnA Maker resources or knowledge bases. All existing QnA Maker service will be supported till [31st of March, 2025](https://azure.microsoft.com/en-us/updates/azure-qna-maker-will-be-retired-on-31-march-2025/).
>
> A newer version of the [custom question and answering](https://azure.microsoft.com/en-us/products/cognitive-services/question-answering/) capability is now available as part of Azure Cognitive Service for Language. The newly released version 5.0 of FAQ Plus is using question answering within the Language Service.


#  FAQ Plus App Template

| [Documentation](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/wiki/Home) | [Deployment guide](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/wiki/Deployment-Guide) | [Architecture](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/wiki/Solution-Overview) |
| ---- | ---- | ---- |

Chatbots on Microsoft Teams are an easy way to provide answers to frequently asked questions by users. However, most chatbots fail to engage with users in a meaningful way because there is no human in the loop when the chatbot fails to answer a question well. 

FAQ Plus bot is a friendly Q&A bot that brings a human in the loop when it is unable to help. A user can ask the bot a question and the bot responds with an answer if it's in the knowledge base. If not, the bot offers  the user an option to "Ask an expert", which posts the question to a pre-configured team of experts to provide support. An expert can assign the question to themself, chat with the user to gain more context and add the question to the knowledge base from using a messaging extention so that the next user to ask that same question will get an answer from the chatbot!

**The December 2022 (version 5) release of FAQ Plus includes [Question Answering](https://learn.microsoft.com/en-us/azure/cognitive-services/language-service/question-answering/overview), an Azure Cognitive Service for Language feature. Question answering provides cloud-based Natural Language Processing (NLP) that allows you to create a natural conversational layer over your data. It is used to find the most appropriate answer for any input from your custom knowledge base (KB) of information. 
Build a knowledge base by adding unstructured documents or extracting questions and answers from your semi-structured content, including FAQ, manuals, and documents. Get the best answers from the questions and answers in your knowledge base—automatically. Question Answering supports a multi-turn feature to the end user experience. With the multi-turn feature, users will be presented with follow-up options along with an answer to their question. This enables the FAQ Plus bot to answer the user's question with more relevance. Multi-turn follow-up options are programmed directly into the Question Answering when the tenant admin uploads the Q&A pairs into the knowledge base.
The latest (version 5) release of FAQ Plus separates the end-user and the sme bot. With splitting the bot and having different bot registrations, users can now setup different permission policies for these two bots.**

****

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

This app template is provided under the [MIT License](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/blob/master/LICENSE) terms.  In addition to these terms, by using this app template you agree to the following:

- You, not Microsoft, will license the use of your app to users or organization. 

- This app template is not intended to substitute your own regulatory due diligence or make you or your app compliant with respect to any applicable regulations, including but not limited to privacy, healthcare, employment, or financial regulations.

- You are responsible for complying with all applicable privacy and security regulations including those related to use, collection and handling of any personal data by your app. This includes complying with all internal privacy and security policies of your organization if your app is developed to be sideloaded internally within your organization. Where applicable, you may be responsible for data related incidents or data subject requests for data collected through your app.

- Any trademarks or registered trademarks of Microsoft in the United States and/or other countries and logos included in this repository are the property of Microsoft, and the license for this project does not grant you rights to use any Microsoft names, logos or trademarks outside of this repository. Microsoft’s general trademark guidelines can be found [here](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general.aspx).

- If the app template enables access to any Microsoft Internet-based services (e.g., Office365), use of those services will be subject to the separately-provided terms of use. In such cases, Microsoft may collect telemetry data related to app template usage and operation. Use and handling of telemetry data will be performed in accordance with such terms of use.

- Use of this template does not guarantee acceptance of your app to the Teams app store. To make this app available in the Teams app store, you will have to comply with the [submission and validation process](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/deploy-and-publish/appsource/publish), and all associated requirements such as including your own privacy statement and terms of use for your app.

## Getting Started

Begin with the [Solution overview](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/wiki/Solution-Overview) to read about what the app does and how it works.

When you're ready to try out FAQ Plus, or to use it in your own organization,  you can choose to follow one of the below guides.
* [Deployment guide powershell](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/wiki/Deployment-Guide-manual).
    * **Recommended** Use this option to deploy the FAQ Plus v5.0 using powershell script. The entire set-up is done by the powershell script.
* [Deployment guide](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/wiki/Deployment-Guide).
    * Use this option to deploy the FAQ+ v5.0 manually.

## Migration

If you already have older version of FAQ Plus installed, then please use this [migration guide](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/wiki/Migration-Guide-manual). Please note that deploying the major version update, like FAQ Plus version 5.0 involves more than syncing the App Service and Azure Functions, so plan to review the migration guide before migrating to latest.

## Feedback

Thoughts? Questions? Ideas? Share them with us [here](https://aka.ms/fqbappfeedback)!

Please report bugs and other code issues [here](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/issues/new).

## Contributing

This project welcomes contributions and suggestions. Most contributions require you to agree to a Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us the rights to use your contribution. For details, visit [https://cla.microsoft.com](https://cla.microsoft.com/).

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/FAQ/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
