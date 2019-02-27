using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WomanDayBot.Users;

namespace WomanDayBot
{
    /// <summary>Defines a dialog for collecting a user's name.</summary>
    public class GreetingsDialog : DialogSet
    {
        /// <summary>The ID of the main dialog.</summary>
        public const string MainDialog = "main";

        private const string NamePromt = "namePromt";
        private const string RoomPromt = "roomPromt";

        // Define keys for tracked values within the dialog.
        private const string Name = "name";
        private const string Room = "room";

        /// <summary>Creates a new instance of this dialog set.</summary>
        /// <param name="dialogState">The dialog state property accessor to use for dialog state.</param>
        public GreetingsDialog(IStatePropertyAccessor<DialogState> dialogState)
            : base(dialogState)
        {
            // Add the text prompt to the dialog set.
            Add(new TextPrompt(NamePromt));
            Add(new ChoicePrompt(RoomPromt));

            var steps = new WaterfallStep[]
            {
                PromtForNameAsync,
                PromtForRoomAsync,
                AcknowledgeUserDataAsync
            };
            // Define the main dialog and add it to the set.
            Add(new WaterfallDialog(MainDialog, steps));
        }

        private async Task<DialogTurnResult> PromtForNameAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Prompt for the party size. The result of the prompt is returned to the next step of the waterfall.
            return await stepContext.PromptAsync(
                NamePromt,
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Не то, чтобы я хотел подкатить, но как тебя зовут. Принцесса?"),
                    RetryPrompt = MessageFactory.Text("Да ладно, ну скажи имечко?")
                },
                cancellationToken);
        }

        private async Task<DialogTurnResult> PromtForRoomAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Record the name information in the current dialog state.
            var name = (string)stepContext.Result;
            stepContext.Values[Name] = name;

            // Prompt for the party size. The result of the prompt is returned to the next step of the waterfall.
            return await stepContext.PromptAsync(
                RoomPromt,
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Мы уже почти на одной волне. Черкани адресок: я заеду."),
                    RetryPrompt = MessageFactory.Text("Да не домашний адрес. В офисе комнату напиши"),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "701", "702", "801", "802", "803", "806", "807", "808" }),
                },
                cancellationToken);
        }

        private async Task<DialogTurnResult> AcknowledgeUserDataAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Record the party size information in the current dialog state.
            var room = (stepContext.Result as FoundChoice).Value;
            stepContext.Values[Room] = room;

            // Send an acknowledgement to the user.
            await stepContext.Context.SendActivityAsync("Ну теперь-то мы с тобой зажжем!", cancellationToken: cancellationToken);

            // Return the collected information to the parent context.
            var userData = new UserData
            {
                Name = Name,
                Room = Room
            };

            return await stepContext.EndDialogAsync(userData, cancellationToken);
        }
    }
}
