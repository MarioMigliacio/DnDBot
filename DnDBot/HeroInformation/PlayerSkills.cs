using DnD.Classes.HeroSkills;
using DnD.Enums.ClassSkills;
using System;
using System.Collections.Generic;

namespace DnDBot.HeroInformation
{
    /// <summary>
    /// The PlayerSkills class is responsible for managing the players selected skills during program execution.
    /// Use of static because it makes sense to only use one managed instance of the players skill collection.
    /// </summary>
    public static class PlayerSkills
    {
        /// <summary>
        /// A static property which can be called to save player Skills.
        /// </summary>
        public static List<BaseSkill> SkillsContainer { get; set; } = new List<BaseSkill>();

        /// <summary>
        /// A helper method used to populate the SkillsContainer with default skills having 0 ranks in them.
        /// </summary>
        public static void PopulateContainer()
        {
            foreach (ClassSkills skill in Enum.GetValues(typeof(ClassSkills)))
            {
                if (SkillFactory.Create(skill) != null)
                {
                    SkillsContainer.Add(SkillFactory.Create(skill));
                }
            }
        }

        /// <summary>
        /// A helper method used to trim skills which have no ranks in them.
        /// </summary>
        public static void TrimContainer()
        {
            List<BaseSkill> toRemove = new List<BaseSkill>();

            foreach (BaseSkill skill in SkillsContainer)
            {
                if (skill.NumberOfRanks == 0)
                {
                    toRemove.Add(skill);
                }
            }

            foreach (BaseSkill skill in toRemove)
            {
                SkillsContainer.Remove(skill);
            }
        }
    }
}