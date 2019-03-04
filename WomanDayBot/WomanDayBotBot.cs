using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WomanDayBot.Dialogs;
using WomanDayBot.Models;
using WomanDayBot.Repositories;
using WomanDayBot.Services;

namespace WomanDayBot
{
  /// <summary>
  /// Main entry point and orchestration for bot.
  /// </summary>
  public class WomanDayBotBot : IBot
  {
    private readonly ILogger<WomanDayBotBot> _logger;
    private readonly WomanDayBotAccessors _accessors;
    private readonly GreetingDialog _greetingDialog;
    private readonly CategoryChooseDialog _categoryChooseDialog;
    private readonly UserState _userState;
    private readonly ConversationState _conversationState;
    private readonly ICardService _cardService;
    private readonly OrderRepository _orderRepository;

    public WomanDayBotBot(
      ILoggerFactory loggerFactory,
      WomanDayBotAccessors womanDayBotAccessors,
      UserState userState,
      ConversationState conversationState,
      ICardService cardService,
      OrderRepository orderRepository)
    {
      _logger = loggerFactory.CreateLogger<WomanDayBotBot>();
      _accessors = womanDayBotAccessors ?? throw new ArgumentNullException(nameof(womanDayBotAccessors));
      _userState = userState ?? throw new ArgumentNullException(nameof(userState));
      _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
      _cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
      _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));

      _greetingDialog = new GreetingDialog(_accessors.DialogStateAccessor);
      _categoryChooseDialog = new CategoryChooseDialog(_accessors.DialogStateAccessor);
    }

    public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
    {
      // Retrieve user data from state.
      var userData = await _accessors.UserDataAccessor.GetAsync(turnContext, () => new UserData(), cancellationToken);
      var category = await _accessors.OrderCategoryAccessor.GetAsync(turnContext, () => OrderCategory.None, cancellationToken);

      // Establish context for our dialog from the turn context.
      var greetingContext = await _greetingDialog.CreateContextAsync(turnContext, cancellationToken);

      var greetingDialogTurnResult = await greetingContext.ContinueDialogAsync(cancellationToken);

      if (greetingDialogTurnResult.Status == DialogTurnStatus.Empty)
      {
        if (string.IsNullOrEmpty(userData.Name) || string.IsNullOrEmpty(userData.Room))
        {
          await greetingContext.BeginDialogAsync(GreetingDialog.DialogId, null, cancellationToken);
        }
        else
        {
          // Begin category choice dialog
          await greetingContext.Context.SendActivityAsync("Begin category choice dialog", cancellationToken: cancellationToken);
        }
      }
      else if (greetingDialogTurnResult.Status == DialogTurnStatus.Complete)
      {
        userData = (UserData)greetingDialogTurnResult.Result;

        await _accessors.UserDataAccessor.SetAsync(
          turnContext,
          userData,
          cancellationToken);

        // Begin category choice dialog
        await greetingContext.Context.SendActivityAsync("Begin category choice dialog", cancellationToken: cancellationToken);
      }

      // Persist any changes to storage.
      await _accessors.UserState.SaveChangesAsync(turnContext, false, cancellationToken);
      await _accessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
    }

    private async Task<bool> TryRegisterOrderAsync(
      DialogContext dialogContext,
      UserData userData,
      CancellationToken cancellationToken)
    {
      if (dialogContext.Context.Activity.Value == null)
      {
        return false;
      }

      var order = JsonConvert.DeserializeObject<Order>(dialogContext.Context.Activity.Value.ToString());
      order.Id = Guid.NewGuid();
      order.RequestTime = DateTime.Now;
      order.UserData = userData;

      var orderDoc = await _orderRepository.CreateItemAsync(order);
      _logger.LogDebug("Created {orderDoc}", orderDoc);

      await dialogContext.Context.SendActivityAsync("Заказ принят", cancellationToken: cancellationToken);

      return true;
    }

    private async Task SendCardsAsync(
      DialogContext dialogContext,
      OrderCategory category,
      CancellationToken cancellationToken)
    {
      var reply = dialogContext.Context.Activity.CreateReply();

      reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
      reply.Attachments = await _cardService.CreateAttachmentsAsync();

      await dialogContext.Context.SendActivityAsync(reply, cancellationToken);
    }
  }
}
