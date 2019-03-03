using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace WomanDayBot.Services
{
  public interface ICardService
  {
    Task<List<Attachment>> CreateAttachmentsAsync();
  }

  public class CardService : ICardService
  {
    private readonly ILogger<CardService> _logger;
    private readonly ICardConfigurationService _cardConfigurationService;

    public CardService(ILoggerFactory loggerFactory, ICardConfigurationService cardConfigurationService)
    {
      _logger = loggerFactory.CreateLogger<CardService>();
      _cardConfigurationService = cardConfigurationService;
    }

    public Task<List<Attachment>> CreateAttachmentsAsync()
    {
      return this.CreateAdaptiveCardAttachmentAsync();
    }

    private async Task<List<Attachment>> CreateAdaptiveCardAttachmentAsync()
    {
      string[] paths = { ".", "Templates", "orderCard.json" };
      var fullPath = Path.Combine(paths);
      var adaptiveCardTemplate = File.ReadAllText(fullPath);

      var cards = new List<Attachment>();

      var cardConfigurations = await _cardConfigurationService.GetCardConfigurationsAsync();
      foreach (var configuration in cardConfigurations)
      {
        var card = adaptiveCardTemplate;
        card = card.Replace(@"__TitleText__", configuration.TitleText);
        card = card.Replace(@"__OrderType__", configuration.OrderType.ToString());
        card = card.Replace(@"__OrderCategory__", configuration.OrderCategory.ToString());
        card = card.Replace(@"__Description__", configuration.Description);
        card = card.Replace(@"__ImageUrl__", configuration.ImageUrl);

        var adaptiveCard = new Attachment()
        {
          ContentType = "application/vnd.microsoft.card.adaptive",
          Content = JsonConvert.DeserializeObject(card),
        };
        cards.Add(adaptiveCard);
      }

      return cards;
    }
  }
}
