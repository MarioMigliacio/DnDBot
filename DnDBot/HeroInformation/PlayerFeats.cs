using DnD.Classes.HeroFeats;
using DnD.Enums.ClassFeats;
using System;
using System.Collections.Generic;

namespace DnDBot.HeroInformation
{
    /// <summary>
    /// The PlayerFeats class is responsible for managing the players selected feats during program execution.
    /// Use of static because it makes sense to only use one managed instance of the players feat collection.
    /// </summary>
    public static class PlayerFeats
    {
        /// <summary>
        /// A static property which can be called to save player Feats.
        /// </summary>
        public static List<BaseFeat> FeatsContainer { get; set; } = new List<BaseFeat>();

        /// <summary>
        /// A helper method used to populate the FeatsContainer with default feats.
        /// </summary>
        public static void PopulateContainer()
        {
            foreach (ClassFeats feat in Enum.GetValues(typeof(ClassFeats)))
            {
                if (FeatFactory.Create(feat) != null)
                {
                    FeatsContainer.Add(FeatFactory.Create(feat));
                }
            }
        }

        /// <summary>
        /// A helper method used to trim feats which have not been acquired.
        /// </summary>
        public static void TrimContainer()
        {
            List<BaseFeat> toRemove = new List<BaseFeat>();

            foreach (BaseFeat feat in FeatsContainer)
            {
                if (!feat.IsAcquired)
                {
                    toRemove.Add(feat);
                }
            }

            foreach (BaseFeat feat in toRemove)
            {
                FeatsContainer.Remove(feat);
            }
        }
    }
}