using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using SimpleChatBot.Extensions;
using SimpleChatBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleChatBot.Bots
{
    public class DialogBot <T> : ActivityHandler where T : Dialog
    {
        private readonly Dialog _dialog;
        private readonly BotStateService _botStateService;
        public DialogBot(BotStateService botStateService, T dialog)
        {
            _botStateService = botStateService ?? throw new ArgumentNullException($"{nameof(botStateService)}");
            _dialog = dialog ?? throw new ArgumentNullException($"{nameof(dialog)}");
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Run the dialog with the new message Activity
            await _dialog.Run(turnContext, _botStateService.DialogStateAccessor, cancellationToken);
        }
    }
}
