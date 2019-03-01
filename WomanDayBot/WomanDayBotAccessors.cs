using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using WomanDayBot.Users;

namespace WomanDayBot
{
  public class WomanDayBotAccessors
  {
    public UserState UserState { get; }

    public ConversationState ConversationState { get; }

    public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }

    public IStatePropertyAccessor<UserData> UserDataAccessor { get; set; }

    public WomanDayBotAccessors(UserState userState, ConversationState conversationState)
    {
      this.UserState = userState ?? throw new ArgumentNullException(nameof(userState));
      this.ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
    }
  }
}
