using System;
using Newtonsoft.Json;
using WomanDayBot.Users;

namespace WomanDayBot.Orders
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
