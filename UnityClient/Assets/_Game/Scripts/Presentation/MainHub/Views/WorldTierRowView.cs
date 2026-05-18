using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.MainHub.Views
{
    public sealed class WorldTierRowView : MonoBehaviour
    {
        [SerializeField] private TMP_Text rowText = null!;
        [SerializeField] private Button button = null!;

        public void Bind(
            string text,
            bool isInteractable,
            Action onClicked)
        {
            if (rowText == null)
            {
                rowText = GetComponentInChildren<TMP_Text>();
            }

            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (rowText != null)
            {
                rowText.text = text;
            }

            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.interactable = isInteractable;

            if (isInteractable)
            {
                button.onClick.AddListener(() => onClicked());
            }
        }
    }
}