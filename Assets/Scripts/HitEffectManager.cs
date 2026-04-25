using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class HitEffectManager : MonoBehaviour
{
    public static HitEffectManager Instance;

    [Header("Settings")]
    public float stopDuration = 0.4f;
    public Color worldRedColor = Color.red;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlaySpecialKillEffect(List<EnemyBase> victims, GameObject player, LineRenderer slash)
    {
        StartCoroutine(SpecialEffectRoutine(victims, player, slash));
    }

    private IEnumerator SpecialEffectRoutine(List<EnemyBase> victims, GameObject player, LineRenderer slash)
    {
        // 1. 시간 정지
        Time.timeScale = 0f;

        Dictionary<SpriteRenderer, Color> spriteColors = new Dictionary<SpriteRenderer, Color>();
        Dictionary<Tilemap, Color> tilemapColors = new Dictionary<Tilemap, Color>();

        SpriteRenderer[] allSprites = FindObjectsOfType<SpriteRenderer>();
        Tilemap[] allTilemaps = FindObjectsOfType<Tilemap>();

        // 2. 색상 강제 변경
        foreach (var sr in allSprites)
        {
            if (sr == null) continue;

            EnemyBase eb = sr.GetComponentInParent<EnemyBase>();
            // 피격 중일 수 있으므로 항상 기본 색상 저장
            Color colorToRestore = (eb != null) ? eb.GetOriginalColor() : sr.color;
            spriteColors[sr] = colorToRestore;

            // A. 주인공 또는 이번에 죽는 적들 -> 검은색
            if (sr.gameObject == player || sr.transform.IsChildOf(player.transform) || IsVictim(sr, victims))
            {
                sr.color = Color.black;
            }
            // B. 나머지 배경 -> 빨간색
            else
            {
                sr.color = worldRedColor;
            }
        }

        foreach (var tm in allTilemaps)
        {
            if (tm == null) continue;
            tilemapColors[tm] = tm.color;
            tm.color = worldRedColor;
        }

        if (slash != null)
        {
            slash.startColor = Color.black;
            slash.endColor = Color.black;
        }

        // 3. 실제 시간 기준으로 대기
        yield return new WaitForSecondsRealtime(stopDuration);

        // 4. 시간 재개 및 색상 복구
        Time.timeScale = 1f;

        foreach (var pair in spriteColors)
        {
            if (pair.Key != null) pair.Key.color = pair.Value;
        }

        foreach (var pair in tilemapColors)
        {
            if (pair.Key != null) pair.Key.color = pair.Value;
        }

        if (slash != null)
        {
            slash.startColor = Color.white;
            slash.endColor = Color.white;
        }
    }

    private bool IsVictim(SpriteRenderer sr, List<EnemyBase> victims)
    {
        foreach (var v in victims)
        {
            if (v != null && (sr.gameObject == v.gameObject || sr.transform.IsChildOf(v.transform)))
                return true;
        }
        return false;
    }
}