using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WomanDayBot.Models;
using WomanDayBot.Repositories;
using WomanDayBot.Services;

namespace WomanDayBot
{
  public class Startup
  {
    private readonly bool _isProduction = false;
    private readonly ILogger<WomanDayBotBot> _logger;

    public IConfiguration Configuration { get; }

    public Startup(IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      _isProduction = env.IsProduction();
      _logger = loggerFactory.CreateLogger<WomanDayBotBot>();

      this.Configuration = new ConfigurationBuilder()
        .SetBasePath(env.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
        .AddEnvironmentVariables()
        .Build();
    }

    public void ConfigureServices(IServiceCollection services)
    {
      var secretKey = Configuration.GetSection("botFileSecret")?.Value;
      var botFilePath = Configuration.GetSection("botFilePath")?.Value;
      if (!File.Exists(botFilePath))
      {
        throw new FileNotFoundException($"The .bot configuration file was not found. botFilePath: {botFilePath}");
      }

      // Loads .bot configuration file and adds a singleton that your Bot can access through dependency injection.
      BotConfiguration botConfig = null;
      try
      {
        botConfig = BotConfiguration.Load(botFilePath, secretKey);
      }
      catch
      {
        var msg = @"Error reading bot file. Please ensure you have valid botFilePath and botFileSecret set for your environment.
                    - You can find the botFilePath and botFileSecret in the Azure App Service application settings.
                    - If you are running this bot locally, consider adding a appsettings.json file with botFilePath and botFileSecret.
                    - See https://aka.ms/about-bot-file to learn more about .bot file its use and bot configuration.";
        throw new InvalidOperationException(msg);
      }

      services.AddSingleton(sp => botConfig ?? throw new InvalidOperationException($"The .bot configuration file could not be loaded. botFilePath: {botFilePath}"));

      // Add BotServices singleton.
      // Create the connected services from .bot file.
      services.AddSingleton(sp => new BotServices(botConfig));

      // Retrieve current endpoint.
      var environment = _isProduction ? "production" : "development";
      var service = botConfig.Services.FirstOrDefault(s => s.Type == "endpoint" && s.Name == environment);
      if (service == null && _isProduction)
      {
        // Attempt to load development environment
        service = botConfig.Services.FirstOrDefault(s => s.Type == "endpoint" && s.Name == "development");
      }

      if (!(service is EndpointService endpointService))
      {
        throw new InvalidOperationException($"The .bot file does not contain an endpoint with name '{environment}'.");
      }

      // Use persistent storage and create state management objects.
      var cosmosSettings = Configuration.GetSection("CosmosDB");
      var cosmosDbStorageOptions = new CosmosDbStorageOptions
      {
        DatabaseId = cosmosSettings["DatabaseID"],
        CollectionId = cosmosSettings["CollectionID"],
        CosmosDBEndpoint = new Uri(cosmosSettings["EndpointUri"]),
        AuthKey = cosmosSettings["AuthenticationKey"]
      };

      services.AddSingleton(cosmosDbStorageOptions);

      IStorage dataStore = new CosmosDbStorage(cosmosDbStorageOptions);

      // Register state models
      var conversationState = new ConversationState(dataStore);
      services.AddSingleton(conversationState);

      var userState = new UserState(dataStore);
      services.AddSingleton(userState);

      // Register repositories
      services.AddSingleton(new OrderRepository(cosmosDbStorageOptions));
      services.AddSingleton<CardConfigurationRepository>();

      // Register services
      services.AddSingleton<ICardConfigurationService, CardConfigurationService>();
      services.AddSingleton<ICardService, CardService>();

      services.AddBot<WomanDayBotBot>(options =>
      {
        options.CredentialProvider = new SimpleCredentialProvider(endpointService.AppId, endpointService.AppPassword);

        options.OnTurnError = async (context, exception) =>
        {
          _logger.LogError(exception, "Unhandled exception");
          await context.SendActivityAsync("Черт, эти программисты опять налажали! Неведома ошибка");
        };
      });

      services.AddSingleton<WomanDayBotAccessors>(sp =>
      {
        return new WomanDayBotAccessors(userState, conversationState)
        {
          UserDataAccessor = userState.CreateProperty<UserData>("WomanDayBot.UserData"),
          DialogStateAccessor = conversationState.CreateProperty<DialogState>("WomanDayBot.DialogState"),
          OrderCategoryAccessor = conversationState.CreateProperty<OrderCategory>("WomanDayBot.OrderCategory")
        };
      });
    }

    public void Configure(IApplicationBuilder app)
    {
      app
        .UseDefaultFiles()
        .UseStaticFiles()
        .UseBotFramework();
    }
  }
}
