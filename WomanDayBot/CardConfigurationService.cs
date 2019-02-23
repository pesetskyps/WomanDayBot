using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WomanDayBot
{
    public interface ICardConfigurationService
    {
        Task<List<CardConfiguration>> ConfigureAsync();
    }
    public class CardConfigurationService : ICardConfigurationService
    {
        private readonly ICardConfigurationRepository _cardConfigurationRepository;

        public CardConfigurationService(ICardConfigurationRepository cardConfigurationRepository)
        {
            _cardConfigurationRepository = cardConfigurationRepository;
        }

        public async Task<List<CardConfiguration>> ConfigureAsync()
        {
            return  await _cardConfigurationRepository.GetAsync();
        }
    }
}
