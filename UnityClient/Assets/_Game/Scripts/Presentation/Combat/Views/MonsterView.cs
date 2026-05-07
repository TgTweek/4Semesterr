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

            nameText.text = _runtimeState.Name;
            healthText.text = $"{_runtimeState.CurrentHealth}/{_runtimeState.MaxHealth}";

            spriteRenderer.color = _runtimeState.IsDead
                ? new Color(1f, 1f, 1f, 0.35f)
                : Color.white;
        }
    }
}