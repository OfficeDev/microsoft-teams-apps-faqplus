# Data flow diagram to detail out the data flow between various services in  FAQ Plus bot

## Add question using compose action

- User clicks on '+' icon on messaging extension, task module is invoked.

- On click of 'Save' button, bot checks for knowledge base associated with user's team from Azure storage.

- With knowledge base Id, question is added in QnAMaker and corresponding result is shown in card.

![dfd_add_ question](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/wiki/Images/Dfd_AddQuestion.png)

## Update/Delete question

- User updates and deleted QnA pair using Update and Delete buttons on adaptive/hero card.

- On clicking buttons, bot checks for knowledge base associated with user's team from Azure storage.

- With knowledge base Id, question is updated/deleted in QnAMaker and corresponding result is shown in card.

![dfd_update_delete](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/wiki/Images/Dfd_UpdateDelete.png)
  
## Azure function for publishing knowledge base

- Azure function is triggered every fifteen minutes to publish knowledge base.

- It publishes knowledge base only if modification is done from last publish time.

![dfd_Publish knowledge base](https://github.com/OfficeDev/microsoft-teams-apps-faqplus/wiki/Images/Dfd_Publish.png)