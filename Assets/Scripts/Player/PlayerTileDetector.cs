using UnityEngine;
using UnityEngine.Tilemaps; // 🌟 WorldToCell을 쓰기 위해 반드시 추가!

public class PlayerTileDetector : MonoBehaviour
{
    [Header("타일맵 연결")]
    public Tilemap tilemap; // 인스펙터에서 씬에 있는 타일맵을 드래그해서 연결해주세요

    [Header("현재 상태 (Read Only)")]
    public TileType currentTile;

    private TileType lastLoggedTile = TileType.None; // 로그 도배 방지

    void Update()
    {
        // 타일맵이 연결되지 않았을 때의 에러 방지
        if (tilemap == null) return;

        // 1. 유니티 공식 함수를 사용해 실제 위치를 정확한 그리드 좌표로 변환! (핵심 수정 부분)
        Vector3Int gridPos = tilemap.WorldToCell(transform.position);

        // 2. 맵 매니저에 현재 좌표를 넣어서 밟고 있는 장판 확인
        currentTile = MapManager.GetTileAt(gridPos.x, gridPos.y);

        // 🌟 [디버그 로그] 타일이 바뀔 때만 콘솔에 출력해서 정확히 확인
        if (currentTile != lastLoggedTile)
        {
            Debug.Log($"<color=cyan>[타일 인식기]</color> 타일맵 좌표 ({gridPos.x}, {gridPos.y}) ➡ 밟은 바닥: {currentTile}");
            lastLoggedTile = currentTile;
        }

        // 3. 장판에 따른 효과 적용 함수 호출
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