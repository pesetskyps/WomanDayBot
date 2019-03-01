using System.Collections.Generic;
using System.Threading.Tasks;

namespace WomanDayBot
{
  public interface ICardConfigurationService
  {
    Task<List<CardConfiguration>> GetConfigurationsAsync();
  }

  public class CardConfigurationService : ICardConfigurationService
  {
    private readonly ICardConfigurationRepository _cardConfigurationRepository;

    public CardConfigurationService(ICardConfigurationRepository cardConfigurationRepository)
    {
      _cardConfigurationRepository = cardConfigurationRepository;
    }

    public async Task<List<CardConfiguration>> GetConfigurationsAsync()
    {
      return await _cardConfigurationRepository.GetAsync();
    }
  }
}
