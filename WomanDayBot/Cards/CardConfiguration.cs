using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WomanDayBot.Orders;

namespace WomanDayBot
{
    public class CardConfiguration
    {
        public CardConfiguration(string ImageUrl, string TitleText, string Description, OrderType OrderType, OrderCategory OrderCategory)
        {
            this.OrderType = OrderType;
            this.Description = Description;
            this.ImageUrl = ImageUrl;
            this.TitleText = TitleText;
            this.OrderCategory = OrderCategory;
        }

        public string ImageUrl { get; private set; }
        public string TitleText { get; private set; }
        public string Description { get; private set; }
        public OrderType OrderType { get; private set; }
        public OrderCategory OrderCategory { get; private set; }
    }
}
