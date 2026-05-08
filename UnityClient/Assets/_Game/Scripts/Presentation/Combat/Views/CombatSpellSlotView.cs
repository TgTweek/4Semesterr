using System;
using Game.Infrastructure.Api.Dtos.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.Combat.Views
{
    public sealed class CombatSpellSlotView : MonoBehaviour
    {
        [SerializeField] private TMP_Text cardNameText = null!;
        [SerializeField] private TMP_Text manaCostText = null!;
        [SerializeField] private TMP_Text effectText = null!;
        [SerializeField] private Button actionButton = null!;
        [SerializeField] private TMP_Text actionButtonText = null!;
        [SerializeField] private Image backgroundImage = null!;

        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = new Color(1f, 0.95f, 0.6f, 1f);

        public string PlayerCardId { get; private set; } = string.Empty;

        public void Bind(
            PlayerCardInventoryItemDto card,
            Action<PlayerCardInventoryItemDto> onClicked,
            int playerDamageBonus = 0)
        {
            PlayerCardId = card.playerCardId;

            cardNameText.text = card.cardName;
            manaCostText.text = $"Mana: {card.manaCost}";
            effectText.text = BuildEffectText(card, playerDamageBonus);

            if (actionButtonText != null)
            {
                actionButtonText.text = GetButtonLabel(card.effectType);
            }

            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() => onClicked(card));

            SetSelected(false);
            SetInteractable(true);
        }

        private static string BuildEffectText(PlayerCardInventoryItemDto card, int playerDamageBonus)
        {
            if (string.Equals(card.effectType, "Damage", StringComparison.OrdinalIgnoreCase))
            {
                var finalDamage = card.effectValue + playerDamageBonus;

                if (playerDamageBonus > 0)
                {
                    return $"Damage: {finalDamage} ({card.effectValue}+{playerDamageBonus})";
                }

                return $"Damage: {finalDamage}";
            }

            return $"{card.effectType}: {card.effectValue}";
        }

        public void SetSelected(bool isSelected)
        {
            backgroundImage.color = isSelected ? selectedColor : normalColor;
        }

        public void SetInteractable(bool isInteractable)
        {
            actionButton.interactable = isInteractable;
        }

        private static string GetButtonLabel(string effectType)
        {
            if (string.Equals(effectType, "Damage", StringComparison.OrdinalIgnoreCase))
            {
                return "Cast";
            }

            if (string.Equals(effectType, "Block", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(effectType, "Heal", StringComparison.OrdinalIgnoreCase))
            {
                return "Use";
            }

            return "Use";
        }
    }
}