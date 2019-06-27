﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Scorables.Internals;
using Microsoft.Bot.Connector;

namespace Scorable.Dialogs
{
    public partial class CommonResponsesScorable : ScorableBase<IActivity, string, double>
    {
        private readonly IDialogTask task;

        public CommonResponsesScorable(IDialogTask task)
        {
            SetField.NotNull(out this.task, nameof(task), task);
        }

        protected override async Task<string> PrepareAsync(IActivity activity, CancellationToken token)
        {
            var message = activity as IMessageActivity;

            if (message != null && !string.IsNullOrWhiteSpace(message.Text))
            {
                var msg = message.Text.ToLowerInvariant();

                if (msg == "hello" || msg == "thank you" || msg == "goodbye")
                {
                    return message.Text;
                }
                else if (msg.Contains("help"))
                {
                    return message.Text;
                }
                else
                {
                    //call another dialog
                }
            }

            return null;
        }

        protected override bool HasScore(IActivity item, string state)
        {
            return state != null;
        }

        protected override double GetScore(IActivity item, string state)
        {
            return 1.0;
        }

        protected override async Task PostAsync(IActivity item, string state, CancellationToken token)
        {
            var message = item as IMessageActivity;
            IDialog<IMessageActivity> interruption = null;
            if (message != null)
            {
                var incomingMessage = message.Text.ToLowerInvariant();
                var messageToSend = string.Empty;

                if (incomingMessage.Contains("help"))
                {
                    var commonResponsesDialog = new CommonResponsesDialog((Activity)message);
                    interruption = commonResponsesDialog.Void<object, IMessageActivity>();
                }
                else
                {
                    if (incomingMessage == "hello")
                        messageToSend = "Hi! I am a bot";

                    if (incomingMessage == "thank you")
                        messageToSend = "You are welcome!";

                    if (incomingMessage == "goodbye")
                        messageToSend = "See you later";
                    var commonResponsesDialog = new CommonResponsesDialog(messageToSend);
                     interruption = commonResponsesDialog.Void<object, IMessageActivity>();
                }
                this.task.Call(interruption, null);
                await this.task.PollAsync(token);
            }
        }

        protected override Task DoneAsync(IActivity item, string state, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}