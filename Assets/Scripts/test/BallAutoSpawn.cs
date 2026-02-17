using UnityEngine;

namespace PlinkoPinball.Gameplay
{
    public sealed class BallAutoSpawnOnPlay : MonoBehaviour
    {
        [SerializeField] private BallSpawner spawner;
        private void Start() { spawner.Spawn(); }
    }
}
