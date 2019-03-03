using Microsoft.Bot.Builder.Azure;
using WomanDayBot.Models;

namespace WomanDayBot.Repositories
{
  public class CardConfigurationRepository : CosmosDbRepository<CardConfiguration>
  {
    private const string DatabaseId = "WomanDayBot";
    private const string CollectionId = "CardConfiguration";

    public CardConfigurationRepository(CosmosDbStorageOptions configurationOptions)
      : base(configurationOptions, DatabaseId, CollectionId) { }
  }
}
