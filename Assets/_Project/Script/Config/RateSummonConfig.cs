using System;
using System.Collections.Generic;
using Wizard;

namespace Sheet
{
    public sealed class RateSummonConfig
    {
        [DataSheet(0)] public int Id;
        [DataSheet(1)] internal SkillRarity[] Rarities;
        [DataSheet(2)] public int Level;
        [DataSheet(3)] internal float[] Rates;
    }

    public static partial class SheetConfig
    {
        public static int RateSummonCount => RateSummonCache.LoadConfigRateTable.Count;

        public static int GetRateSummonLevel(int order)
        {
            return RateSummonCache.LoadConfigRateTable.GetByOrder(order).Level;
        }

        public static float GetRateSummon(
            int order,
            SkillRarity rarity)
        {
            RateSummonConfig config =
                RateSummonCache.LoadConfigRateTable.GetByOrder(order);

            for (int i = 0; i < config.Rarities.Length; i++)
            {
                if (config.Rarities[i] == rarity)
                    return config.Rates[i];
            }

            throw new KeyNotFoundException(
                $"Cannot find summon rarity '{rarity}' " +
                $"at level {config.Level}.");
        }

        private static SheetTable<RateSummonConfig> LoadRateSummons()
        {
            SheetTable<RateSummonConfig> table =
                LoadConfig<RateSummonConfig>("RateSummon");

            for (int i = 0; i < table.Count; i++)
            {
                RateSummonConfig config = table[i];
                if (config.Rarities.Length != config.Rates.Length)
                {
                    throw new InvalidOperationException($"RateSummon Id {config.Id} has different " + "rarity and rate counts.");
                }

                var rarities = new HashSet<SkillRarity>();
                for (int j = 0; j < config.Rarities.Length; j++)
                {
                    if (!rarities.Add(config.Rarities[j]))
                    {
                        throw new InvalidOperationException($"RateSummon Id {config.Id} contains duplicate " + $"rarity '{config.Rarities[j]}'.");
                    }
                }
            }

            return table;
        }

        private static class RateSummonCache
        {
            public static readonly SheetTable<RateSummonConfig> LoadConfigRateTable = LoadRateSummons();
        }
    }
}
