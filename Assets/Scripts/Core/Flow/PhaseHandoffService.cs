using UnityEngine;

namespace PlinkoPinball.Gameplay.Core.Flow
{
    /// <summary>
    /// 씬 전환 간 플링코 컨텍스트를 전달하는 DDOL 서비스
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PhaseHandoffService : MonoBehaviour
    {
        public static PhaseHandoffService Instance { get; private set; }

        private bool _hasPlinkoContext;
        private PlinkoPhaseHandoffContext _plinkoContext;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 플링코 컨텍스트를 저장
        /// </summary>
        public void SetPlinkoContext(in PlinkoPhaseHandoffContext context)
        {
            _plinkoContext = context;
            _hasPlinkoContext = true;
        }

        /// <summary>
        /// 저장된 플링코 컨텍스트를 반환하고 내부 저장값을 비움
        /// </summary>
        public bool TryConsumePlinkoContext(out PlinkoPhaseHandoffContext context)
        {
            context = _plinkoContext;

            if (!_hasPlinkoContext)
            {
                return false;
            }

            _hasPlinkoContext = false;
            _plinkoContext = default;
            return true;
        }

        /// <summary>
        /// 저장된 플링코 컨텍스트를 강제로 제거
        /// </summary>
        public void ClearPlinkoContext()
        {
            _hasPlinkoContext = false;
            _plinkoContext = default;
        }
    }
}