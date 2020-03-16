using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using SimpleChatBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChatBot.Services
{
    public class BotStateService
    {
        // Variables
        public ConversationState ConversationState { get;}
        public UserState UserState { get; }

        // IDs

        public static string DialogStateId { get; } = $"{nameof(BotStateService)}.DialogState";
        public static string ConversationDataId { get; } = $"{nameof(BotStateService)}.ConversationState";
        public static string UserDataId { get; } = $"{nameof(BotStateService)}.UserState";

        // Accsessors 

        public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }
        public IStatePropertyAccessor<ConversationData> ConversationStateAccessor { get; set; }
        public IStatePropertyAccessor<UserProfile> UserStateAccessor { get; set; }

        public BotStateService(ConversationState conversationState,UserState userState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException($"{nameof(conversationState)}");
            UserState = userState ?? throw new ArgumentNullException($"{nameof(userState)}");
            InitializeAccessors();
        }

        private void InitializeAccessors()
        {
            // Initialize Dialog state
            ConversationStateAccessor = ConversationState.CreateProperty<ConversationData>(ConversationDataId);
            DialogStateAccessor = ConversationState.CreateProperty<DialogState>(DialogStateId);
            // Initialize User state
            UserStateAccessor = UserState.CreateProperty<UserProfile>(UserDataId);
        }
    }
}
