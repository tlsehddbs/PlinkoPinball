namespace PlinkoPinball.Core.TableEvents
{
    /// <summary>
    /// 테이블에서 발생하는 이벤트를 분류
    /// </summary>
    public enum TableEventType
    {
        Hit = 0,        // 충돌(물리적인 접촉)
        Pass = 1,       // (레인/ 게이트 등) 트리거 (통과)
        Switch = 2,     // 스위치 (테이블의 요소 등의 상태 변화를 확인)
        Zone = 3        // 특정 구역에 진입/이탈
    }
}
