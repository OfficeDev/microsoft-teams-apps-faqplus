# Telemetry

The FAQ Plus app logs telemetry to [Azure Application Insights](https://azure.microsoft.com/en-us/services/monitor/). You can go to the respective Application Insights blade of the Azure App Services to view basic telemetry about your services, such as requests, failures, and dependency errors, custom events, traces etc.

The FAQ Plus app integrates with Application Insights to gather bot activity analytics, as described [here](https://blog.botframework.com/2019/03/21/bot-analytics-behind-the-scenes/).

The app logs AadObjectId of user for tracing logs. The deployer should ensure that the solution meets their privacy/data retention requirements, and can choose to remove it if they wish.

The FAQ Plus app logs following events:

`Activity`:
- Basic activity info: `ActivityId`, `ActivityType`, `Event Name`
- Basic user info: `FromID`

`UserActivity`:
- Basic activity info: `ActivityId`, `ActivityType`, `Event Name`
- Basic user info: `UserAadObjectId`
- Context of how it was invoked: `ConversationType`

`customEvents`:
- CRUD operations logging.

*Application Insights queries:*

- This query gives total number of times Bot is added to 1:1 chat.

```
traces
| where message contains "Bot added to 1:1 chat"
```

- This query gives total number of times Bot is successfully added to team.

```
traces
| where message contains "Bot added to team"
```

- This query gives total number of times  user sends feedback card.

```
traces
| where message contains "Sending user feedback card" 
```

- This query gives total number of times Bot sends ask an expert card.

```
traces
| where message contains "Sending user ask an expert card"
```

- This query gives total number of times  card is submitted in channel.
```
traces
| where message contains "Card submit in channel"
```

- This query gives total number of times Bot sends tour card.
```
traces
| where message contains "Sending team tour card"
```

- This query gives total number of times expert receives question.
```
traces
| where message contains "Received question for expert"
```

- This query gives total number of times the app feedback is received.
```
traces
| where message contains "Received app feedback"
```
For e.g.: trace showing the total number of times feedback card is sent.

![trace_example](/Wiki/Images/trace_example.png)

The **Configurator** app with Application Insights to gather event activity analytics, as described [here]((https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)).

The Configurator App logs following events:

`Exceptions`:

- Global exceptions logging.

`customEvents`:

- CRUD operations logging.

*Application Insights Log Levels:*
- **Trace = 0** : Logs that contain the most detailed messages. These messages may contain sensitive application data. These messages are disabled by default and should never be enabled in a production environment.
- **Debug = 1** : Logs that are used for interactive investigation during development. These logs should primarily contain information useful for debugging and have no long-term value.
- **Information = 2** : Logs that track the general flow of the application. These logs should have long-term value.
- **Warning = 3** :Logs that highlight an abnormal or unexpected event in the application flow, but do not otherwise cause the application execution to stop
- **Error = 4** : Logs that highlight when the current flow of execution is stopped due to a failure. These should indicate a failure in the current activity, not an application-wide failure.
- **Critical = 5** :Logs that describe an unrecoverable application or system crash, or a catastrophic failure that requires immediate attention.
- **None = 6** : Not used for writing log messages. Specifies that a logging category should not write any messages.

If the Admin user wants to change Log Level, he/she has to go to Application Settings in the Configuration of the App Service and change the Log Level value for "ApplicationInsightsLogLevel" .
For e.g. 
"ApplicationInsightsLogLevel": "Information"

Below are the possible values of Log Level:  
1. Trace
2. Debug
3. Information
4. Warning
5. Error
6. Critical
7. None