using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WomanDayBot.Orders;
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
    private const string CategoryPromt = "categoryPromt";

    // Define keys for tracked values within the dialog.
    private const string Name = "name";
    private const string Room = "room";
    private const string Category = "category";

    /// <summary>Creates a new instance of this dialog set.</summary>
    /// <param name="dialogState">The dialog state property accessor to use for dialog state.</param>
    public GreetingsDialog(IStatePropertyAccessor<DialogState> dialogState)
        : base(dialogState)
    {
      // Add the text prompt to the dialog set.
      Add(new TextPrompt(NamePromt, UserNamePromptValidatorAsync));
      Add(new ChoicePrompt(CategoryPromt));
      Add(new ChoicePrompt(RoomPromt));

      var steps = new WaterfallStep[]
      {
                PromtForNameAsync,
                PromtForCategoryAsync,
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

      Regex regex = new Regex(@"^[A-Za-zа-яА-Я0-9]+(?:[ _@.][A-Za-zа-яА-Я0-9]+)");

      if (value != null)
      {
        if (regex.IsMatch(value))
        {
          return true;
        }
      }

      await promptContext.Context.SendActivitiesAsync(
                  new[]
                  {
                    MessageFactory.Text("К сожалению, я не могу распознать ваше имя."),
                    promptContext.Options.RetryPrompt,
                  },
                  cancellationToken);
      return false;
    }

    private async Task<DialogTurnResult> PromtForCategoryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
      // Record the name information in the current dialog state.
      var name = (string)stepContext.Result;
      stepContext.Values[Name] = name;
      // Prompt for the party size. The result of the prompt is returned to the next step of the waterfall.
      return await stepContext.PromptAsync(
        CategoryPromt,
        new PromptOptions
        {
          Prompt = MessageFactory.Text($"{name}, Пожалуйста выберите категорию"),
          RetryPrompt = MessageFactory.Text($"{name}, Выбери категорию, блин!"),
          Choices = ChoiceFactory.ToChoices(Enum.GetNames(typeof(OrderCategory)))
        },
        cancellationToken);
    }

    private async Task<DialogTurnResult> PromtForRoomAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken = default(CancellationToken))
    {
      // Record the name information in the current dialog state.
      var category = (stepContext.Result as FoundChoice).Value;
      stepContext.Values[Category] = category;

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
      Enum.TryParse<OrderCategory>(stepContext.Values[Category].ToString(), true, out var category);
      // Return the collected information to the parent context.
      var userData = new UserData
      {
        Name = (string)stepContext.Values[Name],
        Room = (string)stepContext.Values[Room],
        OrderCategory = category
      };

      return await stepContext.EndDialogAsync(userData, cancellationToken);
    }
  }
}
