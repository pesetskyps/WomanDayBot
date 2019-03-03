using Microsoft.Bot.Builder.Azure;

namespace WomanDayBot.Orders
{
  public class OrderRepository : CosmosDbRepository<Order>
    {
        private const string DatabaseId = "WomanDayBot";
        private const string CollectionId = "Orders";
        public OrderRepository(CosmosDbStorageOptions configurationOptions) : base(configurationOptions, DatabaseId, CollectionId)
        {

        }
    }
}
