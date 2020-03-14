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
                FinalStepAsync
            };

            AddDialog(new WaterfallDialog($"{nameof(MainDialog)}.mainflow",waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(MainDialog)}.email"));
            AddDialog(new TextPrompt($"{nameof(MainDialog)}.success"));
            AddDialog(new TextPrompt($"{nameof(MainDialog)}.erorr"));

            InitialDialogId = $"{nameof(MainDialog)}.mainflow";
        }

        
        public async Task<DialogTurnResult> InitializeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // First, we use the Dispatch model determine which Cognitive Service (LUIS or QnA) to use.
            var recognizerResult = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);

            // Top intent tell us which cognitive service to use
            var topIntent = recognizerResult.GetTopScoringIntent();

            var luisResult = recognizerResult.Properties["luisResult"] as LuisResult;
            var entites = luisResult.Entities;

         
            string emailAddress = "";
            string message = "";
            foreach (var entity in entites)
            {
                if (entity.Type == "EmailBody") message += entity.Entity;
                if (entity.Type == "builtin.email") emailAddress += entity.Entity;
            }

            bool isSent = SendEmail(emailAddress, message);
            if(isSent)
            {
                return await stepContext.PromptAsync($"{nameof(MainDialog)}.success",
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("The messeage sent successfully")
                    }, cancellationToken);
            }
            else
            {
                return await stepContext.PromptAsync($"{nameof(MainDialog)}.erorr",
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("There is a problem in our program. Please try later")
                    });
            }
            
        }

        public async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
        private bool IsValidEmailAddress(string message)
        {
            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Match match = regex.Match(message);
            if (match.Success) return true;
            return false;

        }

        private bool SendEmail(string emailAddress,string messageBody)
        {
            try
            {
                var fromAddress = new MailAddress("bot33103@gmail.com", "Bot");
                var toAddress = new MailAddress(emailAddress);
                const string fromPassword = "Bingo777";
                const string subject = "Sending message from Bot";
                string body = messageBody;

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                    Timeout = 20000
                };
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    smtp.Send(message);
                }
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }
    }
}
