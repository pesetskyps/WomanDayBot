// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Generated with `dotnet new corebot` vX.X.X

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace WomanDayBot
{
  public class Program
  {
    public static void Main(string[] args)
    {
      BuildWebHost(args).Run();
    }

    public static IWebHost BuildWebHost(string[] args)
    {
      return WebHost
        .CreateDefaultBuilder(args)
        .ConfigureLogging((hostingContext, logging) =>
        {
          logging.AddAzureWebAppDiagnostics();
        })
        // .UseApplicationInsights()
        .UseStartup<Startup>()
        .Build();
    }
  }
}
