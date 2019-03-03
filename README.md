# Bot mini demo can be seen [here][44]
- card support
- userdata save to cosmosdb
- order save to cosmosdb

# Prerequisites
- .NET 2.1 [here][43]
- Bot Framework Emulator [here][6]
- CosmosDB local database [here][42]

# Create CosmosDB database
- start CosmosDB emulator
- go to https://localhost:8081/_explorer/index.html
- Create database WomanDayBot
- Create Collection
    - Database id: WomanDayBot
    - Collection Id: Orders
    - Partition key: /orderId
    - Throughput: 10000
    - Unique keys: /orderId
- Create Collection
    - Database id: WomanDayBot
    - Collection Id: CardConfiguration
    - Partition key: /cardconfigid
    - Throughput: 10000
    - Unique keys: /cardconfigid
- Upload Documents for local development localted in the .\WomanDayBot\Data\CardConfiguration.json (upload may hand locally, create docs one by one then
)

# WomanDayBot
Run the bot from a terminal or from Visual Studio.
    - Launch Visual Studio
    - File -> Open -> Project/Solution
    - Select `<your_project_folder>/WomanDayBot.sln` file
    - Press `F5` to run the project

# Testing the bot using Bot Framework Emulator **v4**
[Bot Framework Emulator][5] is a desktop application that allows bot developers to test and debug their bots on localhost or running remotely through a tunnel.

- Install the Bot Framework Emulator version 4.2.0 or greater from [here][6]

## Connect to the bot using Bot Framework Emulator **v4**
- Launch Bot Framework Emulator
- File -> Open Bot Configuration
- Navigate to `<your_project_folder>/WomanDayBot` folder
- Select `WomanDayBot.bot` file
- Secret is stored in Azure Bot settings (ask in skype chat)

# Further reading
- [Bot Framework Documentation][20]
- [Bot Basics][32]
- [Prompt types][23]
- [Waterfall dialogs][24]
- [Ask the user questions][26]
- [Activity processing][25]
- [Azure Bot Service Introduction][21]
- [Azure Bot Service Documentation][22]
- [.NET Core CLI tools][23]
- [Azure CLI][7]
- [msbot CLI][9]
- [Azure Portal][10]
- [Language Understanding using LUIS][11]
- [Channels and Bot Connector Service][27]

[1]: https://dev.botframework.com
[4]: https://dotnet.microsoft.com/download
[5]: https://github.com/microsoft/botframework-emulator
[6]: https://github.com/Microsoft/BotFramework-Emulator/releases
[7]: https://docs.microsoft.com/cli/azure/?view=azure-cli-latest
[8]: https://docs.microsoft.com/cli/azure/install-azure-cli?view=azure-cli-latest
[9]: https://github.com/Microsoft/botbuilder-tools/tree/master/packages/MSBot
[10]: https://portal.azure.com
[11]: https://www.luis.ai
[20]: https://docs.botframework.com
[21]: https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0
[22]: https://docs.microsoft.com/azure/bot-service/?view=azure-bot-service-4.0
[23]: https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-prompts?view=azure-bot-service-4.0
[24]: https://docs.microsoft.com/en-us/javascript/api/botbuilder-dialogs/waterfall
[25]: https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-concept-activity-processing?view=azure-bot-service-4.0
[26]: https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-tutorial-waterfall?view=azure-bot-service-4.0
[27]: https://docs.microsoft.com/en-us/azure/bot-service/bot-concepts?view=azure-bot-service-4.0
[30]: https://www.npmjs.com/package/restify
[31]: https://www.npmjs.com/package/dotenv
[32]: https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0
[40]: https://aka.ms/azuredeployment
[41]: ./PREREQUISITES.md
[42]: https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator
[43]: https://dotnet.microsoft.com/download/dotnet-core/2.1
[44]: https://www.dropbox.com/s/9qirasydixdq5g4/womandaybot.mp4
