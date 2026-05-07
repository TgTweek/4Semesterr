using System;
using Game.Infrastructure.Api.Dtos.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.Inventory.Views
{
    public sealed class InventoryCardItemView : MonoBehaviour
    {
        [SerializeField] private TMP_Text cardNameText = null!;
        [SerializeField] private TMP_Text rarityText = null!;
        [SerializeField] private TMP_Text manaCostText = null!;
        [SerializeField] private TMP_Text effectText = null!;
        [SerializeField] private TMP_Text locationText = null!;
        [SerializeField] private Button actionButton = null!;
        [SerializeField] private TMP_Text actionButtonText = null!;

        public void Bind(
            PlayerCardInventoryItemDto card,
            bool isLoadout,
            Action<PlayerCardInventoryItemDto> onActionClicked)
        {
            cardNameText.text = card.cardName;
            rarityText.text = card.rarity;
            manaCostText.text = $"Mana: {card.manaCost}";
            effectText.text = $"{card.effectType}: {card.effectValue}";

            locationText.text = isLoadout
                ? $"Loadout #{card.loadoutOrder}"
                : "Stash";

            actionButtonText.text = isLoadout ? "Stash" : "Equip";

            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() => onActionClicked(card));
        }
    }
}