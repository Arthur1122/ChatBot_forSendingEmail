using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChatBot.Models
{
    public class ConversationData
    {
        // Track wheter we have already asked the user's name
        public bool PromptedUserForName { get; set; } = false;
    }
}
