using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WomanDayBot
{
    public interface ICardConfigurationService
    {
        Task<IEnumerable<CardConfiguration>> ConfigureAsync();
    }
    public class CardConfigurationService : ICardConfigurationService
    {
        private readonly CardConfigurationRepository _cardConfigurationRepository;

        public CardConfigurationService(CardConfigurationRepository cardConfigurationRepository)
        {
            _cardConfigurationRepository = cardConfigurationRepository;
        }

        public async Task<IEnumerable<CardConfiguration>> ConfigureAsync()
        {
            return  await _cardConfigurationRepository.GetItemsAsync();
        }
    }
}
