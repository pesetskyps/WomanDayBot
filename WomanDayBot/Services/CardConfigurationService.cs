using System.Collections.Generic;
using System.Threading.Tasks;
using WomanDayBot.Models;
using WomanDayBot.Repositories;

namespace WomanDayBot.Services
{
  public interface ICardConfigurationService
  {
    Task<IEnumerable<CardConfiguration>> GetConfigurationsAsync();
  }

  public class CardConfigurationService : ICardConfigurationService
  {
    private readonly CardConfigurationRepository _cardConfigurationRepository;

    public CardConfigurationService(CardConfigurationRepository cardConfigurationRepository)
    {
      _cardConfigurationRepository = cardConfigurationRepository;
    }

    public async Task<IEnumerable<CardConfiguration>> GetConfigurationsAsync()
    {
      return await _cardConfigurationRepository.GetItemsAsync();
    }
  }
}
