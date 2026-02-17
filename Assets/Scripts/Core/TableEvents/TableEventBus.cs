using System;

namespace PlinkoPinball.Core.TableEvents
{
    /// <summary>
    /// 테이블 이벤트 디스패처(버스)
    /// - 테이블의 각 요소는 버스로 이벤트를 발행함
    /// - 시스템(점수, 시간, 연출 등)을 구독해서 처리
    /// 
    /// 추후 DI/ServiceLocator로 교체 가능 (기능이 커질때)
    /// </summary>
    public static class TableEventBus
    {
        // 테이블 이벤트가 발생하 때마다 호출됨
        public static event Action<TableEvent> OnEvent; 

        public static void Publish(in TableEvent e)
        {
            OnEvent?.Invoke(e);
        }
    }
}
