![architecture-overview](./Images/architecture_overview.png)

The **FAQ Plus** application has the following main components:

* **Question Answering**: Resources that comprise the Question Answering service, which implements the "FAQ" part of the application. The installer creates a [Question Answering Project Knowledge Base](https://learn.microsoft.com/en-us/azure/cognitive-services/language-service/question-answering/how-to/manage-knowledge-base) using the [tools](https://learn.microsoft.com/en-us/azure/cognitive-services/language-service/question-answering/concepts/project-development-lifecycle) provided by Question Answering.

* **FAQ Plus Expert Bot**: The bot serves experts team
	* The experts team receives notifications from the bot when end-users ask questions to expert or create feedback items. The bot tracks questions in a simple "ticketing system", with a basic life cycle of Unassigned -> Assigned to expert -> Closed. The bot notifies both the end-user and the experts team as the request changes states.
	* Using the bot, members of the experts team can add QnA to the existing knowledge base.
	* The bot also implements a messaging extension that lets members of the expert team search for tickets or questions in the knowledge base.

* **FAQ Plus User Bot**: The bot serves both end-users and experts team
	* The knowledge base (KB) in Question Answering is presented to end-user in a 1:1 conversational bot interface. Through the bot, end-user can ask the question to bot, escalate to a designated experts team, send feedback about the app, or give feedback on specific answers.

* **Blob Storage** : The knowledge base with QnA and associated metadata is stored in blob storage by Azure function. The same is shown by messaging extension for respective section and search categories using Azure search service.

* **Azure Function**: QnA changes in Question Answering are published every fifteen minutes to knowledge base by time triggered Azure functions.
  
* **Configuration Application**: An Azure App Service lets app admins configure the application to provide team and knowledge base details. These values are necessary to map the expert team and the associated knowledge base. Currently app supports only one Question Answering Project knowledge base per tenant(deployment).

## Question Answering

FAQ Plus uses Question Answering to respond to user questions; in fact, you can have a blank knowledge base to start using FAQ Plus. The prQuestion answering provides cloud-based Natural Language Processing (NLP) that allows you to create a natural conversational layer over your data. It is used to find the most appropriate answer for any input from your custom knowledge base (KB) of information. Please keep in mind that a good knowledge base requires curation and feedback: see [Development lifecycle of a knowledge base](https://learn.microsoft.com/en-us/azure/cognitive-services/language-service/question-answering/concepts/project-development-lifecycle).

For more details about Question Answering, please refer to the [Question Answering documentation](https://learn.microsoft.com/en-us/azure/cognitive-services/language-service/question-answering/overview).

## Bot and Messaging Extension

The bot is built using the [Bot Framework SDK v4 for .NET](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0) and [ASP.NET Core 3.1](https://docs.microsoft.com/en-us/aspnet/core/?view=aspnetcore-3.1). The bot has a conversational interface in personal (1:1) scope for end-users and in team scope for the experts team. It also implements a messaging extension with [query commands](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/messaging-extensions/search-extensions), which the experts team can use to search for and share requests or knowledge base questions.

## Azure Function
Azure function [ASP.NET Core 3.1](https://docs.microsoft.com/en-us/aspnet/core/?view=aspnetcore-3.1) is used in building the bot. It publishes the QnA pair in the Question Answering and stores the QnA in the blob storage. It creates a search index to search questions in the knowledge base tab on messaging extension.

## Blob Storage
Blob Storage stores the QnA pair in JSON format. QnA can be searched from the search tab in the messaging extension.

## Configuration App
The configuration app is a standard [ASP.NET MVC 5](https://docs.microsoft.com/en-us/aspnet/mvc/mvc5) web app. The configuration app will be used infrequently, so the included ARM template puts it in the same App Service Plan as the bot and Question Answering.

From this simple web interface, app administrators can:

* designate the experts team
* set the knowledge base to query
* set the welcome message that's sent to all end-users
* set the content of the Help tab
