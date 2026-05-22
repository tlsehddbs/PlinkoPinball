using UnityEngine;

namespace PlinkoPinball.UI
{
    /// <summary>
    /// 전역 시스템 상태를 TopBarView에 전달
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TopBarPresenter : MonoBehaviour
    {
        public enum TopBarMode
        {
            Pinball,
            Plinko
        }

        [Header("References")]
        [SerializeField] private TopBarView view;

        [Header("Mode")]
        [SerializeField] private TopBarMode mode;

        [Header("Debug Values")]
        [SerializeField] private float powerCurrent = 100f;
        [SerializeField] private float powerMax = 100f;

        [SerializeField] private long throughput = 248520;
        [SerializeField] private int compressionCurrent = 7;
        [SerializeField] private int compressionThreshold = 10;
        [SerializeField] private int startBallCount = 12;
        [SerializeField] private long credits = 12480;

        private void Start()
        {
            Refresh();
        }

        /// <summary>
        /// 현재 상태를 TopBar에 갱신한다.
        /// </summary>
        public void Refresh()
        {
            if (view == null)
            {
                return;
            }

            switch (mode)
            {
                case TopBarMode.Pinball:
                    {
                        view.SetPower(powerCurrent, powerMax);
                        break;
                    }

                case TopBarMode.Plinko:
                    {
                        view.SetSystemStatus("Charging...");
                        break;
                    }
            }

            view.SetThroughput(throughput);
            view.SetCompression(compressionCurrent, compressionThreshold);
            view.SetStartBallCount(startBallCount);
            view.SetCredits(credits);
        }

        public void SetMode(TopBarMode newMode)
        {
            mode = newMode;
            Refresh();
        }

        public void SetPower(float current, float max)
        {
            powerCurrent = current;
            powerMax = max;

            Refresh();
        }

        public void SetThroughput(long value)
        {
            throughput = value;

            Refresh();
        }

        public void SetCompression(int current, int threshold)
        {
            compressionCurrent = current;
            compressionThreshold = threshold;

            Refresh();
        }

        public void SetStartBallCount(int count)
        {
            startBallCount = count;

            Refresh();
        }

        public void SetCredits(long value)
        {
            credits = value;

            Refresh();
        }
    }
}