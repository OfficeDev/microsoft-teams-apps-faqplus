## FAQ Plus

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

![FAQ Plus in action (user view1)](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/wiki/Images/UserInteraction1.png)

![FAQ Plus in action (user view2)](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/wiki/Images/UserInteraction2.png)

![FAQ Plus in action (user view3)](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/wiki/Images/UserInteraction3.png)


*	Expert using FAQ Plus:

![FAQ Plus in action (experts view1)](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/wiki/Images/ExpertInteraction1.png)

![FAQ Plus in action (experts view2)](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/wiki/Images/ExpertInteraction2.png)

*	Expert invoking the task module to add QnA pair:

![Invoking_taskmodule1](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/wiki/Images/Invoking_taskmodule1.png)


*	Expert configuring the bot to respond with a hero card as an answer to a question:

![Add question screen 1](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/wiki/Images/add-question-richcard1.png)

*	Expert previewing the QnA pair before saving:
   
![Preview_Rich_card](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/wiki/Images/Preview_Rich_card.png)


*	Expert updating the QnA pair:

![Updating_Question-ui1](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/wiki/Images/Updating_Question-ui1.png)

![Updating_Question-ui2](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/wiki/Images/Updating_Question-ui2.png)

![Updating_Question-ui4](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/wiki/Images/Updating_Question-ui4.png)


*	Some of the fields are markdown supported and are indicated with "(Markdown supported)" beside the field label:
   
![Adding_Markdown-Support-1](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/wiki/Images/Adding_Markdown-Support1.png)
   

*	This is how the card will look like when the bot responds with the answer to the Experts team:

![Adding_Markdown-Support-3](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/wiki/Images/Adding_Markdown-Support3.png)


*	This is how the card will look like when the bot responds with the answer to the End-user:

![End-user_Rich_Card](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/wiki/Images/End-user_Rich_Card.png)

Please refer the following documentation links for further details related to the app:

- [Solution overview](Solution-Overview)
	- [Data stores](Data-Stores)
	- [Cost estimate](Cost-Estimates)

- Deploying the app
	- [Deployment guide](Deployment-Guide)
	- [Troubleshooting](Troubleshooting)
