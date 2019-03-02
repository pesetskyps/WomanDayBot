using Microsoft.Bot.Builder.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WomanDayBot.Orders;

namespace WomanDayBot
{
    public class CardConfigurationRepository : CosmosDbRepository<CardConfiguration>
    {
        private const string DatabaseId = "WomanDayBot";
        private const string CollectionId = "CardConfiguration";
        public CardConfigurationRepository(CosmosDbStorageOptions configurationOptions) : base(configurationOptions, DatabaseId, CollectionId){}
    }
}
