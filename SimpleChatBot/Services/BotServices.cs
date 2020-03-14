using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChatBot.Services
{
    public class BotServices
    {
        [Obsolete]
        public BotServices(IConfiguration configuration)
        {
            // Read te setting for Congnitive services (LUIS, QnA) from appsetings.json
            Dispatch = new LuisRecognizer(new LuisApplication(
                configuration["LuisAppId"],
                configuration["LuisAPIKey"],
                $"https://{configuration["LuisAPIHostName"]}.api.cognitive.microsoft.com"),
                new LuisPredictionOptions { IncludeAllIntents = true, IncludeInstanceData = true }, true
                );
        }

        public LuisRecognizer Dispatch { get; private set; }
    }
}
