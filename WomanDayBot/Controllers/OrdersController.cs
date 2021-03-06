using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WomanDayBot.Models;
using WomanDayBot.Repositories;

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
      IEnumerable<Order> orders = await _orderRepository.GetItemsAsync();
      return orders;
    }

    [HttpPut("[action]")]
    public async Task<IActionResult> UpdateOrderAsync([FromBody]Order order)
    {
      if (order is null)
      {
        return BadRequest();
      }

      await _orderRepository.PatchItemAsync(order.DocumentId.ToString(), nameof(order.IsComplete), order.IsComplete);

      return Ok();
    }
  }
}
