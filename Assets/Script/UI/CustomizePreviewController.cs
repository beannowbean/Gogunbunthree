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
public class CustomizePreviewController : MonoBehaviour
{
    [Header("Preview Objects")]
    public GameObject previewPlayer;
    public GameObject previewHelicopter;
    public GameObject previewHook;

    // ...existing code...

    // Events
    public event Action<GameObject> OnActivePreviewChanged;

    // Internal state
    private GameObject activePreview = null;

    // Physics caches
    private class RigidbodyState
    {
        public bool wasKinematic;
        public bool wasUseGravity;
        public RigidbodyConstraints prevConstraints;
        public Vector3 velocity;
        public Vector3 angularVelocity;
    }

    private Dictionary<Rigidbody, RigidbodyState> cachedRigidbodies = new Dictionary<Rigidbody, RigidbodyState>();
    private Dictionary<Collider, bool> cachedColliders = new Dictionary<Collider, bool>();
    private Dictionary<GameObject, List<MonoBehaviour>> disabledComponents = new Dictionary<GameObject, List<MonoBehaviour>>();
    private Dictionary<GameObject, PreviewTransformState> previewTransformStates = new Dictionary<GameObject, PreviewTransformState>();
    private Dictionary<GameObject, Coroutine> enforceCoroutines = new Dictionary<GameObject, Coroutine>();

    private class PreviewTransformState
    {
        public Transform parent;
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;
    }

    public GameObject ActivePreview => activePreview;

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
        }

        activePreview = active;
        OnActivePreviewChanged?.Invoke(activePreview);
    }

    public void ResetPreviewRotation()
    {
        if (activePreview != null)
            activePreview.transform.localRotation = Quaternion.identity;
    }

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

    private Vector3 GetPreviewOffsetFor(GameObject go)
    {
        return Vector3.zero;
    }

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
