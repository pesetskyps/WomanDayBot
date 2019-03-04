using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using WomanDayBot.Models;

namespace WomanDayBot.Dialogs
{
  /// <summary>Defines a dialog for collecting a user's name.</summary>
  public class GreetingDialog : DialogSet
  {
    public const string DialogId = "GreetingsDialogId";

    private const string NamePromt = "NamePromt";
    private const string RoomPromt = "RoomPromt";

    // Define keys for tracked values within the dialog.
    private const string NameKey = "NameKey";
    private const string RoomKey = "RoomKey";

    /// <summary>Creates a new instance of this dialog set.</summary>
    /// <param name="dialogState">The dialog state property accessor to use for dialog state.</param>
    public GreetingDialog(IStatePropertyAccessor<DialogState> dialogState)
      : base(dialogState)
    {
      var steps = new WaterfallStep[]
      {
        PromtForNameAsync,
        PromtForRoomAsync,
        EndDialogAsync
      };

      Add(new TextPrompt(NamePromt, this.UserNamePromptValidatorAsync));
      Add(new ChoicePrompt(RoomPromt));
      Add(new WaterfallDialog(DialogId, steps));
    }

    private async Task<DialogTurnResult> PromtForNameAsync(
      WaterfallStepContext stepContext,
      CancellationToken cancellationToken = default(CancellationToken))
    {
      return await stepContext.PromptAsync(
        NamePromt,
        new PromptOptions
        {
          Prompt = MessageFactory.Text("Не то, чтобы я хотел подкатить, но как тебя зовут. Принцесса?"),
          RetryPrompt = MessageFactory.Text("Да ладно, ну скажи имечко?")
        },
        cancellationToken);
    }

    /// <summary>
    /// User name validator
    /// </summary>
    /// <param name="promptContext">String that need to validate</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>True if name valid and false if not valid</returns>
    private async Task<bool> UserNamePromptValidatorAsync(
      PromptValidatorContext<string> promptContext,
      CancellationToken cancellationToken = default(CancellationToken))
    {
      if (!promptContext.Recognized.Succeeded)
      {
        await promptContext.Context.SendActivityAsync(
          "Извините, но я вас не понял. Пожалуйста, введите своё имя.",
          cancellationToken: cancellationToken);

        return false;
      }

      var value = promptContext.Recognized.Value;

      var regex = new Regex(@"(\w)+");

      if (value != null && regex.IsMatch(value))
      {
        return true;
      }

      await promptContext.Context.SendActivitiesAsync(new[]
      {
        MessageFactory.Text("К сожалению, я не могу распознать ваше имя."),
        promptContext.Options.RetryPrompt
      },
      cancellationToken);

      return false;
    }

    private async Task<DialogTurnResult> PromtForRoomAsync(
      WaterfallStepContext stepContext,
      CancellationToken cancellationToken = default(CancellationToken))
    {
      // Record the name information in the current dialog state.
      var name = (string)stepContext.Result;
      stepContext.Values[NameKey] = name;

      // Prompt for the party size. The result of the prompt is returned to the next step of the waterfall.
      return await stepContext.PromptAsync(
        RoomPromt,
        new PromptOptions
        {
          Prompt = MessageFactory.Text("Мы уже почти на одной волне. Черкани адресок: я заеду."),
          RetryPrompt = MessageFactory.Text("Да не домашний адрес. В офисе комнату напиши."),
          Choices = ChoiceFactory.ToChoices(new List<string> { "701", "702", "801", "802", "803", "806", "807", "808" })
        },
        cancellationToken);
    }

    private async Task<DialogTurnResult> EndDialogAsync(
      WaterfallStepContext stepContext,
      CancellationToken cancellationToken = default(CancellationToken))
    {
      var room = (stepContext.Result as FoundChoice).Value;
      stepContext.Values[RoomKey] = room;

      // Return the collected information to the parent context.
      var userData = new UserData
      {
        Name = (string)stepContext.Values[NameKey],
        Room = (string)stepContext.Values[RoomKey]
      };

      return await stepContext.EndDialogAsync(userData, cancellationToken);
    }
  }
}
