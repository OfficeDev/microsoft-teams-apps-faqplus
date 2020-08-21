## Assumptions

The estimate below assumes:

-   500 users in the tenant
-   Each user performs 5 add, update or delete operations per day.
-   Each user uses messaging extension 25 times/week.

## [](/wiki/costestimate#sku-recommendations)SKU recommendations

The recommended SKUs for a production environment are:

-   QnAMaker: Standard (S0)
-   App Service: Standard (S2)
-   Azure Search: Basic
    -   Create up to 14 knowledge bases
    -   The Azure Search service cannot be upgraded once it is provisioned, so select a tier that will meet your anticipated needs.


## Estimated load

**Number of QnA queries**: 500 users * 5 questions/user/day * 30 (number of days in a month) = 75000 questions/month

**Data storage**: 1 GB max    

**Table data operations**:
* Configuration
   * (2 reads/question * 75000 questions) + (1 read/escalation * 75000 escalations) + (1 read/update * 2 updates/ticket * 75000 tickets) = 375000 reads
   * (1 write/update * 3 updates/ticket * 75000 tickets) = 225000 writes

**Blob data operations**:

* Blob storage is called when messaging extension is used. 
* Total number of read calls in storage = 4 calls/hour(Azure Function) * 24 hours/day * 30 days = 2880
* Total number of write calls in storage =  4 calls/hour(Azure Function) * 24 hours/day * 30 days = 2880 

## Estimated cost

**IMPORTANT:** This is only an estimate, based on the assumptions above. Your actual costs may vary.

Prices were taken from the [Azure Pricing Overview](https://azure.microsoft.com/en-us/pricing/) on 27 December 2019, for the West US 2 region.

Use the [Azure Pricing Calculator](https://azure.com/e/595930b9653945a2870a339a5ea8bce2) to model different service tiers and usage patterns.

Resource                                    | Tier          | Load          | Monthly price
---                                         | ---           | ---           | --- 
Storage account (Table)                     | Standard_LRS  | < 1GB data, 75,000 operations | $0.05 + $0.01 = $0.06
Storage account (Blob)                      |Standard_LRS   |< 1GB data, 5,000 write operations, 5,000 read operations|$0.05|
Bot Channels Registration                   | F0            | N/A           | Free
App Service Plan                            | S2            | 744 hours     | $148.8
App Service (Messaging Extension)           | -             |               | (charged to App Service Plan) 
Application Insights (Messaging Extension)  | -             | < 5GB data    | (free up to 5 GB)
App Service (Configuration)                 | -             |               |  (charged to App Service Plan)
Application Insights (Configuration)        | -             | < 5GB data    | (free up to 5 GB)
QnAMaker Cognitive Service                  | S0            |               | $10
Azure Search                                | B             |               | $75.14
App Service (QnAMaker)                      | F0            |               | (charged to App Service Plan)
Application Insights (QnAMaker)             | -             | < 5GB data    | (free up to 5 GB)
Azure Function                              |Dedicated      |4 executions/hour * 24 hours/day * 30 days = 2880 executions|(free up to 1 million executions)
**Total**                                   |               |               | **$233.99**