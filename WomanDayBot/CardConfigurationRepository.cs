using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WomanDayBot.Orders;

namespace WomanDayBot
{
    public interface ICardConfigurationRepository
    {
        Task<List<CardConfiguration>> GetAsync();
    }
    public class CardConfigurationRepository : ICardConfigurationRepository
    {
        public async Task<List<CardConfiguration>> GetAsync()
        {
            return new List<CardConfiguration>
            {
                new CardConfiguration("https://picsum.photos/300?image=222",
                    "A bit of Champaine for miss!","Dear women. Oh common! There should be the day to feel yourself relaxed. Only today!",
                    OrderType.Champaine, 
                    OrderCategory.Alcohol),
                new CardConfiguration("https://picsum.photos/300?image=221",
                    "Morning Coffee","If you have at least one eye opened. Just press the button and fresh coffee from our home coffee machine will be at your desk!",
                    OrderType.Coffee, 
                    OrderCategory.NonAlcohol),
                new CardConfiguration("https://picsum.photos/300?image=221",
                    "Morning Burger","If you have at least one eye opened. Just press the button and fresh Burger from our home Burger machine will be at your desk!",
                    OrderType.Burger,
                    OrderCategory.Food),
                new CardConfiguration("https://picsum.photos/300?image=221",
                    "Morning Chocolate","If you have at least one eye opened. Just press the button and fresh Chocolate from our home Chocolate machine will be at your desk!",
                    OrderType.Chocolate,
                    OrderCategory.Sweet)
            };
        }
    }
}
