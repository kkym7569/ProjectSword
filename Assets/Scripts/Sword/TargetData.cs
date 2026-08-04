using UnityEngine;

[CreateAssetMenu(fileName = "New Target Data", menuName = "Game/Target Data")]
public class TargetData : ScriptableObject
{
    public string targetName;
    public Color trailColor = Color.white; // 이 검의 고유 색상
    public float dashSpeedMultiplier = 1.0f; // 이 검을 탈 때의 속도 배율
    // 데미지, 이펙트 등 원하는 속성을 계속 추가할 수 있습니다.
}