using UnityEngine;

public class PlayerTileDetector : MonoBehaviour
{
    public TileType currentTile;

    void Update()
    {
        // 1. 플레이어의 현재 실제 위치(World Position) 가져오기
        Vector2 pos = transform.position;

        // 2. 실제 위치를 정수형 그리드 좌표로 변환 (반올림 처리)
        // 주의: 아이소메트릭 뷰의 경우, 그리드 모양에 따라 별도의 수학적 변환 공식이 필요할 수 있습니다.
        int gridX = Mathf.RoundToInt(pos.x);
        int gridY = Mathf.RoundToInt(pos.y);

        // 3. 맵 매니저(자판기)에 현재 좌표를 넣어서 밟고 있는 장판 확인
        currentTile = MapManager.GetTileAt(gridX, gridY);

        // 4. 장판에 따른 효과 적용 함수 호출
        ApplyFloorEffect(currentTile);
    }

    void ApplyFloorEffect(TileType floor)
    {
        switch (floor)
        {
            case TileType.Water:
                // 이동 속도 감소 로직
                break;
            case TileType.Lava:
                // 체력 감소 로직
                break;
            case TileType.Grass:
                // 정상 속도
                break;
        }
    }
}