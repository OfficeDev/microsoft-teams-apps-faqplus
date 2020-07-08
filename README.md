---
page_type: sample
languages:
- csharp
products:
- office-teams
description: FAQ Plus bot is a friendly Q&A bot that brings a human in the loop when it is unable to help with an answer from the knowledge base.
urlFragment: microsoft-teams-apps-faqplusv2
---
#  FAQ Plus App Template

| [Documentation](https://github.com/OfficeDev/microsoft-teams-apps-faqplusv2/wiki/Home) | [Deployment guide](https://github.com/OfficeDev/microsoft-teams-apps-faqplusv2/wiki/Deployment-Guide) | [Architecture](https://github.com/OfficeDev/microsoft-teams-apps-faqplusv2/wiki/Solution-Overview) |
| ---- | ---- | ---- |

Chatbots are an easy way to provide answers to frequently asked questions by end-users. However, most chatbots fail to engage with end-users in a meaningful way because there is no human in the loop when the chatbot fails. 

FAQ Plus bot is a friendly Q&A bot that brings a human in the loop when it is unable to help. One can ask the bot a question and the bot responds with an answer if it's in the knowledge base. If not, the bot allows the end-user to submit a query which then gets posted in a pre-configured team of experts to provide support by acting upon the notifications from within their team itself.

**The July 2020 (version 3) release of FAQ Plus includes a multi-turn feature to the end user experience. With the multi-turn feature, users will be presented with follow-up options along with an answer to their question. This enables the FAQ Plus bot to answer the user's question with more relevance. Multi-turn follow-up options are programmed directly into the QnA Maker when the tenant admin uploads the proper documents into the knowledge base.**

**Earlier releases of FAQ Plus has following features:**

*	An Enduser interacting with FAQ Plus:

![FAQ Plus in action (user view)](https://github.com/OfficeDev/microsoft-teams-faqplusplus-app/wiki/images/FAQPlusEndUser.gif)

*	Experts team using FAQ Plus:

![FAQ Plus in action (experts view)](https://github.com/OfficeDev/microsoft-teams-faqplusplus-app/wiki/images/FAQPlusExperts.gif)


**FAQ Plus [Version 2] (To_Do_Link) provides new features to experts team such as:**
* Adding/editing/deleting/previewing QnA
* Viewing update history of QnA
* View all the existing QnA
* View the original version of the edited QnA
* View details of manually added QnA

Experts team invoking the task module to add QnA pair:

![Invoking_taskmodule1](/Wiki/Images/Invoking_taskmodule1.png)

Experts team configuring the bot to respond with a hero card as an answer to a question:

![Invoking_taskmodule2](/Wiki/Images/Invoking_taskmodule2.png)

![Add question screen 1](/Wiki/Images/add-question-richcard1.png)

Experts team previewing the QnA pair before saving:
   
![Preview_Rich_card](/Wiki/Images/Preview_Rich_card.png)

Experts team updating the QnA pair:

![Updating_Question-ui1](/Wiki/Images/Updating_Question-ui1.png)

![Updating_Question-ui2](/Wiki/Images/Updating_Question-ui2.png)

![Updating_Question-ui4](/Wiki/Images/Updating_Question-ui4.png)

Some of the fields are markdown supported and are indicated with "(Markdown supported)" beside the field label:
   
![Adding_Markdown-Support-1](/Wiki/Images/Adding_Markdown-Support1.png)
   
This is how the card will look like when the bot responds with the answer to the Experts team:

![Adding_Markdown-Support-3](/Wiki/Images/Adding_Markdown-Support3.png)

This is how the card will look like when the bot responds with the answer to the End-user:

![End-user_Rich_Card](/Wiki/Images/End-user_Rich_Card.png)

## Legal Notices

Please read the license terms applicable to this template [here](https://nam06.safelinks.protection.outlook.com/?url=https%3A%2F%2Fgithub.com%2FOfficeDev%2Fmicrosoft-teams-company-communicator-app%2Fblob%2Fmaster%2FLICENSE&data=04%7C01%7CNidhi.Shandilya%40microsoft.com%7C263aa462997e45b8e20108d7479b86be%7C72f988bf86f141af91ab2d7cd011db47%7C1%7C0%7C637056605626329514%7CUnknown%7CTWFpbGZsb3d8eyJWIjoiMC4wLjAwMDAiLCJQIjoiV2luMzIiLCJBTiI6Ik1haWwiLCJXVCI6Mn0%3D%7C-1&sdata=o6iWeK9uPnMs9vWhK29bXG6FSitjy8S64aXiM%2B5%2FdtE%3D&reserved=0). In addition to these terms, you agree to the following:

- You are responsible for complying with all applicable privacy and security regulations, as well as all internal privacy and security policies of your company. You must also include your own privacy statement and terms of use for your app if you choose to deploy or share it broadly.
- This template includes functionality to provide all with required information, and it is your responsibility to ensure the data is presented accurately.
- Use and handling of any personal data collected by your app is your responsibility. Microsoft will not have any access to data collected through your app, and therefore is not responsible for any data related incidents.
- Any Microsoft trademarks and logos included in this repository are property of Microsoft and should not be reused, redistributed, modified, repurposed, or otherwise altered or used outside of this repository.

## Getting Started

Begin with the [Solution overview](/wiki/Solution-overview) to read about what the app does and how it works.

When you're ready to try out FAQ Plus [Version 2], or to use it in your own organization, follow the steps in the [Deployment guide](/wiki/DeployementGuide).

## Feedback

Thoughts? Questions? Ideas? Share them with us on [Teams UserVoice](https://microsoftteams.uservoice.com/forums/555103-public)!

Please report bugs and other code issues [here](/issues/new).

## Contributing

This project welcomes contributions and suggestions. Most contributions require you to agree to a Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us the rights to use your contribution. For details, visit [https://cla.microsoft.com](https://cla.microsoft.com/).

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/FAQ/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.