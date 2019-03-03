using System;
using Newtonsoft.Json;

namespace WomanDayBot.Models
{
  public class CardConfiguration
  {
    public CardConfiguration(
      string imageUrl, 
      string titleText, 
      string description, 
      string orderType, 
      string orderCategory)
    {
      this.OrderType = orderType;
      this.Description = description;
      this.ImageUrl = imageUrl;
      this.TitleText = titleText;
      this.OrderCategory = orderCategory;
    }

    [JsonProperty(PropertyName = "cardconfigid")]
    public Guid CardConfigurationId { get; private set; }
    public string ImageUrl { get; private set; }
    public string TitleText { get; private set; }
    public string Description { get; private set; }
    public string OrderType { get; private set; }
    public string OrderCategory { get; private set; }
  }
}
