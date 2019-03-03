using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using WomanDayBot.Orders;

namespace WomanDayBot.Web.Controllers
{
  [Route("api/[controller]")]
  public class OrdersController : Controller
  {
    private readonly ILogger<WomanDayBotBot> _logger;
    private readonly OrderRepository _orderRepository;
    public OrdersController(ILoggerFactory loggerFactory, OrderRepository orderRepository)
    {
      _logger = loggerFactory.CreateLogger<WomanDayBotBot>();
      _orderRepository = orderRepository;
    }

    [HttpGet("[action]")]
    public async Task<IEnumerable<Order>> GetOrdersAsync()
    {
      IEnumerable<Order> orders = await _orderRepository.GetItemsAsync(x => !x.IsComplete);
      return orders;
    }

    [HttpPut("[action]")]
    public async Task<IActionResult> UpdateOrderAsync([FromBody]Order order)
    {
      if (order is null)
      {
        return NotFound();
      }

      Order qwe = await _orderRepository.GetItemAsync(order.Id.ToString());
      await _orderRepository.UpdateItemAsync(order.Id.ToString(), order);

      return Ok();
    }
  }
}
