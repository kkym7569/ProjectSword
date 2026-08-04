using UnityEngine;

/// <summary>
/// SwordType에 맞는 SwordBehaviour 컴포넌트를 생성해주는 팩토리.
/// 새 검을 추가할 때 case 한 줄만 추가하면 됩니다.
/// </summary>
public static class SwordBehaviourFactory
{
    /// <summary>
    /// target 오브젝트에 swordType에 맞는 SwordBehaviour를 붙이고 Init한 뒤 반환합니다.
    /// Normal 타입이면 null을 반환합니다 (특수 동작 없음).
    /// </summary>
    public static SwordBehaviour Create(GameObject target, TargetData data, PlayerCombat combat)
    {
        SwordBehaviour behaviour = null;

        switch (data.swordType)
        {
            case SwordType.Normal:
                // 기본 베기 - 특수 동작 없음
                break;

            case SwordType.Projectile:
                behaviour = target.AddComponent<ProjectileSword>();
                break;

            case SwordType.Battoujutsu:
                behaviour = target.AddComponent<BattoujutsuSword>();
                break;

            // ★ 새 검 추가 시 여기에 case 한 줄만 추가하면 됩니다.
            // case SwordType.YourNewSword:
            //     behaviour = target.AddComponent<YourNewSword>();
            //     break;
        }

        behaviour?.Init(data, combat);
        return behaviour;
    }
}
