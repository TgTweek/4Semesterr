using Game.Domain.Entities;
using TMPro;
using UnityEngine;

namespace Game.Presentation.Combat.Views
{
    public sealed class MonsterView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer = null!;
        [SerializeField] private TMP_Text nameText = null!;
        [SerializeField] private TMP_Text healthText = null!;

        private MonsterRuntimeState _runtimeState = null!;

        public MonsterRuntimeState RuntimeState => _runtimeState;

        public void BindState(MonsterRuntimeState runtimeState)
        {
            _runtimeState = runtimeState;
            Refresh();
        }

        public void SetSprite(Sprite sprite)
        {
            if (spriteRenderer == null)
            {
                return;
            }

            if (sprite != null)
            {
                spriteRenderer.sprite = sprite;
            }
        }

        public void Refresh()
        {
            if (_runtimeState == null)
            {
                return;
            }

            if (nameText != null)
            {
                nameText.text = _runtimeState.Name;
            }

            if (healthText != null)
            {
                healthText.text = $"{_runtimeState.CurrentHealth}/{_runtimeState.MaxHealth}";
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.color = _runtimeState.IsDead
                    ? new Color(1f, 1f, 1f, 0.35f)
                    : Color.white;
            }
        }
    }
}