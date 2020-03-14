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


        // IDs

        public static string DialogStateId { get; } = $"{nameof(BotStateService)}.DialogState";
        public static string ConversationDataId { get; } = $"{nameof(BotStateService)}.ConversationState";
        // Accsessors 

        public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }
        public IStatePropertyAccessor<ConversationData> ConversationStateAccessor { get; set; }

        public BotStateService(ConversationState conversationState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException($"{nameof(conversationState)}");
            InitializeAccessors();
        }

        private void InitializeAccessors()
        {
            // Initialize Dialog state
            ConversationStateAccessor = ConversationState.CreateProperty<ConversationData>(ConversationDataId);
            DialogStateAccessor = ConversationState.CreateProperty<DialogState>(DialogStateId);
        }
    }
}
