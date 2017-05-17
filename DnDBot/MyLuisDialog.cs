using DnD.Classes.HeroFeats;
using DnD.Classes.HeroSkills;
using DnD.Classes.Player;
using DnD.Dice;
using DnD.Enums.Alignment;
using DnD.Enums.ClassFeats;
using DnD.Enums.ClassTypes;
using DnD.Enums.Races;
using DnD.Enums.Stats;
using DnDBot.HeroInformation;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
        private readonly List<string> _genderOptions = new List<string>(new string[] { "male", "female", "other", "unknown" });
        private List<string> _raceOptions = new List<string>();
        private List<string> _alignOptions = new List<string>();
        private List<string> _classOptions = new List<string>();
        private List<string> _statOptions = new List<string>();
        private List<string> _skillOptions = new List<string>();
        private List<string> _featOptions = new List<string>();
        private Queue<int> _statRolls = new Queue<int>();
        private string _lastSkillSelected = string.Empty;
        private string _lastFeatSelected = string.Empty;

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

                _name = string.Empty;
                _gender = string.Empty;
                _raceOptions = new List<string>();
                _alignOptions = new List<string>();
                _classOptions = new List<string>();
                _statOptions = new List<string>();
                _skillOptions = new List<string>();
                _featOptions = new List<string>();
                _statRolls = new Queue<int>();

                // for testing.
                //Player.Name = "cheezus";
                //Player.Gender = "male";
                //Player.DesiredAlign = Alignment.ChaoticEvil;
                //Player.DesiredClass = ClassType.Barbarian;
                //Player.DesiredRace = RaceType.HalfOrc;
                //Player.StatsContainer = new Dictionary<Stats, int>
                //{
                //    { Stats.Charisma, 10 },
                //    { Stats.Constitution, 10 },
                //    { Stats.Dexterity, 10 },
                //    { Stats.Intellect, 10 },
                //    { Stats.Strength, 10 },
                //    { Stats.Wisdom, 10 }
                //};

                //_name = Player.Name;
                //_gender = Player.Gender;

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
                await context.PostAsync("Sorry, I did not recognize the Name value. Maybe I can learn it for next time! Usage: name [value]");

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
        /// Intended usage: gender set. Accepting options: male, female, other, and unknown.
        /// This method also prompts a list of choices to lock in the selected gender.
        /// </summary>
        /// <param name="context">The <see cref="IDialogContext>"/> which is passed in.</param>
        /// <param name="result">The <see cref="LuisResult>"/> which is passed in.</param>
        /// <returns>Method awaits the completion of the Posting process.</returns>
        [LuisIntent("PlayerGender")]
        public async Task PlayerGender(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I recognized an intent to assign hero gender.");

            PromptOptions<string> options = new PromptOptions<string>(
                "What gender would you like your hero to be.",
                "That was not a valid option.",
                "You are being silly!",
                _genderOptions,
                1);

            PromptDialog.Choice(context, PlayerGender_Dialog, options);
        }

        /// <summary>
        /// The actual setting of our Player objects' gender is done through this private asynchronous helper method.
        /// </summary>
        /// <param name="context">The <see cref="IDialogContext>"/> which is passed in.</param>
        /// <param name="result">The <see cref="IAwaitable{string}>"/> which is passed in.</param>
        /// <returns>Method awaits the completion of the Posting process.</returns>
        private async Task PlayerGender_Dialog(IDialogContext context, IAwaitable<string> result)
        {
            string genders = await result;

            if (_genderOptions.Contains(genders))
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

        #region Player Race Intent

        /// <summary>
        /// This PlayerRace method is called when LUIS recognizes the intent 'PlayerRace' being called.
        /// Intended usage: race set. Accepting any of the <see cref="RaceType"/> enumeration fields except 'None'.
        /// This method also prompts a list of choices to lock in the selected race.
        /// </summary>
        /// <param name="context">The <see cref="IDialogContext>"/> which is passed in.</param>
        /// <param name="result">The <see cref="LuisResult>"/> which is passed in.</param>
        /// <returns>Method awaits the completion of the Posting process.</returns>
        [LuisIntent("PlayerRace")]
        public async Task PlayerRace(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I recognized an intent to assign hero race.");

            foreach (RaceType race in Enum.GetValues(typeof(RaceType)))
            {
                if (race != RaceType.None)
                {
                    _raceOptions.Add(race.ToString());
                }
            }
            
            PromptOptions<string> options = new PromptOptions<string>(
                "What race would you like your hero to be.",
                "That was not a valid option.",
                "You are being silly!",
                _raceOptions,
                1);

            PromptDialog.Choice(context, PlayerRace_Dialog, options);
        }

        /// <summary>
        /// The actual setting of our Player objects' race is done through this private asynchronous helper method.
        /// </summary>
        /// <param name="context">The <see cref="IDialogContext>"/> which is passed in.</param>
        /// <param name="result">The <see cref="IAwaitable{string}>"/> which is passed in.</param>
        /// <returns>Method awaits the completion of the Posting process.</returns>
        private async Task PlayerRace_Dialog(IDialogContext context, IAwaitable<string> result)
        {
            string input = await result;
            RaceType race = RaceType.None;

            switch (input)
            {
                case "Dwarf": race = RaceType.Dwarf; break;
                case "Elf": race = RaceType.Elf; break;
                case "Gnome": race = RaceType.Gnome; break;
                case "HalfElf": race = RaceType.HalfElf; break;
                case "HalfOrc": race = RaceType.HalfOrc; break;
                case "Halfling": race = RaceType.Halfling; break;
                case "Human": race = RaceType.Human; break;
                default: race = RaceType.None; break;
            }

            if (race != RaceType.None)
            {
                Player.DesiredRace = race;
                await context.PostAsync($"So your desired race: '{Player.DesiredRace}' has been set successfully.");
            }
            else
            {
                await context.PostAsync($"The race option: {race} was incorrect, please use one of the options.");
            }

            context.Wait(MessageReceived);
        }

        #endregion

        #region Player Align Intent

        /// <summary>
        /// This PlayerAlign method is called when LUIS recognizes the intent 'PlayerAlign' being called.
        /// Intended usage: align set. Accepting any of the <see cref="Alignment"/> enumeration fields except 'None'.
        /// This method also prompts a list of choices to lock in the selected alignment.
        /// </summary>
        /// <param name="context">The <see cref="IDialogContext>"/> which is passed in.</param>
        /// <param name="result">The <see cref="LuisResult>"/> which is passed in.</param>
        /// <returns>Method awaits the completion of the Posting process.</returns>
        [LuisIntent("PlayerAlign")]
        public async Task PlayerAlign(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I recognized an intent to assign hero alignment.");

            foreach (Alignment align in Enum.GetValues(typeof(Alignment)))
            {
                if (align != Alignment.None)
                {
                    _alignOptions.Add(align.ToString());
                }
            }

            PromptOptions<string> options = new PromptOptions<string>(
                "What alignment would you like your hero to be associated with.",
                "That was not a valid option.",
                "You are being silly!",
                _alignOptions,
                1);

            PromptDialog.Choice(context, PlayerAlign_Dialog, options);
        }

        /// <summary>
        /// The actual setting of our Player objects' alignment is done through this private asynchronous helper method.
        /// </summary>
        /// <param name="context">The <see cref="IDialogContext>"/> which is passed in.</param>
        /// <param name="result">The <see cref="IAwaitable{string}>"/> which is passed in.</param>
        /// <returns>Method awaits the completion of the Posting process.</returns>
        private async Task PlayerAlign_Dialog(IDialogContext context, IAwaitable<string> result)
        {
            string input = await result;
            Alignment align = Alignment.None;

            switch (input)
            {
                case "ChaoticEvil": align = Alignment.ChaoticEvil; break;
                case "ChaoticGood": align = Alignment.ChaoticGood; break;
                case "ChaoticNeutral": align = Alignment.ChaoticNeutral; break;
                case "LawfulEvil": align = Alignment.LawfulEvil; break;
                case "LawfulGood": align = Alignment.LawfulGood; break;
                case "LawfulNeutral": align = Alignment.LawfulNeutral; break;
                case "Evil": align = Alignment.Evil; break;
                case "Good": align = Alignment.Good; break;
                case "Neutral": align = Alignment.Neutral; break;
                default: align = Alignment.None; break;
            }

            if (align != Alignment.None)
            {
                Player.DesiredAlign = align;
                await context.PostAsync($"So your desired alignment: '{Player.DesiredAlign}' has been set successfully.");
            }
            else
            {
                await context.PostAsync($"The align option: {align} was incorrect, please use one of the options.");
            }

            context.Wait(MessageReceived);
        }

        #endregion

        #region Player Class Intent

        /// <summary>
        /// This PlayerClass method is called when LUIS recognizes the intent 'PlayerClass' being called.
        /// Intended usage: class set. Accepting any of the <see cref="ClassType"/> enumeration fields except 'None' and 'Caster'.
        /// This method also prompts a list of choices to lock in the selected class.
        /// </summary>
        /// <param name="context">The <see cref="IDialogContext>"/> which is passed in.</param>
        /// <param name="result">The <see cref="LuisResult>"/> which is passed in.</param>
        /// <returns>Method awaits the completion of the Posting process.</returns>
        [LuisIntent("PlayerClass")]
        public async Task PlayerClass(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I recognized an intent to assign hero class.");

            foreach (ClassType classtype in Enum.GetValues(typeof(ClassType)))
            {
                if (classtype != ClassType.None && classtype != ClassType.Caster)
                {
                    _classOptions.Add(classtype.ToString());
                }
            }

            PromptOptions<string> options = new PromptOptions<string>(
                "What alignment would you like your hero to be associated with.",
                "That was not a valid option.",
                "You are being silly!",
                _classOptions,
                1);

            PromptDialog.Choice(context, PlayerClass_Dialog, options);
        }

        /// <summary>
        /// The actual setting of our Player objects' class is done through this private asynchronous helper method.
        /// </summary>
        /// <param name="context">The <see cref="IDialogContext>"/> which is passed in.</param>
        /// <param name="result">The <see cref="IAwaitable{string}>"/> which is passed in.</param>
        /// <returns>Method awaits the completion of the Posting process.</returns>
        private async Task PlayerClass_Dialog(IDialogContext context, IAwaitable<string> result)
        {
            string input = await result;
            ClassType classtype = ClassType.None;

            switch (input)
            {
                case "Barbarian": classtype = ClassType.Barbarian; break;
                case "Bard": classtype = ClassType.Bard; break;
                case "Cleric": classtype = ClassType.Cleric; break;
                case "Druid": classtype = ClassType.Druid; break;
                case "Fighter": classtype = ClassType.Fighter; break;
                case "Monk": classtype = ClassType.Monk; break;
                case "Paladin": classtype = ClassType.Paladin; break;
                case "Ranger": classtype = ClassType.Ranger; break;
                case "Rogue": classtype = ClassType.Rogue; break;
                case "Sorcerer": classtype = ClassType.Sorcerer; break;
                case "Wizard": classtype = ClassType.Wizard; break;
                default: classtype = ClassType.None; break;
            }

            if (classtype != ClassType.None)
            {
                Player.DesiredClass = classtype;
                await context.PostAsync($"So your desired class: '{Player.DesiredClass}' has been set successfully.");
            }
            else
            {
                await context.PostAsync($"The class option: {classtype} was incorrect, please use one of the options.");
            }

            context.Wait(MessageReceived);
        }

        #endregion

        #region Player RollStats Intent

        /// <summary>
        /// This PlayerStats method is called when LUIS recognizes the intent 'RollStats' being called.
        /// Intended usage: roll stats. Uses the Static <see cref="Dice"/> object to roll the <see cref="Player.StatsContainer"/> fields.
        /// This method also prompts a yes/no whether or not the user wants to keep the selected rolls.
        /// </summary>
        /// <param name="context">The <see cref="IDialogContext>"/> which is passed in.</param>
        /// <param name="result">The <see cref="LuisResult>"/> which is passed in.</param>
        /// <returns>Method awaits the completion of the Posting process.</returns>
        [LuisIntent("RollStats")]
        public async Task PlayerStats(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I recognized an intent to roll hero stats.");

            if (Player.StatsContainer.Count == 6)
            {
                await context.PostAsync("You have already finished this process! (use 'new' command to wipe all information)");
            }

            int sum = 0;
            List<int> storage = new List<int>();

            if ((_statRolls.Count == 0 || _statRolls.Count == 6) && Player.StatsContainer.Count != 6)
            {
                _statRolls = new Queue<int>();

                for (int i = 0; i < 6; i++)
                {
                    storage.Clear();
                    storage.Add(Dice.D6);
                    storage.Add(Dice.D6);
                    storage.Add(Dice.D6);
                    storage.Add(Dice.D6);
                    storage.Sort();     // special note: List.Sort() does ascending order. Rolling stats takes the best 3 of 4 rolls.
                    sum = storage[3] + storage[2] + storage[1];
                    _statRolls.Enqueue(sum);
                }
            }

            StringBuilder rolls = new StringBuilder();

            if (_statRolls.Count > 0)
            {
                foreach (int roll in _statRolls)
                {
                    rolls.AppendLine($" {roll} ");
                }

                PromptDialog.Confirm(context, PlayerStats_Dialog, $"Continue with the following rolls: {rolls}");
            }
            else
            {
                context.Wait(MessageReceived);
            }            
        }

        /// <summary>
        /// This intermediate method accepts a YES/NO Confirmation dialog from the user.
        /// The PlayerStats_Dialog method then forwards which possible choices are left for 
        /// the user to allocate stats into through the use of a Choice Dialog.
        /// </summary>
        /// <param name="context">The <see cref="IDialogContext>"/> which is passed in.</param>
        /// <param name="result">The <see cref="IAwaitable{bool}>"/> which is passed in.</param>
        /// <returns>Method awaits the completion of the Posting process.</returns>
        private async Task PlayerStats_Dialog(IDialogContext context, IAwaitable<bool> result)
        {
            if (await result)
            {
                if (_statOptions.Count == 0)
                {
                    foreach (Stats stat in Enum.GetValues(typeof(Stats)))
                    {
                        _statOptions.Add(stat.ToString());
                    }
                }

                PromptOptions<string> options = new PromptOptions<string>(
                $"Where would you like to allocate the {_statRolls.Peek()}?",
                "That was not a valid option.",
                "You are being silly!",
                _statOptions,
                1);

                PromptDialog.Choice(context, PlayerStat_Allocation, options);                
            }
            else
            {
                await context.PostAsync("Ok, clearing any currently set Stats.");
                Player.StatsContainer.Clear();
                _statRolls.Clear();
                _statOptions.Clear();
                context.Wait(MessageReceived);
            }
        }

        /// <summary>
        /// The actual setting of our Player objects' stats is done through this private asynchronous helper method.
        /// </summary>
        /// <param name="context">The <see cref="IDialogContext>"/> which is passed in.</param>
        /// <param name="result">The <see cref="IAwaitable{string}>"/> which is passed in.</param>
        /// <returns>Method awaits the completion of the Posting process.</returns>
        private async Task PlayerStat_Allocation(IDialogContext context, IAwaitable<string> result)
        {
            string input = await result;

            switch (input)
            {
                case "Charisma":
                {
                    if (!Player.StatsContainer.ContainsKey(Stats.Charisma))
                    {
                        Player.StatsContainer.Add(Stats.Charisma, _statRolls.Dequeue());
                        _statOptions.Remove("Charisma");
                        await context.PostAsync($"Allocation complete. Charisma: {Player.StatsContainer[Stats.Charisma]}.");
                    }
                    else
                    {
                        await context.PostAsync($"You have already allocated a {Player.StatsContainer[Stats.Charisma]} for Charisma!");
                    }
                    break;
                }              
                case "Constitution":
                {
                    if (!Player.StatsContainer.ContainsKey(Stats.Constitution))
                    {
                        Player.StatsContainer.Add(Stats.Constitution, _statRolls.Dequeue());
                        _statOptions.Remove("Constitution");
                        await context.PostAsync($"Allocation complete. Constitution: {Player.StatsContainer[Stats.Constitution]}.");
                    }
                    else
                    {
                        await context.PostAsync($"You have already allocated a {Player.StatsContainer[Stats.Constitution]} for Constitution!");
                    }
                    break;
                }
                case "Dexterity":
                {
                    if (!Player.StatsContainer.ContainsKey(Stats.Dexterity))
                    {
                        Player.StatsContainer.Add(Stats.Dexterity, _statRolls.Dequeue());
                        _statOptions.Remove("Dexterity");
                        await context.PostAsync($"Allocation complete. Dexterity: {Player.StatsContainer[Stats.Dexterity]}.");
                    }
                    else
                    {
                        await context.PostAsync($"You have already allocated a {Player.StatsContainer[Stats.Dexterity]} for Dexterity!");
                    }
                    break;
                }
                case "Intellect":
                {
                    if (!Player.StatsContainer.ContainsKey(Stats.Intellect))
                    {
                        Player.StatsContainer.Add(Stats.Intellect, _statRolls.Dequeue());
                        _statOptions.Remove("Intellect");
                        await context.PostAsync($"Allocation complete. Intellect: {Player.StatsContainer[Stats.Intellect]}.");
                    }
                    else
                    {
                        await context.PostAsync($"You have already allocated a {Player.StatsContainer[Stats.Intellect]} for Intellect!");
                    }
                    break;
                }
                case "Strength":
                {
                    if (!Player.StatsContainer.ContainsKey(Stats.Strength))
                    {
                        Player.StatsContainer.Add(Stats.Strength, _statRolls.Dequeue());
                        _statOptions.Remove("Strength");
                        await context.PostAsync($"Allocation complete. Strength: {Player.StatsContainer[Stats.Strength]}.");
                    }
                    else
                    {
                        await context.PostAsync($"You have already allocated a {Player.StatsContainer[Stats.Strength]} for Strength!");
                    }
                    break;
                }
                case "Wisdom":
                {
                    if (!Player.StatsContainer.ContainsKey(Stats.Wisdom))
                    {
                        Player.StatsContainer.Add(Stats.Wisdom, _statRolls.Dequeue());
                        _statOptions.Remove("Wisdom");
                        await context.PostAsync($"Allocation complete. Wisdom: {Player.StatsContainer[Stats.Wisdom]}.");
                    }
                    else
                    {
                        await context.PostAsync($"You have already allocated a {Player.StatsContainer[Stats.Wisdom]} for Wisdom!");
                    }
                    break;
                }
                default: await context.PostAsync("You must select a valid Stat!"); break;
            }

            context.Wait(MessageReceived);
        }

        #endregion

        #region Player Skills Intent
        
        /// <summary>
        /// This PlayerSkill method is called when LUIS recognizes the intent 'PlayerSkills' being called.
        /// Intended usage: skill set. Accepting any of the <see cref="ClassSkills"/> enumeration fields.
        /// This method also prompts a list of choices to lock in the selected skill.
        /// </summary>
        /// <param name="context">The <see cref="IDialogContext>"/> which is passed in.</param>
        /// <param name="result">The <see cref="LuisResult>"/> which is passed in.</param>
        /// <returns>Method awaits the completion of the Posting process.</returns>
        [LuisIntent("PlayerSkills")]
        public async Task PlayerSkill(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I recognized an intent to assign hero skills.");

            // special note: we need to have the class, race, and stats done before we can look at Skills.
            if (Player.DesiredAlign == Alignment.None ||
                Player.DesiredClass == ClassType.None || Player.DesiredClass == ClassType.Caster ||
                Player.DesiredRace == RaceType.None ||
                Player.StatsContainer.Count != 6)
            {
                await context.PostAsync("In order to set skills, you must first fully set your hero's RACE, ALIGN, CLASS, and STATS.");
                context.Wait(MessageReceived);
            }
            else
            {
                if (PlayerSkills.SkillsContainer.Count == 0)
                {
                    PlayerSkills.PopulateContainer();
                    Player.GetHero = Hero.GetStageTwoHero(Player.DesiredClass, Player.DesiredRace, Player.StatsContainer);
                }

                if (_skillOptions.Count == 0)
                {
                    foreach (var skill in PlayerSkills.SkillsContainer)
                    {
                        _skillOptions.Add(skill.SkillType.ToString());
                    }
                }

                if (Player.GetHero.SkillRanksAvailable > 0)
                {
                    PromptOptions<string> options = new PromptOptions<string>(
                        $"What skills would you like your hero to have? You have {Player.GetHero.SkillRanksAvailable} available point(s) to spend.",
                        "That was not a valid option.",
                        "You are being silly!",
                        _skillOptions,
                        1);

                    PromptDialog.Choice(context, PlayerSkill_Dialog, options);                    
                }
                else
                {
                    await context.PostAsync("You have no more skill ranks to place. (use 'new' command to wipe all information)");
                    context.Wait(MessageReceived);
                }
            }
        }

        /// <summary>
        /// This intermediate method accepts a string choice dialog from the user.
        /// The PlayerSkill_Dialog method then forwards a YES/NO choice dialog
        /// to the user confirming that this is the correct skill they want to add.
        /// </summary>
        /// <param name="context">The <see cref="IDialogContext>"/> which is passed in.</param>
        /// <param name="result">The <see cref="IAwaitable{string}>"/> which is passed in.</param>
        /// <returns>Method awaits the completion of the Posting process.</returns>
        private async Task PlayerSkill_Dialog(IDialogContext context, IAwaitable<string> result)
        {
            _lastSkillSelected = await result;
            string description = SkillInformation.GetDescription(_lastSkillSelected);

            await context.PostAsync($"{_lastSkillSelected} description:");
            await context.PostAsync($"{description}");

            PromptDialog.Confirm(context, PlayerSkill_AddSkill, $"Do you want to add a skill rank into {_lastSkillSelected}?");                    
        }

        /// <summary>
        /// The actual setting of our Player objects' skills is done through this private asynchronous helper method.
        /// </summary>
        /// <param name="context">The <see cref="IDialogContext>"/> which is passed in.</param>
        /// <param name="result">The <see cref="IAwaitable{bool}>"/> which is passed in.</param>
        /// <returns>Method awaits the completion of the Posting process.</returns>
        private async Task PlayerSkill_AddSkill(IDialogContext context, IAwaitable<bool> result)
        {
            if (await result)
            {
                foreach (var skill in PlayerSkills.SkillsContainer)
                {
                    if (skill.SkillType.ToString() == _lastSkillSelected)
                    {
                        if (skill.NumberOfRanks < Player.GetHero.SkillCap && Player.GetHero.SkillRanksAvailable > 0)
                        {
                            skill.NumberOfRanks++;
                            Player.GetHero.SkillRanksAvailable--;
                            await context.PostAsync($"We have added a rank into {_lastSkillSelected}. Current ranks: {skill.NumberOfRanks}. You have {Player.GetHero.SkillRanksAvailable} skill ranks left.");

                            if (skill.NumberOfRanks == Player.GetHero.SkillCap)
                            {
                                _skillOptions.Remove(_lastSkillSelected);
                            }
                        }

                        break;
                    }
                }
            }
            else
            {
                await context.PostAsync($"Ok, we wont add any skill ranks into {_lastSkillSelected}.");
            }

            context.Wait(MessageReceived);
        }

        #endregion

        #region Place Multiple Skills Intent

        /// <summary>
        /// This MultiplePlayerSkills method is called when LUIS recognizes the intent 'PlaceMultipleSkills' being called.
        /// Intended usage: place multiple. Accepting any of the <see cref="ClassSkills"/> enumeration fields.
        /// This method also prompts a list of choices to lock in the selected skill.
        /// </summary>
        /// <param name="context">The <see cref="IDialogContext>"/> which is passed in.</param>
        /// <param name="result">The <see cref="LuisResult>"/> which is passed in.</param>
        /// <returns>Method awaits the completion of the Posting process.</returns>
        [LuisIntent("PlaceMultipleSkills")]
        public async Task MultiplePlayerSkills(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I recognized an intent to assign 4 hero skills at once.");

            // special note: we need to have the class, race, and stats done before we can look at Skills.
            if (Player.DesiredAlign == Alignment.None ||
                Player.DesiredClass == ClassType.None || Player.DesiredClass == ClassType.Caster ||
                Player.DesiredRace == RaceType.None ||
                Player.StatsContainer.Count != 6)
            {
                await context.PostAsync("In order to set skills, you must first fully set your hero's RACE, ALIGN, CLASS, and STATS.");
                context.Wait(MessageReceived);
            }
            else
            {
                if (PlayerSkills.SkillsContainer.Count == 0)
                {
                    PlayerSkills.PopulateContainer();
                    Player.GetHero = Hero.GetStageTwoHero(Player.DesiredClass, Player.DesiredRace, Player.StatsContainer);
                }

                if (_skillOptions.Count == 0)
                {
                    foreach (var skill in PlayerSkills.SkillsContainer)
                    {
                        _skillOptions.Add(skill.SkillType.ToString());
                    }
                }

                if (Player.GetHero.SkillRanksAvailable >= 4)
                {
                    PromptOptions<string> options = new PromptOptions<string>(
                        $"What skills would you like your hero to have? You have {Player.GetHero.SkillRanksAvailable} available point(s) to spend.",
                        "That was not a valid option.",
                        "You are being silly!",
                        _skillOptions,
                        1);

                    PromptDialog.Choice(context, MultiplePlayerSkill_Dialog, options);
                }
                else
                {
                    await context.PostAsync($"You dont have enough skill ranks to place multiple (need 4, have: {Player.GetHero.SkillRanksAvailable}). (use 'new' command to wipe all information)");
                    context.Wait(MessageReceived);
                }
            }
        }

        /// <summary>
        /// This intermediate method accepts a string choice dialog from the user.
        /// The MultiplePlayerSkill_Dialog method then forwards a YES/NO choice dialog
        /// to the user confirming that this is the correct skill they want to add.
        /// </summary>
        /// <param name="context">The <see cref="IDialogContext>"/> which is passed in.</param>
        /// <param name="result">The <see cref="IAwaitable{string}>"/> which is passed in.</param>
        /// <returns>Method awaits the completion of the Posting process.</returns>
        private async Task MultiplePlayerSkill_Dialog(IDialogContext context, IAwaitable<string> result)
        {
            _lastSkillSelected = await result;
            string description = SkillInformation.GetDescription(_lastSkillSelected);

            await context.PostAsync($"{_lastSkillSelected} description:");
            await context.PostAsync($"{description}");

            PromptDialog.Confirm(context, MultiplePlayerSkill_AddSkill, $"Do you want to add 4 skill ranks into {_lastSkillSelected}?");
        }

        /// <summary>
        /// The actual setting of our Player objects' skills is done through this private asynchronous helper method.
        /// </summary>
        /// <param name="context">The <see cref="IDialogContext>"/> which is passed in.</param>
        /// <param name="result">The <see cref="IAwaitable{bool}>"/> which is passed in.</param>
        /// <returns>Method awaits the completion of the Posting process.</returns>
        private async Task MultiplePlayerSkill_AddSkill(IDialogContext context, IAwaitable<bool> result)
        {
            if (await result)
            {
                foreach (var skill in PlayerSkills.SkillsContainer)
                {
                    if (skill.SkillType.ToString() == _lastSkillSelected)
                    {
                        if (skill.NumberOfRanks == 0 && Player.GetHero.SkillRanksAvailable >= 4)
                        {
                            skill.NumberOfRanks += 4;
                            Player.GetHero.SkillRanksAvailable -= 4;
                            await context.PostAsync($"We have added 4 ranks into {_lastSkillSelected}. Current ranks: {skill.NumberOfRanks}. You have {Player.GetHero.SkillRanksAvailable} skill ranks left.");

                            if (skill.NumberOfRanks == Player.GetHero.SkillCap)
                            {
                                _skillOptions.Remove(_lastSkillSelected);
                            }
                        }
                        else
                        {
                            await context.PostAsync($"We cant place 4 ranks into {_lastSkillSelected} because that would exceed the maximum allowed at this level.");
                        }

                        break;
                    }
                }
            }
            else
            {
                await context.PostAsync($"Ok, we wont add any skill ranks into {_lastSkillSelected}.");
            }

            context.Wait(MessageReceived);
        }

        #endregion

        #region Player Feats Intent

        /// <summary>
        /// This PlayerFeat method is called when LUIS recognizes the intent 'PlayerFeats' being called.
        /// Intended usage: feat set. Accepting any of the <see cref="ClassFeats"/> enumeration fields.
        /// This method also prompts a list of choices to lock in the selected feat.
        /// </summary>
        /// <param name="context">The <see cref="IDialogContext>"/> which is passed in.</param>
        /// <param name="result">The <see cref="LuisResult>"/> which is passed in.</param>
        /// <returns>Method awaits the completion of the Posting process.</returns>
        [LuisIntent("PlayerFeats")]
        public async Task PlayerFeat(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I recognized an intent to assign hero feats.");
            
            // Special note: we must first ensure that the player has allocated all skills before doing feats.
            if (PlayerSkills.SkillsContainer.Count == 0 || Player.GetHero.SkillRanksAvailable > 0)
            {
                await context.PostAsync("In order to set feats, you must first fully allocate all hero skills.");
                context.Wait(MessageReceived);
            }
            else
            {
                if (PlayerFeats.FeatsContainer.Count == 0)
                {
                    PlayerFeats.PopulateContainer();
                }

                if (_featOptions.Count == 0)
                {
                    foreach (var feat in PlayerFeats.FeatsContainer)
                    {
                        _featOptions.Add(feat.FeatType.ToString());
                    }
                }

                if (Player.GetHero.FeatsAvailable > 0 || Player.GetHero.BonusFeatsAvailable)                 
                {
                    int count = Player.GetHero.FeatsAvailable;
                    if (Player.GetHero.BonusFeatsAvailable)
                    {
                        count++;
                    }

                    PromptOptions<string> options = new PromptOptions<string>(
                        $"What feats would you like your hero to acquire? You can acquire {count} feat(s).",
                        "That was not a valid option.",
                        "You are being silly!",
                        _featOptions,
                        1);

                    PromptDialog.Choice(context, PlayerFeat_Dialog, options);
                    
                }
                else
                {
                    await context.PostAsync("You cannot acqure any more feats. (use 'new' command to wipe all information)");
                    context.Wait(MessageReceived);
                }
            }
        }

        /// <summary>
        /// This intermediate method accepts a string choice dialog from the user.
        /// The PlayerFeat_Dialog method then forwards a YES/NO choice dialog
        /// to the user confirming that this is the correct feat they want to add.
        /// </summary>
        /// <param name="context">The <see cref="IDialogContext>"/> which is passed in.</param>
        /// <param name="result">The <see cref="IAwaitable{string}>"/> which is passed in.</param>
        /// <returns>Method awaits the completion of the Posting process.</returns>
        private async Task PlayerFeat_Dialog(IDialogContext context, IAwaitable<string> result)
        {
            _lastFeatSelected = await result;
            ClassFeats which = (ClassFeats)FeatInformation.GetEnumFromString(_lastFeatSelected);
            
            string prereq = FeatInformation.GetPrerequisitesString(which);
            string description = FeatInformation.GetDescription(which);

            await context.PostAsync($"{_lastFeatSelected} description:");
            await context.PostAsync($"{description}");
            await context.PostAsync($"{_lastFeatSelected} prereqs:");
            await context.PostAsync($"{prereq}");

            PromptDialog.Confirm(context, PlayerFeat_AddFeat, "Do you want to add this feat?");
        }

        /// <summary>
        /// The actual setting of our Player objects' feats is done through this private asynchronous helper method.
        /// </summary>
        /// <param name="context">The <see cref="IDialogContext>"/> which is passed in.</param>
        /// <param name="result">The <see cref="IAwaitable{bool}>"/> which is passed in.</param>
        /// <returns>Method awaits the completion of the Posting process.</returns>
        private async Task PlayerFeat_AddFeat(IDialogContext context, IAwaitable<bool> result)
        {
            if (await result)
            {
                foreach (var feat in PlayerFeats.FeatsContainer)
                {
                    if (feat.FeatType.ToString() == _lastFeatSelected)
                    {
                        FeatRequirementCheck.CheckIfFeatCanBeAcquired(Player.GetHero, feat);

                        if (feat.CanAcquire && !feat.IsAcquired)
                        {
                            if (Player.GetHero.BonusFeatsAvailable)
                            {
                                Player.GetHero.BonusFeatsAvailable = false;
                                FeatRequirementCheck.AcquireTheFeat(feat);
                                _featOptions.Remove(_lastFeatSelected);
                                await context.PostAsync($"We acquired the {_lastFeatSelected} feat.");
                            }
                            else if (Player.GetHero.FeatsAvailable > 0)
                            {
                                Player.GetHero.FeatsAvailable--;
                                FeatRequirementCheck.AcquireTheFeat(feat);
                                _featOptions.Remove(_lastFeatSelected);
                                await context.PostAsync($"We acquired the {_lastFeatSelected} feat.");
                            }                                                        
                        }

                        break;
                    }
                }
            }
            else
            {
                await context.PostAsync($"Ok, we wont acquire the {_lastFeatSelected} feat.");
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
            await context.PostAsync("You can use 'show' or 'info' anytime to display currently set fields");
            await context.PostAsync("Lets start from these basic hero fundamentals:");

            if (Player.Name == null)
            {
                await context.PostAsync("A name must be set. Use the command 'name' followed by the desired first name of your hero.");
            }

            if (Player.Gender == null)
            {
                await context.PostAsync("A gender must be set. Use the command 'gender' to walk through this process.");
            }

            if (Player.DesiredAlign == Alignment.None)
            {
                await context.PostAsync("An alignment must be set. Use the command 'align' to walk through this process.");
            }

            if (Player.DesiredRace == RaceType.None)
            {
                await context.PostAsync("A Player Race must be set. Use the command 'race' to walk through this process.");
            }

            if (Player.DesiredClass == ClassType.None)
            {
                await context.PostAsync("A Player Class must be set. Use the command 'class' to walk through this process.");
            }            

            if (Player.StatsContainer.Count < 6)
            {
                await context.PostAsync("The Players Stats must be set. Use the command 'stats' to walk through this process.");
            }

            if (Player.GetHero == null || Player.GetHero.SkillRanksAvailable > 0)
            {
                await context.PostAsync("The Players Skills must be set. Use the command 'skills or multiple' to walk through this process.");
            }

            if (Player.GetHero == null || Player.GetHero.BonusFeatsAvailable || Player.GetHero.FeatsAvailable > 0)
            {
                await context.PostAsync("The Players Feats must be set. Use the command 'feat' to walk through this process.");
            }
            else
            {
                await context.PostAsync("Your hero is complete! Try the 'save' command to export this hero to a file!");
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

        #region Info Intent

        /// <summary>
        /// The DisplayInfo method is fired when LUIS recognizes terms pertinant to the Info Intent.
        /// Usage: 'show' or 'info'.
        /// </summary>
        /// <param name="context">The <see cref="IDialogContext>"/> which is passed in.</param>
        /// <param name="result">The <see cref="LuisResult>"/> which is passed in.</param>
        /// <returns>Method awaits the completion of the Posting process.</returns>
        [LuisIntent("Info")]
        public async Task DisplayInfo(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Your hero has the following properties set:");

            if (Player.Name != null)
            {
                await context.PostAsync($"Name: {Player.Name}.");
            }

            if (Player.Gender != null)
            {
                await context.PostAsync($"Gender: {Player.Gender}.");
            }

            if (Player.DesiredAlign != Alignment.None)
            {
                await context.PostAsync($"Alignment: {Player.DesiredAlign}.");
            }

            if (Player.DesiredRace != RaceType.None)
            {
                await context.PostAsync($"Race: {Player.DesiredRace}.");
            }

            if (Player.DesiredClass != ClassType.None && Player.DesiredClass != ClassType.Caster)
            {
                await context.PostAsync($"Class: {Player.DesiredClass}.");
            }

            if (Player.StatsContainer.Count == 6)
            {
                await context.PostAsync("Players' Stats:");
                StringBuilder sb = new StringBuilder();

                foreach (var stat in Player.StatsContainer)
                {
                    sb.AppendLine($"{stat.Key}: {stat.Value}");
                }

                await context.PostAsync($"{sb.ToString()}.");
            }

            if (PlayerSkills.SkillsContainer.Count != 0)
            {
                await context.PostAsync("Players' Skills:");
                StringBuilder sb = new StringBuilder();

                foreach (var skill in PlayerSkills.SkillsContainer)
                {
                    if (skill.NumberOfRanks > 0)
                    {
                        sb.AppendLine($"{skill.SkillType.ToString()}: {skill.NumberOfRanks}");
                    }
                }

                await context.PostAsync($"{sb.ToString()}.");
            }

            if (PlayerFeats.FeatsContainer.Count != 0)
            {
                await context.PostAsync("Players' Feats:");
                StringBuilder sb = new StringBuilder();

                foreach (var feat in PlayerFeats.FeatsContainer)
                {
                    if (feat.IsAcquired)
                    {
                        sb.AppendLine($" {feat.FeatType.ToString()} ");
                    }
                }

                await context.PostAsync($"{sb.ToString()}.");
            }

            context.Wait(MessageReceived);
        }

        #endregion

        #region Save Hero Intent

        /// <summary>
        /// The SaveHero method is fired when LUIS recognizes the intent 'SaveHero' being called.
        /// Intended usage: save. The Hero must be complete at this point.
        /// </summary>
        /// <param name="context">The <see cref="IDialogContext>"/> which is passed in.</param>
        /// <param name="result">The <see cref="LuisResult>"/> which is passed in.</param>
        /// <returns>Method awaits the completion of the Posting process.</returns>
        [LuisIntent("Save")]
        public async Task SaveHero(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I recognized an intent to save the hero.");

            if (Player.GetHero == null || Player.GetHero.BonusFeatsAvailable || Player.GetHero.FeatsAvailable > 0)
            {
                await context.PostAsync("You have not finished placing Feats for your hero. You may not save yet!");
            }
            else
            {
                // Call Trim on the containers:
                PlayerFeats.TrimContainer();
                PlayerSkills.TrimContainer();

                // Set the Static information generated thus far into the Player!
                // Recall that earlier we used Player.GetHero = Hero.GetStageTwoHero(Player.DesiredClass, Player.DesiredRace, Player.StatsContainer);
                // which means we already set the class, race, and stats. We still however, need to set the feats, skills, name, and gender!
                Player.GetHero.PlayerFeats = PlayerFeats.FeatsContainer;
                Player.GetHero.PlayerSkills = PlayerSkills.SkillsContainer;
                Player.GetHero.Name = Player.Name;
                Player.GetHero.Gender = Player.Gender;
                Player.GetHero.PlayerAlignment = Player.DesiredAlign;

                SaveLoadInfo.Serialize(Player.GetHero);

                await context.PostAsync("Your hero has successfully been saved! Check your desktop in the DnDSaves folder! Thank you.");
            }

            context.Wait(MessageReceived);
        }

       #endregion
    }
}