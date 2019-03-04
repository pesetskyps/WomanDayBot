using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using WomanDayBot.Models;

namespace WomanDayBot.Dialogs
{
  public class CategoryChooseDialog : DialogSet
  {
    public const string DialogId = "CategoryChooseDialogId";
    private const string OrderCategoryPromt = "OrderCategoryPromt";

    public CategoryChooseDialog(IStatePropertyAccessor<DialogState> dialogState)
      : base(dialogState)
    {
      var steps = new WaterfallStep[]
      {
        PromtForCategoryAsync,
        EndDialogAsync
      };

      Add(new ChoicePrompt(OrderCategoryPromt));
      Add(new WaterfallDialog(DialogId, steps));
    }

    private async Task<DialogTurnResult> PromtForCategoryAsync(
      WaterfallStepContext stepContext,
      CancellationToken cancellationToken = default(CancellationToken))
    {
      // exclude None from the choices
      var categories = Enum.GetNames(typeof(OrderCategory))
        .Where(x => string.Equals(x, OrderCategory.None.ToString()) == false)
        .ToList();

      return await stepContext.PromptAsync(
        OrderCategoryPromt,
        new PromptOptions
        {
          Prompt = MessageFactory.Text("Выбирай категорию"),
          RetryPrompt = MessageFactory.Text("Повтори-ка"),
          Choices = ChoiceFactory.ToChoices(categories)
        },
        cancellationToken);
    }

    private async Task<DialogTurnResult> EndDialogAsync(
      WaterfallStepContext stepContext,
      CancellationToken cancellationToken = default(CancellationToken))
    {
      var choiceValue = (stepContext.Result as FoundChoice).Value;
      var category = Enum.Parse<OrderCategory>(choiceValue);

      return await stepContext.EndDialogAsync(category, cancellationToken);
    }
  }
}
