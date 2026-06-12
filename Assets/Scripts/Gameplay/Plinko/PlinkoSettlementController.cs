using UnityEngine;
using PlinkoPinball.Core;
using PlinkoPinball.Gameplay.Core.Flow;
using PlinkoPinball.UI.Presenters;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 플링코 런 결과를 메타 재화에 반영하고 결과 표시를 담당
    /// </summary>
    public sealed class PlinkoSettlementController : MonoBehaviour
    {
        [SerializeField] private PlinkoResultPresenter resultPresenter;

        /// <summary>
        /// 플링코 결과를 정산하고 결과 UI를 표시
        /// </summary>
        public void CompleteRun(int roundIndex, int pinballScore, in PlinkoRunResult result)
        {
            if (resultPresenter != null)
            {
                resultPresenter.Show(result.EarnedCurrency, result.TotalPinHits, result.BallsResolved);
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[PlinkoSettlement] Round={roundIndex}, PinballScore={pinballScore}, EarnedCurrency={result.EarnedCurrency}", this);
#endif

            GameSessionState.Instance?.RecordPlinkoRunResult(roundIndex, pinballScore, in result);

        }

        /// <summary>
        /// 결과 확인 후 핀볼 씬으로 복귀
        /// UI 버튼에 연결하는 용도
        /// </summary>
        public void ReturnToPinballScene()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CompletePlinkoAndReturnToPinball();
            }
        }
    }
}
