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
        [SerializeField] private Image backgroundImage = null!;

        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = new Color(1f, 0.95f, 0.6f, 1f);

        public string PlayerCardId { get; private set; } = string.Empty;

        public void Bind(
            PlayerCardInventoryItemDto card,
            Action<PlayerCardInventoryItemDto> onClicked)
        {
            PlayerCardId = card.playerCardId;

            cardNameText.text = card.cardName;
            manaCostText.text = $"Mana: {card.manaCost}";
            effectText.text = $"{card.effectType}: {card.effectValue}";

            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() => onClicked(card));

            SetSelected(false);
            SetInteractable(true);
        }

        public void SetSelected(bool isSelected)
        {
            backgroundImage.color = isSelected ? selectedColor : normalColor;
        }

        public void SetInteractable(bool isInteractable)
        {
            actionButton.interactable = isInteractable;
        }
    }
}