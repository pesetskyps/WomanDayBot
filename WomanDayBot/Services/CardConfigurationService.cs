using System.Collections.Generic;
using System.Threading.Tasks;
using WomanDayBot.Models;
using WomanDayBot.Repositories;

namespace WomanDayBot.Services
{
  public interface ICardConfigurationService
  {
    Task<IEnumerable<CardConfiguration>> GetCardConfigurationsAsync();
  }

  public class CardConfigurationService : ICardConfigurationService
  {
    private readonly CardConfigurationRepository _cardConfigurationRepository;

    public CardConfigurationService(CardConfigurationRepository cardConfigurationRepository)
    {
      _cardConfigurationRepository = cardConfigurationRepository;
    }

    public async Task<IEnumerable<CardConfiguration>> GetCardConfigurationsAsync()
    {
      return await _cardConfigurationRepository.GetItemsAsync();
    }
  }
}
