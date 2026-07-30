using System;
using System.Collections.Generic;
using Sheet;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Wizard
{
    public class RandomCard : MonoBehaviour
    {
        [SerializeField] private TMP_Text[] cardTexts;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text rateStatusText;
        [SerializeField] private GameObject rateStatusPanel;
        [SerializeField] private Button randomButton;
        [SerializeField] private Button increaseLevelButton;
        [SerializeField] private Button statusButton;

        private readonly List<SkillConfig>[] skillBuckets =
        {
            new(),
            new(),
            new(),
            new()
        };

        private readonly SkillConfig[] currentCards = new SkillConfig[3];
        private int rateIndex;

        public int CurrentLevel =>
            SheetConfig.GetRateSummonLevel(rateIndex + 1);
        public IReadOnlyList<SkillConfig> CurrentCards => currentCards;

        private void Awake()
        {
            var skills = SheetConfig.LoadConfigSkills;

            BuildSkillBuckets(skills);

            randomButton.onClick.AddListener(RandomizeCards);
            increaseLevelButton.onClick.AddListener(IncreaseLevel);
            statusButton.onClick.AddListener(ToggleRateStatus);

            rateStatusPanel.SetActive(false);
            UpdateRateView();
            RandomizeCards();
        }

        private void OnDestroy()
        {
            randomButton.onClick.RemoveListener(RandomizeCards);
            increaseLevelButton.onClick.RemoveListener(IncreaseLevel);
            statusButton.onClick.RemoveListener(ToggleRateStatus);
        }

        public void RandomizeCards()
        {
            for (int i = 0; i < currentCards.Length; i++)
            {
                SkillConfig skill = RollSkill();
                currentCards[i] = skill;
                cardTexts[i].text =
                    $"<b>{skill.Name}</b>\n{skill.Rarity}\nPrice: {skill.Price}";
            }
        }

        public void IncreaseLevel()
        {
            if (rateIndex >= SheetConfig.RateSummonCount - 1)
                return;

            rateIndex++;
            UpdateRateView();
        }

        public void ToggleRateStatus()
        {
            rateStatusPanel.SetActive(!rateStatusPanel.activeSelf);
        }

        private void BuildSkillBuckets(SheetTable<SkillConfig> skills)
        {
            for (int i = 0; i < skills.Count; i++)
            {
                SkillConfig skill = skills[i];
                int rarityIndex = (int)skill.Rarity;

                if ((uint)rarityIndex >= skillBuckets.Length)
                {
                    throw new InvalidOperationException(
                        $"Unknown skill rarity '{skill.Rarity}' " +
                        $"on skill Id {skill.Id}.");
                }

                skillBuckets[rarityIndex].Add(skill);
            }

            for (int i = 0; i < skillBuckets.Length; i++)
            {
                if (skillBuckets[i].Count == 0)
                {
                    throw new InvalidOperationException(
                        $"Skill config has no {(SkillRarity)i} card.");
                }
            }
        }

        private SkillConfig RollSkill()
        {
            int rateOrder = rateIndex + 1;
            float totalRate =
                SheetConfig.GetRateSummon(rateOrder, SkillRarity.Common) +
                SheetConfig.GetRateSummon(rateOrder, SkillRarity.Rare) +
                SheetConfig.GetRateSummon(rateOrder, SkillRarity.Epic) +
                SheetConfig.GetRateSummon(rateOrder, SkillRarity.Legendary);
            float roll = UnityEngine.Random.value * totalRate;

            for (int i = 0; i < skillBuckets.Length; i++)
            {
                roll -=
                    SheetConfig.GetRateSummon(rateOrder, (SkillRarity)i);
                if (roll <= 0f)
                {
                    List<SkillConfig> bucket = skillBuckets[i];
                    return bucket[UnityEngine.Random.Range(0, bucket.Count)];
                }
            }

            List<SkillConfig> lastBucket = skillBuckets[^1];
            return lastBucket[
                UnityEngine.Random.Range(0, lastBucket.Count)];
        }

        private void UpdateRateView()
        {
            int rateOrder = rateIndex + 1;
            levelText.text = $"Rate level: {CurrentLevel}";
            rateStatusText.text =
                $"Level {CurrentLevel}\n" +
                $"Common: {SheetConfig.GetRateSummon(rateOrder, SkillRarity.Common):0.##}%\n" +
                $"Rare: {SheetConfig.GetRateSummon(rateOrder, SkillRarity.Rare):0.##}%\n" +
                $"Epic: {SheetConfig.GetRateSummon(rateOrder, SkillRarity.Epic):0.##}%\n" +
                $"Legendary: {SheetConfig.GetRateSummon(rateOrder, SkillRarity.Legendary):0.##}%";
            increaseLevelButton.interactable =
                rateIndex < SheetConfig.RateSummonCount - 1;
        }
    }
}
