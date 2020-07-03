using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using SimpleChatBot.Models;
using SimpleChatBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleChatBot.Dialogs
{
    public class GreetingDialog : ComponentDialog
    {
        private readonly BotStateService _botStateService;

        public GreetingDialog(string dialogId,BotStateService botStateService):base(dialogId)
        {
            _botStateService = botStateService ?? throw new ArgumentNullException($"{nameof(botStateService)}");

            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            var waterfallSteps = new WaterfallStep[]
            {
                InitialStepAsync,
                FinalStepAsync
            };

            AddDialog(new WaterfallDialog($"{nameof(GreetingDialog)}.mainflow",waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(GreetingDialog)}.name"));

            InitialDialogId = $"{nameof(GreetingDialog)}.mainflow";
        }


        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _botStateService.UserStateAccessor.GetAsync(stepContext.Context, () => new UserProfile());

            if(String.IsNullOrEmpty(userProfile.Name))
            {
                return await stepContext.PromptAsync($"{nameof(GreetingDialog)}.name",
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("What's your name?")
                    });
            }
            else
            {
                return await stepContext.NextAsync();
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _botStateService.UserStateAccessor.GetAsync(stepContext.Context, () => new UserProfile());
            if(String.IsNullOrEmpty(userProfile.Name))
            {
                // seting the name from stepContext.Context
                userProfile.Name = (string)stepContext.Result;

                // Save any state changes that might have ocurrded during the turn
                await _botStateService.UserStateAccessor.SetAsync(stepContext.Context, userProfile);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Hello {userProfile.Name}. How can I help you today ?"));
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
