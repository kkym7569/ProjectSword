using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattoujutsuSword : SwordBehaviour
{
    public override void OnAfterAttackMove(Vector2 arrivalPos)
    {
        playerCombat.StartCoroutine(SlashRotateRoutine(arrivalPos));
    }

    private IEnumerator SlashRotateRoutine(Vector2 center)
    {
        float duration = data.spinDuration;
        float timer = 0f;
        float angleOffset = 0f;
        float rotateSpeed = 720f / data.spinDuration;

        HashSet<EnemyBase> alreadyHit = new HashSet<EnemyBase>();

        float rectWidth = 0.35f;
        float rectInner = 1.0f;
        float rectOuter = 0.15f;
        int tailLength = 40;

        // ── 꼬리: 각 슬롯을 GL 드로우 대신 LineRenderer 4선분으로 그림 ──
        // 사각형 테두리 + 대각선 2개로 채운 것처럼 보이게
        LineRenderer[] tails = new LineRenderer[tailLength];
        for (int i = 0; i < tailLength; i++)
        {
            GameObject obj = new GameObject($"Tail_{i}");
            LineRenderer lr = obj.AddComponent<LineRenderer>();

            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.positionCount = 5;   // 사각형 외곽선 (닫힌 루프)
            lr.loop = false;
            lr.useWorldSpace = true;
            lr.startWidth = rectWidth * 2f;
            lr.endWidth = rectWidth * 2f;
            lr.numCornerVertices = 2;
            lr.sortingLayerName = "Target";
            lr.sortingOrder = 200;
            lr.startColor = Color.clear;
            lr.endColor = Color.clear;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            tails[i] = lr;
            obj.SetActive(false);
        }

        Queue<RectSnapshot> snapshots = new Queue<RectSnapshot>();

        while (timer < duration)
        {
            timer += Time.deltaTime;
            angleOffset += rotateSpeed * Time.deltaTime;

            float rad = angleOffset * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            Vector2 perp = new Vector2(-dir.y, dir.x);
            Vector2 rectCenter = (Vector2)center + dir * data.spinRadius;
            Vector2 innerDir = ((Vector2)center - rectCenter).normalized;

            snapshots.Enqueue(new RectSnapshot
            {
                center = rectCenter,
                innerDir = innerDir,
                perp = perp,
                angle = angleOffset,
            });
            if (snapshots.Count > tailLength) snapshots.Dequeue();

            RectSnapshot[] arr = snapshots.ToArray();

            for (int i = 0; i < tailLength; i++)
            {
                int dataIdx = arr.Length - 1 - i;
                if (dataIdx < 0) { tails[i].gameObject.SetActive(false); continue; }

                float t = (float)i / tailLength;
                float alpha = Mathf.Lerp(1f, 0f, t);
                float scale = Mathf.Lerp(1f, 0.15f, t);

                RectSnapshot s = arr[dataIdx];
                Color col = new Color(data.trailColor.r, data.trailColor.g,
                                      data.trailColor.b, alpha);

                float w = rectWidth * scale;
                float ri = rectInner * scale;
                float ro = rectOuter * scale;

                Vector3 v0 = s.center + s.perp * w + (Vector2)(s.innerDir * ri);
                Vector3 v1 = s.center - s.perp * w + (Vector2)(s.innerDir * ri);
                Vector3 v2 = s.center - s.perp * w - (Vector2)(s.innerDir * ro);
                Vector3 v3 = s.center + s.perp * w - (Vector2)(s.innerDir * ro);

                LineRenderer lr = tails[i];
                lr.gameObject.SetActive(true);

                // 사각형을 선으로 채움: 대각선 방향으로 왔다갔다 그려서 면처럼 보이게
                lr.positionCount = 6;
                lr.SetPosition(0, v0);
                lr.SetPosition(1, v2);  // 대각선
                lr.SetPosition(2, v1);
                lr.SetPosition(3, v3);  // 대각선
                lr.SetPosition(4, v0);
                lr.SetPosition(5, v1);

                // 선 굵기 = 사각형 너비만큼 → 내부가 채워져 보임
                float lineW = w * 2.2f;
                lr.startWidth = lineW;
                lr.endWidth = lineW;

                lr.startColor = col;
                lr.endColor = col;
                lr.material.color = col;
            }

            // 타격 판정
            Collider2D[] hits = Physics2D.OverlapBoxAll(
                rectCenter,
                new Vector2(rectWidth * 2f, rectInner + rectOuter),
                angleOffset,
                LayerMask.GetMask("Enemy"));

            foreach (var col in hits)
            {
                EnemyBase enemy = col.GetComponent<EnemyBase>();
                if (enemy == null || enemy.IsDead || alreadyHit.Contains(enemy)) continue;
                alreadyHit.Add(enemy);
                enemy.TakeDamage(data.spinDamage);
            }

            yield return null;
        }

        foreach (var lr in tails)
            if (lr != null) Object.Destroy(lr.gameObject);

        Debug.Log($"[BattoujutsuSword] 회전 베기 완료 — {alreadyHit.Count}명 적중");
    }

    private struct RectSnapshot
    {
        public Vector2 center, innerDir, perp;
        public float angle;
    }
}
