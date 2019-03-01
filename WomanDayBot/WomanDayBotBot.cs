﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WomanDayBot.Orders;
using WomanDayBot.Users;

namespace WomanDayBot
{
  /// <summary>
  /// Main entry point and orchestration for bot.
  /// </summary>
  public class WomanDayBotBot : IBot
  {
    /// <summary>The dialog set that has the dialog to use.</summary>
    private GreetingsDialog _greetingsDialog;

    private readonly WomanDayBotAccessors _accessors;
    private readonly ILogger<WomanDayBotBot> _logger;
    private readonly ICardFactory _cardFactory;
    private readonly OrderRepository<Order> _orderRepository;
    private readonly UserState _userState;
    private readonly ConversationState _conversationState;
    private readonly BotServices _services;

    public WomanDayBotBot(
      BotServices services, 
      UserState userState, 
      ConversationState conversationState,
      WomanDayBotAccessors womanDayBotAccessors,
      ILoggerFactory loggerFactory, 
      ICardFactory cardFactory, 
      OrderRepository<Order> orderRepository)
    {
      _services = services ?? throw new ArgumentNullException(nameof(services));
      _userState = userState ?? throw new ArgumentNullException(nameof(userState));
      _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
      _accessors = womanDayBotAccessors ?? throw new ArgumentNullException(nameof(_accessors));
      _greetingsDialog = new GreetingsDialog(_accessors.DialogStateAccessor);
      _logger = loggerFactory.CreateLogger<WomanDayBotBot>();
      _cardFactory = cardFactory ?? throw new ArgumentNullException(nameof(_cardFactory));
      _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(_orderRepository));
    }

    public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
    {
      // Retrieve user data from state.
      var userData = await _accessors.UserDataAccessor.GetAsync(turnContext, () => new UserData(), cancellationToken);

      // Establish context for our dialog from the turn context.
      var dialogContext = await _greetingsDialog.CreateContextAsync(turnContext, cancellationToken);

      if (dialogContext.ActiveDialog == null)
      {
        if (string.IsNullOrEmpty(userData.Name) || string.IsNullOrEmpty(userData.Room))
        {
          await dialogContext.BeginDialogAsync(GreetingsDialog.MainDialog, null, cancellationToken);
        }
        else
        {
          await this.SendCardsOrRegisterOrderAsync(turnContext, cancellationToken, dialogContext, userData);
        }
      }
      else
      {
        var dialogTurnResult = await dialogContext.ContinueDialogAsync(cancellationToken);
        if (dialogTurnResult.Status == DialogTurnStatus.Complete)
        {
          userData = (UserData)dialogTurnResult.Result;

          await _accessors.UserDataAccessor.SetAsync(
              turnContext,
              userData,
              cancellationToken);

          await this.SendCardsOrRegisterOrderAsync(turnContext, cancellationToken, dialogContext, userData);
        }
      }

      // Persist any changes to storage.
      await _accessors.UserState.SaveChangesAsync(turnContext, false, cancellationToken);
      await _accessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
    }

    private async Task SendCardsOrRegisterOrderAsync(
      ITurnContext turnContext, 
      CancellationToken cancellationToken,
       DialogContext dialogContext, 
       UserData userData)
    {
      if (dialogContext.Context.Activity.Value != null)
      {
        var order = JsonConvert.DeserializeObject<Order>(dialogContext.Context.Activity.Value.ToString());
        order.Id = Guid.NewGuid();
        order.RequestTime = DateTime.Now;
        order.UserData = userData;

        var orderDoc = await _orderRepository.CreateItemAsync(order);
        _logger.LogDebug("Created {orderDoc}", orderDoc);
      }

      if (turnContext.Activity.Type == ActivityTypes.Message)
      {
        // Add the card to our reply.
        var reply = turnContext.Activity.CreateReply();
        reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
        reply.Attachments = await _cardFactory.CreateAttachmentsAsync();

        await dialogContext.Context.SendActivityAsync(reply, cancellationToken);
      }
    }
  }
}
