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
        // Supported LUIS Intents
        public const string GreetingIntent = "Greeting";
        public const string CancelIntent = "Cancel";
        public const string HelpIntent = "Help";
        public const string NoneIntent = "None";

        /// <summary>
        /// Key in the bot config (.bot file) for the LUIS instance.
        /// In the .bot file, multiple instances of LUIS can be configured.
        /// </summary>
        public static readonly string LuisConfiguration = "MarchApp";

        /// <summary>The bot's state and state property accessor objects.</summary>
        private WomanDayBotAccessors Accessors { get; }

        /// <summary>The dialog set that has the dialog to use.</summary>
        private GreetingsDialog GreetingsDialog { get; }

        private readonly ILogger<WomanDayBotBot> _logger;

        public ICardFactory _cardFactory { get; }

        private readonly OrderRepository<Order> _orderRepository;
        private readonly UserState _userState;
        private readonly ConversationState _conversationState;
        private readonly BotServices _services;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreBot"/> class.
        /// <param name="botServices">Bot services.</param>
        /// <param name="accessors">Bot State Accessors.</param>
        /// </summary>
        public WomanDayBotBot(BotServices services, UserState userState, ConversationState conversationState, 
            ILoggerFactory loggerFactory, ICardFactory cardFactory, WomanDayBotAccessors womanDayBotAccessors, OrderRepository<Order> orderRepository)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _userState = userState ?? throw new ArgumentNullException(nameof(userState));
            _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            Accessors = womanDayBotAccessors;
            // Create the greetings dialog.
            GreetingsDialog = new GreetingsDialog(Accessors.DialogStateAccessor);

            _logger = loggerFactory.CreateLogger<WomanDayBotBot>();
            _cardFactory = cardFactory;
            _orderRepository = orderRepository;
            // Verify LUIS configuration.
            if (!_services.LuisServices.ContainsKey(LuisConfiguration))
            {
                throw new InvalidOperationException($"The bot configuration does not contain a service type of `luis` with the id `{LuisConfiguration}`.");
            }
        }

        private DialogSet Dialogs { get; set; }

        /// <summary>
        /// Run every turn of the conversation. Handles orchestration of messages.
        /// <param name="turnContext">Bot Turn Context.</param>
        /// <param name="cancellationToken">Task CancellationToken.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// </summary>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // Retrieve user data from state.
            UserData userData = await Accessors.UserDataAccessor.GetAsync(turnContext, () => new UserData());

            // Establish context for our dialog from the turn context.
            DialogContext dc = await GreetingsDialog.CreateContextAsync(turnContext);

            // Handle conversation update, message, and delete user data activities.
            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.ConversationUpdate:

                    // Greet any user that is added to the conversation.
                    IConversationUpdateActivity activity = turnContext.Activity.AsConversationUpdateActivity();
                    if (activity.MembersAdded.Any(member => member.Id != activity.Recipient.Id))
                    {
                        if (userData.Name is null)
                        {
                            // If we don't already have their name, start a dialog to collect it.
                            await turnContext.SendActivityAsync("Welcome to the User Data bot.");
                            await dc.BeginDialogAsync(GreetingsDialog.MainDialog);
                        }
                        else
                        {
                            // Otherwise, greet them by name.
                            await turnContext.SendActivityAsync($"Hi {userData.Name}! Welcome back to the User Data bot.");
                        }
                    }

                    break;

                case ActivityTypes.Message:

                    // If there's a dialog running, continue it.
                    if (dc.ActiveDialog != null)
                    {
                        var dialogTurnResult = await dc.ContinueDialogAsync();
                        if (dialogTurnResult.Status == DialogTurnStatus.Complete
                            && dialogTurnResult.Result is string name
                            && !string.IsNullOrWhiteSpace(name))
                        {
                            // If it completes successfully and returns a valid name, save the name and greet the user.
                            userData.Name = name;
                            await turnContext.SendActivityAsync($"Pleased to meet you {userData.Name}.");
                        }
                    }
                    else if (userData.Name is null)
                    {
                        // Else, if we don't have the user's name yet, ask for it.
                        await dc.BeginDialogAsync(GreetingsDialog.MainDialog);
                    }
                    else
                    {
                        // Else, echo the user's message text.
                        await turnContext.SendActivityAsync($"{userData.Name} said, '{turnContext.Activity.Text}'.");
                    }

                    break;

                case ActivityTypes.DeleteUserData:

                    // Delete the user's data.
                    userData.Name = null;
                    await turnContext.SendActivityAsync("I have deleted your user data.");

                    break;
            }

            // Update the user data in the turn's state cache.
            await Accessors.UserDataAccessor.SetAsync(turnContext, userData, cancellationToken);

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
                await dc.Context.SendActivityAsync(ff);

                if(dc.Context.Activity.Value != null)
                {
                    var order = JsonConvert.DeserializeObject<Order>(dc.Context.Activity.Value.ToString());
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
                await dc.Context.SendActivityAsync(reply, cancellationToken);

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
