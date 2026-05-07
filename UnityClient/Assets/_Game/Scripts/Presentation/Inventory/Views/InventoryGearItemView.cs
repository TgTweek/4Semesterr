using System;
using Game.Infrastructure.Api.Dtos.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.Inventory.Views
{
    public sealed class InventoryGearItemView : MonoBehaviour
    {
        [SerializeField] private TMP_Text gearNameText = null!;
        [SerializeField] private TMP_Text slotText = null!;
        [SerializeField] private TMP_Text rarityText = null!;
        [SerializeField] private TMP_Text armorText = null!;
        [SerializeField] private TMP_Text setText = null!;
        [SerializeField] private TMP_Text locationText = null!;
        [SerializeField] private Button actionButton = null!;
        [SerializeField] private TMP_Text actionButtonText = null!;

        public void Bind(
            PlayerGearInventoryItemDto gear,
            bool isLoadout,
            Action<PlayerGearInventoryItemDto> onActionClicked)
        {
            gearNameText.text = gear.gearName;
            slotText.text = gear.slot;
            rarityText.text = gear.rarity;
            armorText.text = $"Armor: {gear.armorValue}";

            setText.text = string.IsNullOrWhiteSpace(gear.setName)
                ? string.Empty
                : $"{gear.setName} - {gear.threePieceBonusDescription}";

            locationText.text = isLoadout ? "Equipped" : "Stash";
            actionButtonText.text = isLoadout ? "Stash" : "Equip";

            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() => onActionClicked(gear));
        }
    }
}