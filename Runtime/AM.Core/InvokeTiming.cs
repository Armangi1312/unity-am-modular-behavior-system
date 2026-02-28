using System;

namespace AM.Core
{
    /// <summary>
    /// 실행과 호출하는 타이밍을 설정하는 열거형입니다.
    /// </summary>
    [Flags]
    public enum InvokeTiming
    {
        /// <summary>
        /// 다른 플래그가 있어도 호출하지 않습니다.
        /// </summary>
        DoNotInvoke = 1 << 0,

        /// <summary>
        /// 초기화할 때 (유니티 이벤트)
        /// </summary>
        Awake = 1 << 1,

        /// <summary>
        /// 시작할 때 (유니티 이벤트)
        /// </summary>
        Start = 1 << 2,

        /// <summary>
        /// 매 프레임 마다 (유니티 이벤트)
        /// </summary>
        Update = 1 << 3,

        /// <summary>
        /// 고정된 프레임 마다(보통 0.02초. 프로젝트 세팅마다 다름.) (유니티 이벤트)
        /// </summary>
        FixedUpdate = 1 << 4,

        /// <summary>
        /// Update가 호출되고 나서 (유니티 이벤트)
        /// </summary>
        LateUpdate = 1 << 5,

        /// <summary>
        /// 오브젝트가 파괴될 때 때(유니티 이벤트)
        /// </summary>
        Destroy = 1 << 6,

        /// <summary>
        /// Processor을 호출하는 컴포넌트(Controller)가 활성화될 때
        /// </summary>
        OnEnable = 1 << 7,

        /// <summary>
        /// Processor을 호출하는 컴포넌트(Controller)가 비활성화될 때
        /// </summary>
        OnDisable = 1 << 8,
    }
}
