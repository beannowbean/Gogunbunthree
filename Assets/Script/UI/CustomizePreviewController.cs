using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 독립된 미리보기 관리기
/// - 프리뷰 오브젝트 활성/비활성
/// - 위치/오프셋 정렬
/// - 물리 안정화(Freeze/Sleep) 및 원복
/// - 활성 프리뷰 변경 시 이벤트 발생
/// </summary>
/// <summary>
/// 커스터마이즈 탭에서 미리보기 오브젝트(플레이어, 헬기, 훅 등)를 관리하는 컨트롤러
/// - 프리뷰 오브젝트 활성/비활성, 위치/오프셋 정렬, 물리 안정화, 원복 등 담당
/// </summary>
public class CustomizePreviewController : MonoBehaviour
{
    [Header("Preview Objects")]
    /// <summary>프리뷰용 플레이어 오브젝트</summary>
    public GameObject previewPlayer;
    /// <summary>프리뷰용 헬리콥터 오브젝트</summary>
    public GameObject previewHelicopter;
    /// <summary>프리뷰용 훅 오브젝트</summary>
    public GameObject previewHook;

    // ...existing code...

    // Events
    /// <summary>
    /// 활성화된 프리뷰 오브젝트가 변경될 때 발생하는 이벤트
    /// </summary>
    public event Action<GameObject> OnActivePreviewChanged;

    // Internal state
    /// <summary>현재 활성화된 프리뷰 오브젝트</summary>
    private GameObject activePreview = null;

    // Physics caches
    /// <summary>Rigidbody의 원래 상태를 저장하는 내부 클래스</summary>
    private class RigidbodyState
    {
        public bool wasKinematic;
        public bool wasUseGravity;
        public RigidbodyConstraints prevConstraints;
        public Vector3 velocity;
        public Vector3 angularVelocity;
    }

    /// <summary>Rigidbody별 원래 상태 캐시</summary>
    private Dictionary<Rigidbody, RigidbodyState> cachedRigidbodies = new Dictionary<Rigidbody, RigidbodyState>();
    /// <summary>Collider별 원래 isTrigger 값 캐시</summary>
    private Dictionary<Collider, bool> cachedColliders = new Dictionary<Collider, bool>();
    /// <summary>비활성화된 컴포넌트 목록(원복용)</summary>
    private Dictionary<GameObject, List<MonoBehaviour>> disabledComponents = new Dictionary<GameObject, List<MonoBehaviour>>();
    /// <summary>프리뷰 오브젝트의 원래 트랜스폼 상태</summary>
    private Dictionary<GameObject, PreviewTransformState> previewTransformStates = new Dictionary<GameObject, PreviewTransformState>();
    /// <summary>물리 안정화 강제 코루틴 핸들러</summary>
    private Dictionary<GameObject, Coroutine> enforceCoroutines = new Dictionary<GameObject, Coroutine>();

    /// <summary>트랜스폼 상태 저장용 내부 클래스</summary>
    private class PreviewTransformState
    {
        public Transform parent;
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;
    }

    /// <summary>현재 활성화된 프리뷰 오브젝트 반환</summary>
    public GameObject ActivePreview => activePreview;

    /// <summary>
    /// 전달받은 오브젝트만 활성화, 나머지는 비활성화 및 물리 원복
    /// </summary>
    public void SetActivePreview(GameObject active)
    {
        // Deactivate others
        if (previewPlayer != null && previewPlayer != active)
        {
            previewPlayer.SetActive(false);
            RestorePhysicsState(previewPlayer);
        }
        if (previewHelicopter != null && previewHelicopter != active)
        {
            previewHelicopter.SetActive(false);
            RestorePhysicsState(previewHelicopter);
        }
        if (previewHook != null && previewHook != active)
        {
            previewHook.SetActive(false);
            RestorePhysicsState(previewHook);
        }

        if (active != null)
        {
            active.SetActive(true);
            MakePreviewPhysicsStable(active);

            // start enforcer
            if (enforceCoroutines.TryGetValue(active, out var existing))
            {
                if (existing != null) StopCoroutine(existing);
                enforceCoroutines.Remove(active);
            }
            Coroutine c = StartCoroutine(EnforceStableWhileActive(active));
            enforceCoroutines[active] = c;

            // Idle 애니메이션만 재생
            if (active == previewPlayer)
                SetPlayerAnimation();
        }

        activePreview = active;
        OnActivePreviewChanged?.Invoke(activePreview);
    }
    /// <summary>
    /// 프리뷰 플레이어의 Idle 애니메이션을 재생
    /// </summary>
    private void SetPlayerAnimation()
    {
        if (previewPlayer == null) return;
        var animator = previewPlayer.GetComponentInChildren<Animator>(true);
        if (animator == null) return;

        animator.Play("idle");
    }


    /// <summary>
    /// 현재 활성 프리뷰의 로컬 회전값을 초기화(정면)
    /// </summary>
    public void ResetPreviewRotation()
    {
        if (activePreview != null)
            activePreview.transform.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// 프리뷰 오브젝트의 움직임/물리/AI 컴포넌트 비활성화 및 Rigidbody, Collider 안정화
    /// </summary>
    private void MakePreviewPhysicsStable(GameObject go)
    {
        if (go == null) return;

        // ...기존 위치/정렬/스폰포인트/오프셋 기능 제거됨...

        // Disable movement components
        List<MonoBehaviour> list = new List<MonoBehaviour>();
        var playerComp = go.GetComponentInChildren<Player>(true);
        if (playerComp != null && playerComp.enabled) { list.Add(playerComp); playerComp.enabled = false; }
        var heliComp = go.GetComponentInChildren<Helicopter>(true);
        if (heliComp != null && heliComp.enabled) { list.Add(heliComp); heliComp.enabled = false; }
        var hookComp = go.GetComponentInChildren<Hook>(true);
        if (hookComp != null && hookComp.enabled) { list.Add(hookComp); hookComp.enabled = false; }

        var movers = go.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var m in movers)
        {
            if (m == null) continue;
            var type = m.GetType().Name.ToLower();
            if ((type.Contains("mover") || type.Contains("controller") || type.Contains("movement") || type.Contains("ai")) && m.enabled)
            {
                list.Add(m);
                m.enabled = false;
            }
        }
        if (list.Count > 0) disabledComponents[go] = list;

        // Cache and modify rigidbodies
        Rigidbody[] rbs = go.GetComponentsInChildren<Rigidbody>(true);
        foreach (var rb in rbs)
        {
            if (rb == null) continue;
            if (!cachedRigidbodies.ContainsKey(rb))
            {
                cachedRigidbodies[rb] = new RigidbodyState
                {
                    wasKinematic = rb.isKinematic,
                    wasUseGravity = rb.useGravity,
                    prevConstraints = rb.constraints,
                    velocity = rb.velocity,
                    angularVelocity = rb.angularVelocity
                };
            }

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            rb.Sleep();
        }

        Collider[] cols = go.GetComponentsInChildren<Collider>(true);
        foreach (var col in cols)
        {
            if (col == null) continue;
            if (!cachedColliders.ContainsKey(col)) cachedColliders[col] = col.isTrigger;
            col.isTrigger = true;
        }
    }

    /// <summary>
    /// 프리뷰가 활성화된 동안 물리 상태를 계속 강제로 유지하는 코루틴
    /// </summary>
    private IEnumerator EnforceStableWhileActive(GameObject go)
    {
        while (go != null && activePreview == go && go.activeInHierarchy)
        {
            Rigidbody[] rbs = go.GetComponentsInChildren<Rigidbody>(true);
            foreach (var rb in rbs)
            {
                if (rb == null) continue;
                rb.useGravity = false;
                rb.constraints = RigidbodyConstraints.FreezeAll;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.Sleep();
            }

            Collider[] cols2 = go.GetComponentsInChildren<Collider>(true);
            foreach (var col in cols2)
            {
                if (col == null) continue;
                col.isTrigger = true;
            }

            yield return null;
        }
    }

    /// <summary>
    /// (확장 가능) 프리뷰 오브젝트별 오프셋 반환 (현재 미사용)
    /// </summary>
    private Vector3 GetPreviewOffsetFor(GameObject go)
    {
        return Vector3.zero;
    }

    /// <summary>
    /// 프리뷰 오브젝트의 물리/트랜스폼/컴포넌트 상태를 원래대로 복구
    /// </summary>
    private void RestorePhysicsState(GameObject go)
    {
        if (go == null) return;

        Rigidbody[] rbs = go.GetComponentsInChildren<Rigidbody>(true);
        foreach (var rb in rbs)
        {
            if (rb == null) continue;
            if (cachedRigidbodies.TryGetValue(rb, out var state))
            {
                rb.constraints = state.prevConstraints;
                rb.useGravity = state.wasUseGravity;
                rb.isKinematic = state.wasKinematic;
                if (!state.wasKinematic)
                {
                    rb.velocity = state.velocity;
                    rb.angularVelocity = state.angularVelocity;
                }
                cachedRigidbodies.Remove(rb);
            }
        }

        Collider[] cols = go.GetComponentsInChildren<Collider>(true);
        foreach (var col in cols)
        {
            if (col == null) continue;
            if (cachedColliders.TryGetValue(col, out var wasTrigger))
            {
                col.isTrigger = wasTrigger;
                cachedColliders.Remove(col);
            }
        }

        if (disabledComponents.TryGetValue(go, out var comps))
        {
            foreach (var c in comps)
            {
                if (c != null) c.enabled = true;
            }
            disabledComponents.Remove(go);
        }

        if (previewTransformStates.TryGetValue(go, out var tstate))
        {
            go.transform.SetParent(tstate.parent, false);
            go.transform.localPosition = tstate.localPosition;
            go.transform.localRotation = tstate.localRotation;
            go.transform.localScale = tstate.localScale;
            previewTransformStates.Remove(go);
        }

        if (enforceCoroutines.TryGetValue(go, out var coroutine))
        {
            if (coroutine != null) StopCoroutine(coroutine);
            enforceCoroutines.Remove(go);
        }

        if (activePreview == go) activePreview = null;
        OnActivePreviewChanged?.Invoke(activePreview);
    }

    /// <summary>
    /// 모든 프리뷰 오브젝트의 상태를 원래대로 복구 (UI 닫을 때 등)
    /// </summary>
    public void RestoreAllModifiedPreviews()
    {
        // Hide preview
        OnActivePreviewChanged?.Invoke(null);

        var rbs = new List<Rigidbody>(cachedRigidbodies.Keys);
        foreach (var rb in rbs)
        {
            if (rb == null) continue;
            var state = cachedRigidbodies[rb];
            rb.constraints = state.prevConstraints;
            rb.useGravity = state.wasUseGravity;
            rb.isKinematic = state.wasKinematic;
            if (!state.wasKinematic)
            {
                rb.velocity = state.velocity;
                rb.angularVelocity = state.angularVelocity;
            }
        }
        cachedRigidbodies.Clear();

        var enforcers = new List<GameObject>(enforceCoroutines.Keys);
        foreach (var k in enforcers)
        {
            if (enforceCoroutines[k] != null) StopCoroutine(enforceCoroutines[k]);
            enforceCoroutines.Remove(k);
        }

        var cols = new List<Collider>(cachedColliders.Keys);
        foreach (var col in cols)
        {
            if (col == null) continue;
            col.isTrigger = cachedColliders[col];
        }
        cachedColliders.Clear();

        var disabledKeys = new List<GameObject>(disabledComponents.Keys);
        foreach (var key in disabledKeys)
        {
            var comps = disabledComponents[key];
            foreach (var c in comps)
            {
                if (c != null) c.enabled = true;
            }
            disabledComponents.Remove(key);
        }

        var tkeys = new List<GameObject>(previewTransformStates.Keys);
        foreach (var key in tkeys)
        {
            var tstate = previewTransformStates[key];
            if (key != null)
            {
                key.transform.SetParent(tstate.parent, false);
                key.transform.localPosition = tstate.localPosition;
                key.transform.localRotation = tstate.localRotation;
                key.transform.localScale = tstate.localScale;
            }
            previewTransformStates.Remove(key);
        }
    }
}
