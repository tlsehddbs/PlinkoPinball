using System;
using UnityEngine;

namespace PlinkoPinball.Services
{
    /// <summary>
    /// 매 라운드의 제한시간을 관리하는 서비스
    /// 
    /// 설계 의도: 
    /// 1) MonoBehaviour가 아닌 순수 클래스로 만들어 테스트/재사용/이식을 쉽게함.
    /// 2) 시간 추가/패널티/일시정지 같은 규칙이 한 곳에 집약하도록 함.
    /// 3) 타이머 관련 UI/사운드는 "이벤트"만을 구독하도록 함.
    /// 
    /// - OnTimeChanged를 시간 간격으로 제어해 직관적으로 구현
    /// </ummary>
    public sealed class TimeManager : ITickable
    {
        public event Action<float> OnTimeChanged;
        public event Action OnTimeOver;

        public float RemainingSeconds { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsPaused { get; private set; }
        // TimeOver 이벤트가 이미 발생했는지를 확인(중복 방지
        public bool HasTimeOverFired { get; private set; }

        // 이벤트 Broadcast 간격 (0이면 매 tick 마다 broadcast)
        private readonly float _notifyIntervalSeconds;
        private float _notifyAccumulator;


        public TimeManager(float notifyIntervalSeconds = 0f)
        {
            _notifyIntervalSeconds = Mathf.Max(0f, notifyIntervalSeconds);
        }

        // 라운드 시작 시 타이머 초기화
        public void Start(float startSeconds)
        {
            RemainingSeconds = Math.Max(0f, startSeconds);

            IsRunning = true;
            IsPaused = false;
            HasTimeOverFired = false;

            _notifyAccumulator = 0f;

            NotifyTimeChanged(force: true);
        }

        // 라운드 종료 등 타이머 정지
        public void Stop()
        {
            IsRunning = false;
            IsPaused = false;
        }

        // 메뉴, (보너스 룸(예정)) 등 타이머 일시정지, 해제
        public void SetPaused(bool paused)
        {
            IsPaused = paused;

            // 일시정지 토글 사용시 HUD가 바로 반응하도록 설정하려면 사용
            // NotifyTimeChanged(force: true);
        }

        // 시간 추가
        public void AddTime(float seconds)
        {
            if (!IsRunning) return;
            if (seconds <= 0f) return;

            RemainingSeconds += seconds;

            NotifyTimeChanged(force: true);
        }

        // 패널티
        public void AddPenalty(float seconds)
        {
            if (!IsRunning) return;
            if (seconds <= 0f) return;

            RemainingSeconds = Math.Max(0f, RemainingSeconds - seconds);

            NotifyTimeChanged(force: true);

            TryFireTimeOver();  // 패널티로 타이머가 0이 될 수 있으므로 체크
        }

        // 매 프레임 호출
        public void Tick(float deltaTime)
        {
            if (!IsRunning) return;
            if (IsPaused) return;
            if (HasTimeOverFired) return;
            if (deltaTime <= 0f) return;

            RemainingSeconds -= deltaTime;

            if (RemainingSeconds <= 0f)
            {
                RemainingSeconds = 0f;
                
                NotifyTimeChanged(force: true);
                TryFireTimeOver();

                return;
            }

            // 이벤트 broadcast 통지 간격 누적
            if (_notifyIntervalSeconds <= 0f)
            {
                // 0이면 매 Tick마다 (정밀한 HUD 업데이트가 필요할 때 사용)
                NotifyTimeChanged(force: true);
            }
            else
            {
                _notifyAccumulator += deltaTime;
                if (_notifyAccumulator >= _notifyIntervalSeconds)
                {
                    // 누적이 간격을 넘으면 broadcast, 누적을 남겨서 드리프트를 줄임
                    _notifyAccumulator -= _notifyIntervalSeconds;
                    NotifyTimeChanged(force: true);
                } 
            }
        }

        private void TryFireTimeOver()
        {
            if (HasTimeOverFired) return;
            if (RemainingSeconds > 0f) return;

            HasTimeOverFired = true;
            IsRunning = false;      // 타이머 종료 시점에 자동 정지

            OnTimeOver?.Invoke();
        }

        /// <param name="force">즉시 업데이트가 필요할 경우 사용</param>
        private void NotifyTimeChanged(bool force)
        {
            if (!force) return;
            OnTimeChanged?.Invoke(RemainingSeconds);
        }
    }
}