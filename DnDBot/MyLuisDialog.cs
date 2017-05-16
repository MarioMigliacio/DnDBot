using DnD.Classes.CharacterClasses;
using DnD.Classes.HeroFeats;
using DnD.Classes.HeroSkills;
using DnD.Classes.HeroSpecials;
using DnD.Classes.Player;
using DnD.Dice;
using DnD.Enums.Alignment;
using DnD.Enums.ClassFeats;
using DnD.Enums.ClassSkills;
using DnD.Enums.ClassSpecials;
using DnD.Enums.ClassTypes;
using DnD.Enums.Races;
using DnD.Enums.SavingThrows;
using DnD.Enums.Stats;
using DnDBot.HeroInformation;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private string _name = string.Empty;
        private string _gender = string.Empty;
        List<string> genderOptions = new List<string>(new string[] { "male", "female", "other", "unknown" });

        #region Player Default Intent

        /// <summary>
        /// The DefaultHero method is fired when LUIS recognizes terms pertinant to the Defualt Intent.
        /// </summary>
        /// <param name="context">The <see cref="IDialogContext>"/> which is passed in.</param>
        /// <param name="result">The <see cref="LuisResult>"/> which is passed in.</param>
        /// <returns>Method awaits the completion of the Posting process.</returns>
        [LuisIntent("Default")]
        public async Task DefaultHero(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I recognized an intent to take Default action.");

            PromptDialog.Confirm(context, DefaultHero_Dialog, "Clicking Yes will initialize your hero creation process (any set properties will be forgotten) Continue??");
        }

        /// <summary>
        /// The actual setting of our Player object to default is done through this private asynchronous helper method.
        /// </summary>
        /// <param name="context">The <see cref="IDialogContext>"/> which is passed in.</param>
        /// <param name="result">The <see cref="IAwaitable{bool}>"/> which is passed in.</param>
        /// <returns>Method awaits the completion of the Posting process.</returns>
        private async Task DefaultHero_Dialog(IDialogContext context, IAwaitable<bool> result)
        {
            if (await result)
            {
                // ensure the hero is in a good state for building off of: default all the properties and collections.
                Player.GetHero = null;
                Player.Name = null;
                Player.Gender = null;
                Player.DesiredAlign = Alignment.None;
                Player.DesiredClass = ClassType.None;
                Player.DesiredRace = RaceType.None;
                Player.StatsContainer = new Dictionary<Stats, int>();
                PlayerFeats.FeatsContainer = new List<BaseFeat>();
                PlayerSkills.SkillsContainer = new List<BaseSkill>();

                await context.PostAsync("All the Properties of the hero have been reset to safe default values. Ready for your next input!");
            }
            else
            {
                await context.PostAsync("Nevermind! We will keep your currently set attributes for this hero (if any were set through commands)");
            }

            context.Wait(MessageReceived);
        }

        #endregion

        #region Player Naming Intent

        /// <summary>
        /// This NamePlayer method is called when LUIS recognizes the intent 'PlayerName' being called.
        /// Intended usage: name 'desired heroname' barring that LUIS can decipher heroname correctly :P 
        /// This method also prompts a yes/no dialog to the user to lock in the name.
        /// </summary>
        /// <param name="context">The <see cref="IDialogContext>"/> which is passed in.</param>
        /// <param name="result">The <see cref="IAwaitable{bool}>"/> which is passed in.</param>
        /// <returns>Method awaits the completion of the Posting process.</returns>
        [LuisIntent("PlayerName")]
        public async Task NamePlayer(IDialogContext context, LuisResult result)
        {
            EntityRecommendation reco;
            
            if (result.TryFindEntity("Name", out reco))
            {
                _name = reco.Entity;
                PromptDialog.Confirm(context, NamePlayer_Dialog, $"Would you like your hero name to be {_name}?");                
            }
            else
            {
                await context.PostAsync("Sorry, I did not recognize a Name value. Maybe I can learn it for next time! Usage: name [value]");

                context.Wait(MessageReceived);
            }           
        }

        /// <summary>
        /// The actual naming of our Player object is done through this private asynchronous helper method.
        /// </summary>
        /// <param name="context">The <see cref="IDialogContext>"/> which is passed in.</param>
        /// <param name="result">The <see cref="IAwaitable{bool}>"/> which is passed in.</param>
        /// <returns>Method awaits the completion of the Posting process.</returns>
        private async Task NamePlayer_Dialog(IDialogContext context, IAwaitable<bool> result)
        {
            if (await result)
            {
                Player.Name = _name;
                await context.PostAsync($"So your desired name '{Player.Name}' has been set successfully.");
            }
            else
            {
                await context.PostAsync("Well, you can set your name any time by trying the 'name' command, followed by the desired first name of your hero.");
            }

            context.Wait(MessageReceived);
        }

        #endregion

        #region Player Gender Intent

        /// <summary>
        /// This PlayerGender method is called when LUIS recognizes the intent 'PlayerGender' being called.
        /// Intended usage: gender 'desired gender' accepting options: male, female, other, and unknown.
        /// This method also prompts a yes/no dialog to the user to lock in the name.
        /// </summary>
        /// <param name="context">The <see cref="IDialogContext>"/> which is passed in.</param>
        /// <param name="result">The <see cref="IAwaitable{bool}>"/> which is passed in.</param>
        /// <returns>Method awaits the completion of the Posting process.</returns>
        [LuisIntent("PlayerGender")]
        public async Task PlayerGender(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I recognized an intent to assign hero gender.");

            PromptOptions<string> options = new PromptOptions<string>(
                "What gender would you like your hero to be.",
                "That was not a valid option.",
                "You are being silly!",
                genderOptions,
                2);

            PromptDialog.Choice(context, PlayerGender_Dialog, options);
        }

        /// <summary>
        /// The actual setting of our Player objects gender is done through this private asynchronous helper method.
        /// </summary>
        /// <param name="context">The <see cref="IDialogContext>"/> which is passed in.</param>
        /// <param name="result">The <see cref="IAwaitable{bool}>"/> which is passed in.</param>
        /// <returns>Method awaits the completion of the Posting process.</returns>
        private async Task PlayerGender_Dialog(IDialogContext context, IAwaitable<string> result)
        {
            string genders = await result;

            if (genderOptions.Contains(genders))
            {
                Player.Gender = genders;
                await context.PostAsync($"So your desired gender: '{Player.Gender}' has been set successfully.");
            }
            else
            {
                await context.PostAsync($"The gender option: {genders} was incorrect, please use one of the options.");
            }

            context.Wait(MessageReceived);
        }

        #endregion

        #region Player Help Intent

        /// <summary>
        /// The HelpProcess method is fired when LUIS recognizes terms pertinant to the Help Intent.
        /// </summary>
        /// <param name="context">The <see cref="IDialogContext>"/> which is passed in.</param>
        /// <param name="result">The <see cref="LuisResult>"/> which is passed in.</param>
        /// <returns>Method awaits the completion of the Posting process.</returns>
        [LuisIntent("Help")]
        public async Task HelpProcess(IDialogContext context, LuisResult result)
        {            
            await context.PostAsync("Lets start from these basic hero fundamentals:");

            if (Player.Name == null)
            {
                await context.PostAsync("A name must be set. Use the command 'name' followed by the desired first name of your hero.");
            }

            if (Player.Gender == null)
            {
                await context.PostAsync("A gender must be set. Use the command 'gender set' to walk through this process.");
            }

            if (Player.DesiredAlign == Alignment.None)
            {
                await context.PostAsync("An alignment must be set. Use the command 'align set' to walk through this process.");
            }

            if (Player.DesiredClass == ClassType.None)
            {
                await context.PostAsync("A Player Class must be set. Use the command 'class set' to walk through this process.");
            }

            if (Player.DesiredRace == RaceType.None)
            {
                await context.PostAsync("A Player Race must be set. Use the command 'race set' to walk through this process.");
            }

            if (Player.StatsContainer.Count < 6)
            {
                await context.PostAsync("The Players Stats must be set. Use the command 'rollstats' to walk through this process.");
            }

            if (PlayerFeats.FeatsContainer.Count == 0)
            {
                await context.PostAsync("The Players Feats must be set. Use the command 'feat set' to walk through this process.");
            }

            if (PlayerSkills.SkillsContainer.Count == 0)
            {
                await context.PostAsync("The Players Skills must be set. Use the command 'skill set' to walk through this process.");
            }
            
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

        #endregion
    }
}