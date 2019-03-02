// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private WomanDayBotAccessors Accessors { get; }
        /// <summary>The dialog set that has the dialog to use.</summary>
        private GreetingsDialog _greetingsDialog;
        private readonly ILogger<WomanDayBotBot> _logger;
        public ICardFactory _cardFactory { get; }
        private readonly OrderRepository<Order> _orderRepository;
        private readonly UserState _userState;
        private readonly ConversationState _conversationState;
        private readonly BotServices _services;

        public WomanDayBotBot(BotServices services, UserState userState, ConversationState conversationState,
            ILoggerFactory loggerFactory, ICardFactory cardFactory, WomanDayBotAccessors womanDayBotAccessors, OrderRepository<Order> orderRepository)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _userState = userState ?? throw new ArgumentNullException(nameof(userState));
            _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            Accessors = womanDayBotAccessors;
            _greetingsDialog = new GreetingsDialog(Accessors.DialogStateAccessor);
            _logger = loggerFactory.CreateLogger<WomanDayBotBot>();
            _cardFactory = cardFactory;
            _orderRepository = orderRepository;
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // Retrieve user data from state.
            UserData userData = await Accessors.UserDataAccessor.GetAsync(turnContext, () => new UserData());

            // Establish context for our dialog from the turn context.
            var _dialogContext = await _greetingsDialog.CreateContextAsync(turnContext, cancellationToken);

            if (_dialogContext.ActiveDialog == null)
            {
                if ((userData.Name == null) || userData.Room == null)
                {
                    await _dialogContext.BeginDialogAsync("main", null, cancellationToken);
                }
                else
                {
                    await SendCardsOrRegisterOrder(turnContext, cancellationToken, _dialogContext, userData);
                }
            }
            else
            {
                var dialogTurnResult = await _dialogContext.ContinueDialogAsync(cancellationToken);
                if (dialogTurnResult.Status is DialogTurnStatus.Complete)
                {
                    userData = (UserData)dialogTurnResult.Result;

                    await Accessors.UserDataAccessor.SetAsync(
                        turnContext,
                        userData,
                        cancellationToken);

                    await SendCardsOrRegisterOrder(turnContext, cancellationToken, _dialogContext, userData);
                }
                  }

            // Persist any changes to storage.
            await Accessors.UserState.SaveChangesAsync(turnContext, false, cancellationToken);
            await Accessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        private async Task SendCardsOrRegisterOrder(ITurnContext turnContext, CancellationToken cancellationToken,
           DialogContext dialogContext, UserData userData)
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
                reply.Attachments = await _cardFactory.WithCategory(userData.OrderCategory).CreateAsync();
                await dialogContext.Context.SendActivityAsync(reply, cancellationToken);
            }
        }
    }
}
