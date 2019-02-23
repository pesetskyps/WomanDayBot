using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                new CardConfiguration("https://picsum.photos/300?image=222","Bro","That's for Bro","bro"),
                new CardConfiguration("https://picsum.photos/300?image=221","No Bro","That's for Not Bro","notbro")
            };
        }
    }
}
