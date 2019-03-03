using System;
using Newtonsoft.Json;

namespace WomanDayBot.Models
{
  public class Order
  {
    [JsonProperty(PropertyName = "orderId")]
    public Guid Id { get; set; }
    public string OrderType { get; set; }
    public string OrderCategory { get; set; }
    public bool IsComplete { get; set; }
    public UserData UserData { get; set; }
    public DateTime RequestTime { get; set; }
  }
}
