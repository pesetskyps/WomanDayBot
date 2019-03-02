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
        private readonly OrderRepository _orderRepository;
        private readonly UserState _userState;
        private readonly ConversationState _conversationState;
        private readonly BotServices _services;

        public WomanDayBotBot(BotServices services, UserState userState, ConversationState conversationState, 
            ILoggerFactory loggerFactory, ICardFactory cardFactory, WomanDayBotAccessors womanDayBotAccessors, OrderRepository orderRepository)
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
                if((userData.Name == null) || userData.Room == null)
                {
                    await _dialogContext.BeginDialogAsync("main", null, cancellationToken);
                }
                else
                {
                    await turnContext.SendActivityAsync($"Привет {userData.Name}. Повеселимся? :)",cancellationToken: cancellationToken);
                }
            }
            else
            {
                // Continue the dialog.
                var dialogTurnResult = await _dialogContext.ContinueDialogAsync(cancellationToken);

                // If the dialog completed this turn, record the reservation info.
                if (dialogTurnResult.Status is DialogTurnStatus.Complete)
                {
                    userData = (UserData)dialogTurnResult.Result;
                    await Accessors.UserDataAccessor.SetAsync(
                        turnContext,
                        userData,
                        cancellationToken);

                    await turnContext.SendActivityAsync($"Привет {userData.Name}. Повеселимся? :) Заказывай штуки-дрюки в комнату {userData.Room}. Фсе принесут, рррр", cancellationToken: cancellationToken);
                }
            }

            // Persist any changes to storage.
            await Accessors.UserState.SaveChangesAsync(turnContext, false, cancellationToken);
            await Accessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);



            ///old
            var activityOld = turnContext.Activity;

            var ff = activityOld.From.Name;
            if (activityOld.Type == ActivityTypes.Message)
            {

                _logger.LogDebug("Received the message from {name}", activityOld.From.Name);
                //return back just username
                await _dialogContext.Context.SendActivityAsync(ff);

                if(_dialogContext.Context.Activity.Value != null)
                {
                    var order = JsonConvert.DeserializeObject<Order>(_dialogContext.Context.Activity.Value.ToString());
                    order.Id = Guid.NewGuid();
                    order.RequestTime = DateTime.Now;
                    order.UserData = userData;
                    var orderDoc = await _orderRepository.CreateItemAsync(order);
                    _logger.LogDebug("Created {orderDoc}", orderDoc);
                }
                

                // Add the card to our reply.
                var reply = turnContext.Activity.CreateReply();
                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                reply.Attachments = await _cardFactory.CreateAsync();
                await _dialogContext.Context.SendActivityAsync(reply, cancellationToken);

            }
            else if (activityOld.Type == ActivityTypes.ConversationUpdate)
            {
                if (activityOld.MembersAdded != null)
                {
                    // Iterate over all new members added to the conversation.
                    foreach (var member in activityOld.MembersAdded)
                    {
                        // Greet anyone that was not the target (recipient) of this message.
                        // To learn more about Adaptive Cards,
                        // See https://aka.ms/msbot-adaptivecards for more details.
                        if (member.Id != activityOld.Recipient.Id)
                        {
                            //var welcomeCard = CreateAdaptiveCardAttachment();
                            //var caurosel = MessageFactory.Carousel(new Attachment[] { welcomeCard, welcomeCard });
                            ////var response = CreateResponse(activity, caurosel);
                            //await dc.Context.SendActivityAsync(caurosel);
                        }
                    }
                }
            }

            await _conversationState.SaveChangesAsync(turnContext);
            await _userState.SaveChangesAsync(turnContext);
        }

        // Create an attachment message response.
        private Activity CreateResponse(Activity activity, Attachment attachment)
        {
            var response = activity.CreateReply();
            response.Attachments = new List<Attachment>() { attachment };
            return response;
        }

        // Load attachment from file.
        private Attachment CreateAdaptiveCardAttachment()
        {
            string[] paths = { ".", "Dialogs", "Welcome", "Resources", "welcomeCard.json" };
            string fullPath = Path.Combine(paths);
            var adaptiveCard = File.ReadAllText(fullPath);
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
        }
    }
}
