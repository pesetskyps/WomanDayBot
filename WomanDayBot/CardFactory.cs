using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WomanDayBot
{
    public interface ICardFactory
    {
         Task<List<Attachment>> CreateAsync();
    }
    public class CardFactory : ICardFactory
    {
        private readonly ILogger<CardFactory> _logger;
        private readonly ICardConfigurationService _cardConfigurationService;

        public CardFactory(ILoggerFactory loggerFactory, ICardConfigurationService cardConfigurationService)
        {
            _logger = loggerFactory.CreateLogger<CardFactory>();
            _cardConfigurationService = cardConfigurationService;
        }

        public Task<List<Attachment>> CreateAsync()
        {
            return this.CreateAdaptiveCardAttachmentAsync();
        }

        private async Task<List<Attachment>> CreateAdaptiveCardAttachmentAsync()
        {
            string[] paths = { ".", "Dialogs", "Welcome", "Resources", "orderCard.json" };
            string fullPath = Path.Combine(paths);
            var adaptiveCardTemplate = File.ReadAllText(fullPath);

            var cardConfigurations = await _cardConfigurationService.ConfigureAsync();
            var cards = new List<Attachment>();
            foreach (var configuration in cardConfigurations)
            {
                var card = adaptiveCardTemplate;
                card = card.Replace(@"__TitleText__", configuration.TitleText);
                card = card.Replace(@"__TitleId__", configuration.TitleId);
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
