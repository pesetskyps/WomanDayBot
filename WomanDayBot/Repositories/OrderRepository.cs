using Microsoft.Bot.Builder.Azure;
using WomanDayBot.Models;

namespace WomanDayBot.Repositories
{
  public class OrderRepository : CosmosDbRepository<Order>
  {
    private const string DatabaseId = "WomanDayBot";
    private const string CollectionId = "Orders";

    public OrderRepository(CosmosDbStorageOptions configurationOptions) 
      : base(configurationOptions, DatabaseId, CollectionId) { }
  }
}
