using UnityEngine;

[CreateAssetMenu(fileName = "New Target Data", menuName = "Game/Target Data")]
public class TargetData : ScriptableObject
{
    [Header("기본 정보")]
    public string targetName;
    public Color trailColor = Color.white;
    public float dashSpeedMultiplier = 1.0f;

    [Header("검 타입")]
    [Tooltip("이 검이 사용할 특수 동작 타입")]
    public SwordType swordType = SwordType.Normal;

    [Header("발사체 검 설정 (Projectile 타입일 때만 사용)")]
    [Tooltip("발사할 구체 프리팹")]
    public GameObject projectilePrefab;
    [Tooltip("발사할 구체 개수")]
    public int projectileCount = 4;
    [Tooltip("구체 이동 속도")]
    public float projectileSpeed = 10f;
    [Tooltip("구체 데미지")]
    public int projectileDamage = 15;
    [Tooltip("플레이어로부터 발사체 소환 거리 (4방향 오프셋)")]
    public float spawnOffset = 1.0f;

    [Header("발도 검 설정 (Battoujutsu 타입일 때만 사용)")]
    [Tooltip("도착 후 회전 공격 반경")]
    public float spinRadius = 2.5f;
    [Tooltip("회전 공격 데미지")]
    public int spinDamage = 30;
    [Tooltip("회전 연출 지속 시간 (초)")]
    public float spinDuration = 0.4f;
}

/// <summary>
/// 검 타입 목록. 새 검을 추가할 때 여기에만 enum 값 하나 추가하면 됩니다.
/// </summary>
public enum SwordType
{
    Normal,         // 기본 베기
    Projectile,     // 발사체 검
    Battoujutsu,    // 발도 검
}
