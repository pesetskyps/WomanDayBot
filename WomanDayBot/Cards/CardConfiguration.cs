using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WomanDayBot.Orders;

namespace WomanDayBot
{
    public class CardConfiguration
    {
        public CardConfiguration(string ImageUrl, string TitleText, string Description, string OrderType, string OrderCategory)
        {
            this.OrderType = OrderType;
            this.Description = Description;
            this.ImageUrl = ImageUrl;
            this.TitleText = TitleText;
            this.OrderCategory = OrderCategory;
        }

        [JsonProperty(PropertyName = "cardconfigid")]
        public Guid CardConfigurationId { get; private set; }
        public string ImageUrl { get; private set; }
        public string TitleText { get; private set; }
        public string Description { get; private set; }
        public string OrderType { get; private set; }
        public string OrderCategory { get; private set; }
    }
}
