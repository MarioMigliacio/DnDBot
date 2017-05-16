using DnD.Classes.Player;
using DnD.Enums.Alignment;
using DnD.Enums.ClassTypes;
using DnD.Enums.Races;
using DnD.Enums.Stats;
using System.Collections.Generic;

namespace DnDBot.HeroInformation
{
    /// <summary>
    /// We only want to make one Hero during the runtime of the application.
    /// Because of this, it makes sense to have one instance and build off of it.
    /// </summary>
    public static class Player
    {
        /// <summary>
        /// Allows access to the Hero being assembled thus far.
        /// </summary>
        public static Hero GetHero { get; set; } = null;

        /// <summary>
        /// A static property which can be called to save player Name preferences.
        /// </summary>
        public static string Name { get; set; } = null;

        /// <summary>
        /// A static property which can be called to save player Gender preferences.
        /// </summary>
        public static string Gender { get; set; } = null;

        /// <summary>
        /// A static property which can be called to save player Race preferences.
        /// </summary>
        public static Alignment DesiredAlign { get; set; } = Alignment.None;

        /// <summary>
        /// A static property which can be called to save player Class Type preferences.
        /// </summary>
        public static ClassType DesiredClass { get; set; } = ClassType.None;

        /// <summary>
        /// A static property which can be called to save player Race preferences.
        /// </summary>
        public static RaceType DesiredRace { get; set; } = RaceType.None;

        /// <summary>
        /// A static property which can be called to save player Stats.
        /// </summary>
        public static Dictionary<Stats, int> StatsContainer { get; set; } = new Dictionary<Stats, int>();        
    }
}