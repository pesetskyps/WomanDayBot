﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using WomanDayBot.Models;
using WomanDayBot.Repositories;
using WomanDayBot.Services;

namespace WomanDayBot
{
  /// <summary>
  /// The Startup class configures services and the app's request pipeline.
  /// </summary>
  public class Startup
  {
    private ILoggerFactory _loggerFactory;
    private readonly bool _isProduction = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="Startup"/> class.
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940.
    /// <param name="env">Provides information about the web hosting environment an application is running in.</param>
    /// </summary>
    //  See https://docs.microsoft.com/en-us/aspnet/core/fundamentals/startup?view=aspnetcore-2.1 for startup fundamentals.
    public Startup(IHostingEnvironment env)
    {
      _isProduction = env.IsProduction();

      var builder = new ConfigurationBuilder()
          .SetBasePath(env.ContentRootPath)
          .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
          .AddEnvironmentVariables();

      Configuration = builder.Build();
    }

    /// <summary>
    /// Gets the configuration that represents a set of key/value application configuration properties.
    /// <value>
    /// The <see cref="IConfiguration"/> that represents a set of key/value application configuration properties.
    /// </value>
    /// </summary>
    public IConfiguration Configuration { get; }

    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// <param name="services">Specifies the contract for a <see cref="IServiceCollection"/> of service descriptors.</param>
    /// </summary>
    public void ConfigureServices(IServiceCollection services)
    {
      var secretKey = Configuration.GetSection("botFileSecret")?.Value;
      var botFilePath = Configuration.GetSection("botFilePath")?.Value;
      if (!File.Exists(botFilePath))
      {
        throw new FileNotFoundException($"The .bot configuration file was not found. botFilePath: {botFilePath}");
      }

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
    - See https://aka.ms/about-bot-file to learn more about .bot file its use and bot configuration.
    ";
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
        service = botConfig.Services.Where(s => s.Type == "endpoint" && s.Name == "development").FirstOrDefault();
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
        AuthKey = cosmosSettings["AuthenticationKey"],
      };

      services.AddSingleton(cosmosDbStorageOptions);

      IStorage dataStore = new CosmosDbStorage(cosmosDbStorageOptions);

      services.AddSingleton(new OrderRepository(cosmosDbStorageOptions));

      // Create and add conversation state.
      var conversationState = new ConversationState(dataStore);
      services.AddSingleton(conversationState);

      var userState = new UserState(dataStore);
      services.AddSingleton(userState);

      services.AddBot<WomanDayBotBot>(options =>
      {
        options.CredentialProvider = new SimpleCredentialProvider(endpointService.AppId, endpointService.AppPassword);

        ILogger logger = _loggerFactory.CreateLogger<WomanDayBotBot>();

        options.OnTurnError = async (context, exception) =>
        {
          logger.LogError($"Exception caught : {exception}");
          await context.SendActivityAsync("Черт, эти программисты опять налажали! Неведома ошибка");
        };
      });

      services.AddSingleton<CardConfigurationRepository>();
      services.AddSingleton<ICardConfigurationService, CardConfigurationService>();

      services.AddSingleton<WomanDayBotAccessors>(sp =>
      {
        var options = sp.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;

        return new WomanDayBotAccessors(userState, conversationState)
        {
          UserDataAccessor = userState.CreateProperty<UserData>("WomanDayBot.UserData"),
          DialogStateAccessor = conversationState.CreateProperty<DialogState>("WomanDayBot.DialogState"),
          OrderCategoryAccessor = conversationState.CreateProperty<OrderCategory>("WomanDayBot.OrderCategory")
        };
      });

      services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

      // In production, the React files will be served from this directory
      services.AddSpaStaticFiles(configuration =>
      {
        configuration.RootPath = "ClientApp/build";
      });

    }

    /// <summary>
    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    /// <param name="app">Application Builder.</param>
    /// <param name="env">Hosting Environment.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> to create logger object for tracing.</param>
    /// </summary>
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      _loggerFactory = loggerFactory;

      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      else
      {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
      }

      app.UseDefaultFiles()
          .UseStaticFiles()
          .UseBotFramework()
          .UseHttpsRedirection()
          .UseSpaStaticFiles();


      app.UseMvc(routes =>
      {
        routes.MapRoute(
                  name: "default",
                  template: "{controller}/{action=Index}/{id?}");
      });

      app.UseSpa(spa =>
      {
        spa.Options.SourcePath = "ClientApp";

        if (env.IsDevelopment())
        {
          spa.UseReactDevelopmentServer(npmScript: "start");
        }
      });
    }
  }
}