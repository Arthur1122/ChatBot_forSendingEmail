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
    public class SendEmailDialog : ComponentDialog
    {

        private readonly BotStateService _botStateService;
        private readonly BotServices _botServices;
        private DialogData _dialogData;

        public SendEmailDialog(string dialogId,BotStateService botStateService, BotServices botServices) : base (dialogId)
        {
            _botStateService = botStateService ?? throw new ArgumentNullException($"{nameof(botStateService)}");
            _botServices = botServices ?? throw new ArgumentNullException($"{nameof(botServices)}");

            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            var waterfallSteps = new WaterfallStep[]
            {
                InitializeStepAsync,
                CheckRecipientEmailStepAsync,
                FinalStepAsync
            };

            AddDialog(new WaterfallDialog($"{nameof(SendEmailDialog)}.mainflow", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(SendEmailDialog)}.checkEmail", CheckEmailAddressValidation));
            AddDialog(new TextPrompt($"{nameof(SendEmailDialog)}.success"));
            AddDialog(new TextPrompt($"{nameof(SendEmailDialog)}.erorr"));

            InitialDialogId = $"{nameof(SendEmailDialog)}.mainflow";
        }


        private async Task<DialogTurnResult> InitializeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // creating new instance of dialog data
            _dialogData = new DialogData();

            // First, we use the Dispatch model determine which Cognitive Service (LUIS or QnA) to use.
            var recognizerResult = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);

            var luisResult = recognizerResult.Properties["luisResult"] as LuisResult;
            var entites = luisResult.Entities;

            foreach (var entity in entites)
            {
                if (entity.Type == "EmailBody")
                {
                    _dialogData.Message += " " + entity.Entity;
                }
                else if(entity.Type == "RecipientName")
                {
                    _dialogData.Recipient += entity.Entity;
                }
                else if(entity.Type == "builtin.email")
                {
                    _dialogData.Recipient += entity.Entity;
                    
                }
            }

            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> CheckRecipientEmailStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            bool isValidEmail = IsValidEmailAddress(_dialogData.Recipient);
            if(isValidEmail)
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }
            else
            {
                return await stepContext.PromptAsync($"{nameof(SendEmailDialog)}.checkEmail",
                 new PromptOptions
                 {
                     Prompt = MessageFactory.Text($"Enter a {_dialogData.Recipient}'s email"),
                     RetryPrompt = MessageFactory.Text("Enter valid email address")
                 }, cancellationToken);
            }
            
        }

        public async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if(stepContext.Result != null)
            {
                _dialogData.Recipient = (string)stepContext.Result;
            }
            bool isSent = SendEmail(_dialogData.Recipient, _dialogData.Message);
            if (isSent)
            {
                 await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Message sent successfuly!")));
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("There is a problem in our program. Please try later"))); 
            }
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private bool IsValidEmailAddress(string message)
        {
            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Match match = regex.Match(message);
            if (match.Success) return true;
            return false;

        }

        private Task<bool> CheckEmailAddressValidation(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var valid = false;

            if (promptContext.Recognized.Succeeded)
            {
                valid = Regex.Match(promptContext.Recognized.Value,
                    @"^[-!#$%&'*+/0-9=?A-Z^_a-z{|}~](\.?[-!#$%&'*+/0-9=?A-Z^_a-z{|}~])*@[a-zA-Z](-?[a-zA-Z0-9])*(\.[a-zA-Z](-?[a-zA-Z0-9])*)+$").Success;
            }
            return Task.FromResult(valid);
        }
        private bool SendEmail(string emailAddress, string messageBody)
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
