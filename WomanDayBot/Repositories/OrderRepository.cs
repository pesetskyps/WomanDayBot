using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Bot.Builder.Azure;
using WomanDayBot.Orders;

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
