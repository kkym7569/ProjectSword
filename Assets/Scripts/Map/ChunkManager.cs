using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps; // 타일맵 제어를 위해 필수

public class ChunkManager : MonoBehaviour
{
    [Header("타일맵 참조")]
    public Tilemap tilemap; // 화면에 그림을 그릴 도화지

    [Header("타일 에셋 (미리 인스펙터에서 연결)")]
    public TileBase waterTile;
    public TileBase grassTile;
    public TileBase lavaTile;

    [Header("청크 설정")]
    public int chunkSize = 16; // 한 청크의 가로세로 타일 개수 (16x16)
    public int renderDistance = 1; // 플레이어 주변 몇 개의 청크를 그릴 것인가 (1이면 3x3, 2면 5x5)

    public Transform player; // 플레이어 위치 참조용

    // 현재 그려져 있는 청크들을 기억하는 딕셔너리 (Key: 청크의 좌표)
    private HashSet<Vector2Int> activeChunks = new HashSet<Vector2Int>();
    private Vector2Int currentPlayerChunk = new Vector2Int(9999, 9999); // 초기값 (무조건 업데이트 되도록)

    void Update()
    {
        // 1. 플레이어의 현재 '청크 좌표'를 계산
        Vector2Int newChunkPos = GetChunkPosition(player.position);

        // 2. 플레이어가 새로운 청크로 넘어갔을 때만 맵을 갱신 (매 프레임 그리지 않음! 핵심 최적화)
        if (newChunkPos != currentPlayerChunk)
        {
            currentPlayerChunk = newChunkPos;
            UpdateChunks();
        }
    }

    // 실제 월드 좌표를 '청크 좌표'로 변환하는 함수
    Vector2Int GetChunkPosition(Vector3 worldPos)
    {
        // 아이소메트릭의 경우 타일맵 컴포넌트의 WorldToCell을 사용해 먼저 그리드 좌표를 얻는 것이 정확합니다.
        Vector3Int gridPos = tilemap.WorldToCell(worldPos);

        int chunkX = Mathf.FloorToInt((float)gridPos.x / chunkSize);
        int chunkY = Mathf.FloorToInt((float)gridPos.y / chunkSize);

        return new Vector2Int(chunkX, chunkY);
    }

    // 주변 청크를 그리고, 멀어진 청크는 지우는 함수
    void UpdateChunks()
    {
        HashSet<Vector2Int> newActiveChunks = new HashSet<Vector2Int>();

        // 플레이어 주변(renderDistance)의 청크 좌표들을 계산
        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            for (int y = -renderDistance; y <= renderDistance; y++)
            {
                Vector2Int chunkPos = new Vector2Int(currentPlayerChunk.x + x, currentPlayerChunk.y + y);
                newActiveChunks.Add(chunkPos);

                // 만약 새로 그려야 하는 청크라면 (기존에 안 그려져 있었다면)
                if (!activeChunks.Contains(chunkPos))
                {
                    GenerateChunk(chunkPos);
                }
            }
        }

        // 기존에는 그려져 있었는데, 이제는 시야에서 벗어난 청크 지우기
        foreach (Vector2Int oldChunk in activeChunks)
        {
            if (!newActiveChunks.Contains(oldChunk))
            {
                ClearChunk(oldChunk);
            }
        }

        // 활성화된 청크 목록 갱신
        activeChunks = newActiveChunks;
    }

    // 수학 공식 자판기를 이용해 청크 하나(16x16 타일)를 타일맵에 그리는 함수
    void GenerateChunk(Vector2Int chunkPos)
    {
        // 청크의 실제 시작 타일 좌표
        int startX = chunkPos.x * chunkSize;
        int startY = chunkPos.y * chunkSize;

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                int tileX = startX + x;
                int tileY = startY + y;

                // 저번에 만든 '수학 공식 자판기'에 좌표를 넣어서 장판 종류를 얻어옴
                TileType type = MapManager.GetTileAt(tileX, tileY);
                TileBase tileToSet = null;

                switch (type)
                {
                    case TileType.Water: tileToSet = waterTile; break;
                    case TileType.Grass: tileToSet = grassTile; break;
                    case TileType.Lava: tileToSet = lavaTile; break;
                }

                // 타일맵 도화지에 해당 타일 칠하기
                tilemap.SetTile(new Vector3Int(tileX, tileY, 0), tileToSet);
            }
        }
    }

    // 시야에서 멀어진 청크의 타일들을 지우는 함수
    void ClearChunk(Vector2Int chunkPos)
    {
        int startX = chunkPos.x * chunkSize;
        int startY = chunkPos.y * chunkSize;

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                // null을 세팅하면 타일이 지워짐 (메모리 확보)
                tilemap.SetTile(new Vector3Int(startX + x, startY + y, 0), null);
            }
        }
    }
}