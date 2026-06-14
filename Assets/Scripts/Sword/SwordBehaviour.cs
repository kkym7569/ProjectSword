using UnityEngine;

/// <summary>
/// 모든 검 특수 동작의 추상 기반 클래스.
///
/// [새 검 추가 방법]
/// 1. SwordType enum에 새 타입 추가 (TargetData.cs)
/// 2. 이 클래스를 상속한 새 스크립트 작성
/// 3. SwordBehaviourFactory.Create() 에 case 한 줄 추가
/// → 그 외 기존 코드는 전혀 수정할 필요 없음
/// </summary>
public abstract class SwordBehaviour : MonoBehaviour
{
    protected TargetData data;
    protected PlayerCombat playerCombat;

    /// <summary>TargetManager가 컴포넌트를 붙인 직후 호출합니다.</summary>
    public void Init(TargetData targetData, PlayerCombat combat)
    {
        data         = targetData;
        playerCombat = combat;
        OnInit();
    }

    /// <summary>초기화 완료 후 서브클래스가 추가 세팅을 할 수 있는 훅.</summary>
    protected virtual void OnInit() { }

    /// <summary>
    /// 플레이어가 이 검을 향해 대시를 시작하기 직전에 호출됩니다.
    /// (이동 출발 전 자리에서 실행할 것들을 여기에)
    /// </summary>
    public virtual void OnBeforeAttackMove(Vector2 playerPos) { }

    /// <summary>
    /// 플레이어가 이 검에 도착(수집)했을 때 호출됩니다.
    /// (도착 후 실행할 것들을 여기에)
    /// </summary>
    public virtual void OnAfterAttackMove(Vector2 arrivalPos) { }
}
