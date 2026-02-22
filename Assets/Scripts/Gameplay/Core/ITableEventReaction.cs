using PlinkoPinball.Core.TableEvents;

namespace PlinkoPinball.Gameplay.Core
{
    /// <summary>
    /// 로컬 반응자(옵저버 패턴)
    /// Trigger가 발동되면 GameObject에 붙어있는 이 인터페이스 구현체들에게 이벤트를 전달
    /// </summary>
    public interface ITableEventReaction
    {
        void OnTableEvent(in TableEvent e);
    }
}
