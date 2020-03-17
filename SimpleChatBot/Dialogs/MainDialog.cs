using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using SimpleChatBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleChatBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly BotStateService _botStateService;
        private readonly BotServices _botServices;
        public MainDialog( BotStateService botStateService, BotServices botServices) : base(nameof(MainDialog))
        {
            _botStateService = botStateService ?? throw new ArgumentNullException($"{nameof(botStateService)}");
            _botServices = botServices ?? throw new ArgumentNullException($"{nameof(botServices)}");

            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            // Create waterfall steps

            var waterfallSteps = new WaterfallStep[]
            {
                InitializeStepAsync,
            };

            AddDialog(new WaterfallDialog($"{nameof(MainDialog)}.mainflow",waterfallSteps));
            AddDialog(new SendEmailDialog($"{nameof(MainDialog)}.sendEmail",_botStateService,_botServices));
            AddDialog(new GreetingDialog($"{nameof(MainDialog)}.greeting", _botStateService));
            
            InitialDialogId = $"{nameof(MainDialog)}.mainflow";
        }

        private async Task<DialogTurnResult> InitializeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // First, we use the Dispatch model determine which Cognitive Service (LUIS or QnA) to use.
            var recognizerResult = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);

            // Top intent tell us which cognitive service to use
            var topIntent = recognizerResult.GetTopScoringIntent();

            var luisResult = recognizerResult.Properties["luisResult"] as LuisResult;
            var entites = luisResult.Entities;

            switch (topIntent.intent)
            {
                case "GreetingIntent":
                    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.greeting", null, cancellationToken);
                case "SendEmialIntent":
                    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.sendEmail", null, cancellationToken);
                case "RecipientNameIntent":
                    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.sendEmail", null, cancellationToken);
                default:
                    await  stepContext.Context.SendActivityAsync(MessageFactory.Text($"I am sorry I dont know what you mean."), cancellationToken);
                    break;
            }
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

    }
}
