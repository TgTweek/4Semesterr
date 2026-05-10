using Game.Domain.Entities;
using TMPro;
using UnityEngine;

namespace Game.Presentation.Combat.Views
{
    public sealed class MonsterView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer = null!;
        [SerializeField] private SpriteRenderer hoverCircleRenderer = null!;
        [SerializeField] private TMP_Text nameText = null!;
        [SerializeField] private TMP_Text healthText = null!;
        [SerializeField] private TMP_Text attackText = null!;

        private MonsterRuntimeState _runtimeState = null!;
        private Collider2D _hitCollider = null!;
        private bool _isHovered;

        public MonsterRuntimeState RuntimeState => _runtimeState;

        private void Awake()
        {
            _hitCollider = GetComponent<Collider2D>();
        }

        public void BindState(MonsterRuntimeState runtimeState)
        {
            _runtimeState = runtimeState;
            _isHovered = false;
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
                spriteRenderer.enabled = true;
            }
        }

        public void SetHovered(bool isHovered)
        {
            _isHovered = isHovered;
            Refresh();
        }

        public void Refresh()
        {
            if (_runtimeState == null)
            {
                return;
            }

            var isDead = _runtimeState.IsDead;

            if (isDead)
            {
                _isHovered = false;
            }

            if (nameText != null)
            {
                nameText.text = _runtimeState.Name;
                nameText.gameObject.SetActive(!isDead);
            }

            if (healthText != null)
            {
                healthText.text = $"{_runtimeState.CurrentHealth}/{_runtimeState.MaxHealth}";
                healthText.gameObject.SetActive(!isDead);
            }

            if (attackText != null)
            {
                attackText.text = $"ATK {_runtimeState.Damage}";
                attackText.gameObject.SetActive(!isDead && _isHovered);
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = !isDead;

                if (!isDead)
                {
                    spriteRenderer.color = Color.white;
                }
            }

            if (hoverCircleRenderer != null)
            {
                hoverCircleRenderer.enabled = !isDead && _isHovered;
            }

            if (_hitCollider != null)
            {
                _hitCollider.enabled = !isDead;
            }
        }
    }
}