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
    private readonly GreetingsDialog _greetingsDialog;
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
      _greetingsDialog = new GreetingsDialog(_accessors.DialogStateAccessor);
      _userState = userState ?? throw new ArgumentNullException(nameof(userState));
      _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
      _cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
      _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    }

    public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
    {
      // Retrieve user data from state.
      var userData = await _accessors.UserDataAccessor.GetAsync(turnContext, () => new UserData(), cancellationToken);

      // Establish context for our dialog from the turn context.
      var _dialogContext = await _greetingsDialog.CreateContextAsync(turnContext, cancellationToken);

      if (_dialogContext.ActiveDialog == null)
      {
        if (string.IsNullOrEmpty(userData.Name) || string.IsNullOrEmpty(userData.Room))
        {
          await _dialogContext.BeginDialogAsync(GreetingsDialog.MainDialog, null, cancellationToken);
        }
        else
        {
          await this.TryRegisterOrderAsync(_dialogContext, userData);

          if (turnContext.Activity.Type == ActivityTypes.Message)
          {
            await this.SendCardsAsync(turnContext, cancellationToken, _dialogContext);
          }
        }
      }
      else
      {
        var dialogTurnResult = await _dialogContext.ContinueDialogAsync(cancellationToken);
        if (dialogTurnResult.Status == DialogTurnStatus.Complete)
        {
          userData = (UserData)dialogTurnResult.Result;

          await _accessors.UserDataAccessor.SetAsync(
              turnContext,
              userData,
              cancellationToken);

          await this.TryRegisterOrderAsync(_dialogContext, userData);

          if (turnContext.Activity.Type == ActivityTypes.Message)
          {
            await this.SendCardsAsync(turnContext, cancellationToken, _dialogContext);
          }
        }
      }

      // Persist any changes to storage.
      await _accessors.UserState.SaveChangesAsync(turnContext, false, cancellationToken);
      await _accessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
    }

    private async Task<bool> TryRegisterOrderAsync(
      DialogContext dialogContext,
      UserData userData)
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

      return true;
    }

    private async Task SendCardsAsync(
      ITurnContext turnContext,
      CancellationToken cancellationToken,
      DialogContext dialogContext)
    {
      // Add the card to our reply.
      var reply = turnContext.Activity.CreateReply();

      reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
      reply.Attachments = await _cardService.CreateAttachmentsAsync();

      await dialogContext.Context.SendActivityAsync(reply, cancellationToken);
    }
  }
}
