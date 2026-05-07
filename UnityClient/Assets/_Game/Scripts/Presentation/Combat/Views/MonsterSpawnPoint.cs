using UnityEngine;

namespace Game.Presentation.Combat.Views
{
    public sealed class MonsterSpawnPoint : MonoBehaviour
    {
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.18f);
        }
#endif
    }
}