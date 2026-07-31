using Wizard;

namespace Sheet
{
    public sealed class SkillConfig
    {
        [DataSheet(0)] public int Id;
        [DataSheet(1)] public string Name;
        [DataSheet(2)] public SkillRarity Rarity;
        [DataSheet(3)] public string Abilities;
        [DataSheet(4)] public string Description;
        [DataSheet(5)] public int Price;
    }

    public static partial class SheetConfig
    {
        public static SheetTable<SkillConfig> LoadConfigSkills => LoadConfig<SkillConfig>("Skill");
    }
}
