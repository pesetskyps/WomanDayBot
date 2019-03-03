using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using WomanDayBot.Orders;

namespace WomanDayBot
{
    public interface ICardFactory
    {
         Task<List<Attachment>> CreateAsync();
         ICardFactory WithCategory(OrderCategory orderCategory);
    }
    public class CardFactory : ICardFactory
    {
        private readonly ILogger<CardFactory> _logger;
        private readonly ICardConfigurationService _cardConfigurationService;
        private OrderCategory _orderCategory;

        public CardFactory(ILoggerFactory loggerFactory, ICardConfigurationService cardConfigurationService)
        {
            _logger = loggerFactory.CreateLogger<CardFactory>();
            _cardConfigurationService = cardConfigurationService;
        }

        public Task<List<Attachment>> CreateAsync()
        {
            return this.CreateAdaptiveCardAttachmentAsync();
        }

        public ICardFactory WithCategory(OrderCategory orderCategory)
        {
          _orderCategory = orderCategory;
          return this;
        }

        private async Task<List<Attachment>> CreateAdaptiveCardAttachmentAsync()
        {
            string[] paths = { ".", "Cards", "Templates", "orderCard.json" };
            string fullPath = Path.Combine(paths);
            var adaptiveCardTemplate = File.ReadAllText(fullPath);
            var cardConfigurations = await _cardConfigurationService.ConfigureAsync();
            _logger.LogTrace($"Found {cardConfigurations.Count} configurations");
            var cards = new List<Attachment>();
            var filteredConfigurations = cardConfigurations.Where(c =>
              _orderCategory == OrderCategory.All || c.OrderCategory == _orderCategory);
            foreach (var configuration in filteredConfigurations)
            {
                var card = JObject.Parse(adaptiveCardTemplate);
                card["body"][0]["url"] = configuration.ImageUrl;
                card["body"][1]["columns"][0]["items"][0]["text"] = configuration.TitleText;
                card["body"][1]["columns"][0]["items"][2]["text"] = configuration.Description;
                card["actions"][0]["id"] = configuration.OrderType.ToString();
                card["actions"][0]["data"]["orderType"] = configuration.OrderType.ToString();
                card["actions"][0]["data"]["orderCategory"] = configuration.OrderCategory.ToString();
        
                var adaptiveCard = new Attachment
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = card
                };
                cards.Add(adaptiveCard);
            }
           
            return cards;
        }
    }
}
