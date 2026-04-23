using UnityEngine;

namespace PlinkoPinball.UI.Upgrades
{
    /// <summary>
    /// 업그레이드 팝업 토글 진입점
    /// </summary>
    // HUD 버튼 또는 InputRouter에서 연결한다
    public sealed class UpgradePopupToggleController : MonoBehaviour
    {
        [SerializeField] private UpgradePopupPresenter popupPresenter;

        /// <summary>
        /// 팝업 표시 상태를 토글
        /// </summary>
        public void Toggle()
        {
            if (popupPresenter == null)
            {
                return;
            }

            if (popupPresenter.gameObject.activeSelf)
            {
                popupPresenter.Close();
            }
            else
            {
                popupPresenter.Open();
            }
        }

        public void Open()
        {
            popupPresenter?.Open();
        }

        public void Close()
        {
            popupPresenter?.Close();
        }
    }
}