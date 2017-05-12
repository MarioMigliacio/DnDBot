using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace DnDBot
{
    /// <summary>
    /// MyLuisDialog is a class that will be used to guide the conversations while the character creation
    /// process is worked out by the robot.
    /// </summary>
    [LuisModel("abe8a7e9-a2d9-41ef-9d5b-537b4952041a", "47e537014c5c4a83b1feac59e50767d2")]
    [Serializable]
    public class MyLuisDialog : LuisDialog<object>
    {
        /// <summary>
        /// The StartCreationProcess method is fired when LUIS recognizes terms pertinant to the Create Intent.
        /// </summary>
        /// <param name="context">The <see cref="IDialogContext>"/> which is passed in.</param>
        /// <param name="result">The <see cref="LuisResult>"/> which is passed in.</param>
        /// <returns>Method awaits the completion of the Posting process.</returns>
        [LuisIntent("Create")]
        public async Task StartCreationProcess(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I have been triggered to begin a new creation task!");
            context.Wait(MessageReceived);
        }

        /// <summary>
        /// The HelpProcess method is fired when LUIS recognizes terms pertinant to the Help Intent.
        /// </summary>
        /// <param name="context">The <see cref="IDialogContext>"/> which is passed in.</param>
        /// <param name="result">The <see cref="LuisResult>"/> which is passed in.</param>
        /// <returns>Method awaits the completion of the Posting process.</returns>
        [LuisIntent("Help")]
        public async Task HelpProcess(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Please use the terms: go, create, new, make, or start to begin the creation process!");
            context.Wait(MessageReceived);
        }

        /// <summary>
        /// The None method is fired when LUIS recognizes terms pertinant to the None Intent.
        /// </summary>
        /// <param name="context">The <see cref="IDialogContext>"/> which is passed in.</param>
        /// <param name="result">The <see cref="LuisResult>"/> which is passed in.</param>
        /// <returns>Method awaits the completion of the Posting process.</returns>
        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"I did not recognize the command as valid.");
            await HelpProcess(context, result);
            context.Wait(MessageReceived);
        }
    }
}