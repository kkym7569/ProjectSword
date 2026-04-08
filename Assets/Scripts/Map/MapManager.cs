using UnityEngine;

public static class MapManager
{
    // 노이즈의 크기 조절 (값이 작을수록 지형이 완만하게 넓어집니다)
    public static float noiseScale = 0.1f;

    // 맵 시드 (이 값이 같으면 항상 똑같은 모양의 무한 맵이 나옵니다)
    public static float seedX = 1234.5f;
    public static float seedY = 5432.1f;

    // 핵심 함수: X, Y 좌표를 넣으면 타일 종류를 뱉어내는 자판기
    public static TileType GetTileAt(int x, int y)
    {
        // 1. 유니티의 펄린 노이즈 함수 사용 (0.0 ~ 1.0 사이의 실수 반환)
        float noiseValue = Mathf.PerlinNoise((x + seedX) * noiseScale, (y + seedY) * noiseScale);

        // 2. 결괏값에 따라 장판 판별 (규칙 적용)
        if (noiseValue < 0.3f)
        {
            return TileType.Water; // 30% 확률 (낮은 지대)
        }
        else if (noiseValue < 0.7f)
        {
            return TileType.Grass; // 40% 확률 (중간 지대)
        }
        else
        {
            return TileType.Lava;  // 30% 확률 (높은 지대)
        }
    }
}