using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

/// <summary>
/// Customize UI 컨트롤러
/// - Inspector에서 Cancel/Apply 버튼을 연결하세요.
/// - 버튼 클릭 시 Inspector의 Button.OnClick에 메서드를 연결하거나 코드에서 리스너를 등록하세요.
/// - Apply: 현재 적용된 스킨/아이템을 PlayerPrefs에 저장합니다.
/// - Cancel: 패널 열기 시의 상태로 되돌립니다.
/// </summary>
public class CustomizeUI : MonoBehaviour
{
    [Header("Skin List UI")]
    public Transform skinListContent; // 스크롤뷰 Content
    public GameObject skinButtonPrefab; // 버튼 프리팹 (Image 포함)

    // 프리뷰 타입별 스킨 리스트 동적 생성
    public void ShowSkinList(string type)
    {
        Debug.Log($"ShowSkinList called: {type} on {gameObject.name}");

        // 안전성 검사
        if (Customize.Instance == null)
        {
            Debug.LogError("Customize.Instance is null — ensure Customize script exists in scene and Awake ran.");
            return;
        }

        if (skinListContent == null)
        {
            Debug.LogError("skinListContent is not assigned on CustomizeUI.");
            return;
        }
        if (skinButtonPrefab == null)
        {
            Debug.LogError($"skinButtonPrefab is not assigned on CustomizeUI (GameObject: {gameObject.name}).");
            return;
        }

        // 기존 버튼 삭제 (단, skinButtonPrefab이 Content의 자식으로 할당된 경우 해당 오브젝트는 파괴하지 않고 비활성화하여
        // 참조가 끊어지는 문제를 방지합니다.)
        int removed = 0;
        for (int i = skinListContent.childCount - 1; i >= 0; i--)
        {
            var child = skinListContent.GetChild(i);
            if (skinButtonPrefab != null && child.gameObject == skinButtonPrefab)
            {
                // 템플릿으로 사용 중인 씬 오브젝트는 파괴하지 말고 비활성화
                if (child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(false);
                    Debug.Log($"Preserved and deactivated skinButtonPrefab in content at index {i}.");
                }
                continue;
            }
            Destroy(child.gameObject);
            removed++;
        }
        Debug.Log($"Cleared existing {removed} children of skinListContent (preserved prefab if present).");

        List<Texture> textureList = null;
        List<Material> materialList = null;
        List<Sprite> spriteList = null;
        System.Action<int> onClick = null;

        if (type == "Player") {
            textureList = Customize.Instance.playerSkins;
            spriteList = Customize.Instance.playerSkinIcons;
            onClick = (idx) => Customize.Instance.EquipPlayerSkinNumber(idx);
        }
        else if (type == "Helicopter") {
            textureList = Customize.Instance.helicopterSkins;
            spriteList = Customize.Instance.helicopterSkinIcons;
            onClick = (idx) => Customize.Instance.EquipHelicopterSkinNumber(idx);
        }
        else if (type == "Hook") {
            materialList = Customize.Instance.hookSkins;
            spriteList = Customize.Instance.hookSkinIcons;
            onClick = (idx) => Customize.Instance.EquipHookSkinNumber(idx);
        }
        else if (type == "Beanie") {
            textureList = Customize.Instance.beanieSkins;
            spriteList = Customize.Instance.beanieSkinIcons;
            onClick = (idx) => {
                Customize.Instance.EquipBeanie();
                Customize.Instance.ChangeBeanieSkinNumber(idx);
            };
        }
        else if (type == "Bag") {
            textureList = Customize.Instance.bagSkins;
            spriteList = Customize.Instance.bagSkinIcons;
            onClick = (idx) => {
                Customize.Instance.EquipBag();
                Customize.Instance.EquipBagSkinNumber(idx);
            };
        }

        int spriteCount = spriteList != null ? spriteList.Count : 0;
        int texCount = textureList != null ? textureList.Count : 0;
        int matCount = materialList != null ? materialList.Count : 0;
        Debug.Log($"Data counts — Sprites: {spriteCount}, Textures: {texCount}, Materials: {matCount}");

        // 안전하게 버튼 개수 결정 (아이콘, 텍스처, 머티리얼 중 가장 많은 개수)
        int count = 0;
        if (spriteList != null && spriteList.Count > 0)
            count = spriteList.Count;
        if (textureList != null && textureList.Count > count)
            count = textureList.Count;
        if (materialList != null && materialList.Count > count)
            count = materialList.Count;

        if (count == 0)
        {
            Debug.LogWarning($"No items found for type '{type}' — nothing to display.");
            return;
        }

        int created = 0;
        for (int i = 0; i < count; i++)
        {
            var btnObj = Instantiate(skinButtonPrefab, skinListContent);
            var btn = btnObj.GetComponent<UnityEngine.UI.Button>();
            var img = btnObj.GetComponent<UnityEngine.UI.Image>();

            if (btn == null) Debug.LogWarning($"Instantiated skinButtonPrefab but Button component is missing on prefab at index {i}.");
            if (img == null) Debug.LogWarning($"Instantiated skinButtonPrefab but Image component is missing on prefab at index {i}.");

            // Ensure instantiated GameObject and components are active/enabled so UI shows up even if prefab had them disabled
            if (btnObj != null && !btnObj.activeSelf)
            {
                btnObj.SetActive(true);
                Debug.Log($"Activated instantiated button GameObject at index {i}.");
            }
            if (img != null && !img.enabled)
            {
                img.enabled = true;
                Debug.Log($"Enabled Image component on button {i}.");
            }
            if (btn != null)
            {
                if (!btn.enabled)
                {
                    btn.enabled = true;
                    Debug.Log($"Enabled Button component on button {i}.");
                }
                btn.interactable = true;
                if (btn.targetGraphic == null && img != null)
                {
                    btn.targetGraphic = img;
                    Debug.Log($"Assigned Image as Button.targetGraphic for button {i}.");
                }
            }

            string used = "none";

            // 아이콘 리스트 우선 적용
            if (spriteList != null && i < spriteList.Count && spriteList[i] != null)
            {
                img.sprite = spriteList[i];
                used = "sprite";
            }
            else if (textureList != null && i < textureList.Count && textureList[i] != null)
            {
                Texture2D tex2D = textureList[i] as Texture2D;
                if (tex2D != null)
                {
                    img.sprite = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), new Vector2(0.5f, 0.5f));
                    used = "texture";
                }
                else
                {
                    used = "texture (not Texture2D)";
                }
            }
            else if (materialList != null && i < materialList.Count && materialList[i] != null)
            {
                Texture2D tex2D = materialList[i].mainTexture as Texture2D;
                if (tex2D != null)
                {
                    img.sprite = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), new Vector2(0.5f, 0.5f));
                    used = "material.mainTexture";
                }
                else
                {
                    used = "material (no Texture2D)";
                }
            }

            int idx = i;
            if (onClick != null)
            {
                btn.onClick.AddListener(() => onClick(idx));
            }
            else
            {
                Debug.LogWarning($"No onClick assigned for type '{type}' at index {i}.");
            }

            Debug.Log($"Created button {i} for type '{type}' - used: {used}");
            created++;
        }

        Debug.Log($"ShowSkinList complete: created {created} buttons for type '{type}'.");
    }

    // 버튼 클릭 이벤트에 연결
    // 버튼 클릭 이벤트에 연결 (Player, Helicopter, Hook)
    public void OnPlayerButtonClicked() => ShowSkinList("Player");
    public void OnHelicopterButtonClicked() => ShowSkinList("Helicopter");
    public void OnHookButtonClicked() => ShowSkinList("Hook");

    // 비니, 옷, 가방 버튼 클릭 시 스킨 목록 표시
    public void OnBeanieButtonClicked() => ShowSkinList("Beanie");
    public void OnBagButtonClicked() => ShowSkinList("Bag");
    // 옷(플레이어 스킨) 버튼은 OnPlayerButtonClicked() 재사용 가능
    [Header("Buttons")]
    public Button cancelButton;
    public Button applyButton;

    [Header("Preview Controller")]
    public CustomizePreviewController previewController;
    public PreviewInputHandler previewInputHandler;

    [Header("Player Option Buttons")]
    [Tooltip("Panel that contains BEANIE / CLOTHES / BACKPACK buttons. Shown when Player preview is active.")]
    public GameObject playerOptionsPanel;

    // Snapshot of the panel entry state (used for Cancel)
    private int originalPlayerSkinIndex = -1;
    private int originalRopeSkinIndex = -1;
    private int originalHookSkinIndex = -1;
    private int originalHelicopterSkinIndex = -1;
    private bool originalBeanieEquipped = false;
    private int originalBeanieSkinIndex = -1;
    private bool originalBagEquipped = false;
    private int originalBagSkinIndex = -1;

    // Preview behavior (spawn, offsets, physics, and input) is handled by separate components:
    // - CustomizePreviewController
    // - PreviewInputHandler

    // They should be assigned via the inspector to `previewController` and `previewInputHandler`.

    private void Start()
    {
        InitializeButtons();
    }

    private void OnEnable()
    {
        CaptureOriginalState();

        // 기본 프리뷰를 Player로 설정 (버튼으로 변경 가능)
        if (previewController != null && previewController.previewPlayer != null)
        {
            previewController.SetActivePreview(previewController.previewPlayer);
            // ensure input handler references
            if (previewInputHandler != null)
            {
                previewInputHandler.previewController = previewController;
            }

            // subscribe to active preview changes so UI updates automatically
            previewController.OnActivePreviewChanged += UpdatePreviewUI;

            // show player options panel if assigned
            if (playerOptionsPanel != null) playerOptionsPanel.SetActive(true);
        }
    }

    private void InitializeButtons()
    {
        if (cancelButton != null) cancelButton.onClick.AddListener(OnCancelClicked);
        if (applyButton != null) applyButton.onClick.AddListener(OnApplyClicked);
    }

    private void OnDestroy()
    {
        if (cancelButton != null) cancelButton.onClick.RemoveListener(OnCancelClicked);
        if (applyButton != null) applyButton.onClick.RemoveListener(OnApplyClicked);

        // Restore any modified preview physics when this UI is destroyed
        if (previewController != null)
        {
            previewController.RestoreAllModifiedPreviews();
            previewController.OnActivePreviewChanged -= UpdatePreviewUI;
        }
    }

    private void CaptureOriginalState()
    {
        if (Customize.Instance == null || Player.Instance == null)
            return;

        originalPlayerSkinIndex = FindPlayerSkinIndex();
        originalRopeSkinIndex = FindRopeSkinIndex();
        originalHookSkinIndex = FindHookSkinIndex();
        originalHelicopterSkinIndex = FindHelicopterSkinIndex();

        originalBeanieEquipped = Player.Instance.IsBeanieEquipped();
        originalBeanieSkinIndex = FindBeanieSkinIndex();

        originalBagEquipped = Player.Instance.IsBagEquipped();
        originalBagSkinIndex = FindBagSkinIndex();
    }

    private int FindPlayerSkinIndex()
    {
        if (Customize.Instance == null || Player.Instance == null) return -1;
        var tex = Player.Instance.GetCurrentPlayerSkinTexture();
        if (tex == null) return -1;
        for (int i = 0; i < Customize.Instance.playerSkins.Count; i++)
        {
            if (Customize.Instance.playerSkins[i] == tex) return i;
        }
        return -1;
    }

    private int FindRopeSkinIndex()
    {
        if (Customize.Instance == null) return -1;
        var mat = Hook.currentSkin;
        if (mat == null) return -1;
        for (int i = 0; i < Customize.Instance.ropeSkins.Count; i++)
        {
            if (Customize.Instance.ropeSkins[i] == mat) return i;
        }
        return -1;
    }

    private int FindHookSkinIndex()
    {
        if (Customize.Instance == null) return -1;
        var mat = Hook.currentSkin;
        if (mat == null) return -1;
        for (int i = 0; i < Customize.Instance.hookSkins.Count; i++)
        {
            if (Customize.Instance.hookSkins[i] == mat) return i;
        }
        return -1;
    }

    private int FindHelicopterSkinIndex()
    {
        if (Customize.Instance == null) return -1;
        var tex = Helicopter.currentSkin;
        if (tex == null) return -1;
        for (int i = 0; i < Customize.Instance.helicopterSkins.Count; i++)
        {
            if (Customize.Instance.helicopterSkins[i] == tex) return i;
        }
        return -1;
    }

    private int FindBeanieSkinIndex()
    {
        if (Customize.Instance == null || Player.Instance == null) return -1;
        var tex = Player.Instance.GetCurrentBeanieSkinTexture();
        if (tex == null) return -1;
        for (int i = 0; i < Customize.Instance.beanieSkins.Count; i++)
        {
            if (Customize.Instance.beanieSkins[i] == tex) return i;
        }
        return -1;
    }

    private int FindBagSkinIndex()
    {
        if (Customize.Instance == null || Player.Instance == null) return -1;
        var tex = Player.Instance.GetCurrentBagSkinTexture();
        if (tex == null) return -1;
        for (int i = 0; i < Customize.Instance.bagSkins.Count; i++)
        {
            if (Customize.Instance.bagSkins[i] == tex) return i;
        }
        return -1;
    }

    private void ApplySelectionsFromIndices(int playerIndex, int ropeIndex, int hookIndex, int heliIndex, bool beanieEquipped, int beanieSkinIndex, bool bagEquipped, int bagSkinIndex)
    {
        if (Customize.Instance == null) return;

        if (playerIndex >= 0) Customize.Instance.EquipPlayerSkinNumber(playerIndex);
        if (ropeIndex >= 0) Customize.Instance.EquipRopeSkinNumber(ropeIndex);
        if (hookIndex >= 0) Customize.Instance.EquipHookSkinNumber(hookIndex);
        if (heliIndex >= 0) Customize.Instance.EquipHelicopterSkinNumber(heliIndex);

        if (beanieEquipped)
        {
            Customize.Instance.EquipBeanie();
            if (beanieSkinIndex >= 0) Customize.Instance.ChangeBeanieSkinNumber(beanieSkinIndex);
        }
        else
        {
            Customize.Instance.UnequipBeanie();
        }

        if (bagEquipped)
        {
            Customize.Instance.EquipBag();
            if (bagSkinIndex >= 0) Customize.Instance.EquipBagSkinNumber(bagSkinIndex);
        }
        else
        {
            Customize.Instance.UnequipBag();
        }
    }

    /// <summary>
    /// Cancel 버튼 클릭 핸들러
    /// - 현재 편집 중인 변경사항을 취소하고 패널이 열렸을 때 상태로 복구한 뒤 MainMenu로 전환
    /// </summary>
    public void OnCancelClicked()
    {
        ApplySelectionsFromIndices(originalPlayerSkinIndex, originalRopeSkinIndex, originalHookSkinIndex, originalHelicopterSkinIndex, originalBeanieEquipped, originalBeanieSkinIndex, originalBagEquipped, originalBagSkinIndex);
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Apply 버튼 클릭 핸들러
    /// - 현재 적용된 설정을 PlayerPrefs에 저장하고 MainMenu로 전환
    /// </summary>
    public void OnApplyClicked()
    {
        if (Customize.Instance == null || Player.Instance == null)
        {
            SceneManager.LoadScene("MainMenu");
            return;
        }

        int playerIndex = FindPlayerSkinIndex();
        int ropeIndex = FindRopeSkinIndex();
        int hookIndex = FindHookSkinIndex();
        int heliIndex = FindHelicopterSkinIndex();
        bool beanieEquipped = Player.Instance.IsBeanieEquipped();
        int beanieSkinIndex = FindBeanieSkinIndex();
        bool bagEquipped = Player.Instance.IsBagEquipped();
        int bagSkinIndex = FindBagSkinIndex();

        PlayerPrefs.SetInt("SelectedPlayerSkinIndex", playerIndex);
        PlayerPrefs.SetInt("SelectedRopeSkinIndex", ropeIndex);
        PlayerPrefs.SetInt("SelectedHookSkinIndex", hookIndex);
        PlayerPrefs.SetInt("SelectedHelicopterSkinIndex", heliIndex);
        PlayerPrefs.SetInt("SelectedBeanieEquipped", beanieEquipped ? 1 : 0);
        PlayerPrefs.SetInt("SelectedBeanieSkinIndex", beanieSkinIndex);
        PlayerPrefs.SetInt("SelectedBagEquipped", bagEquipped ? 1 : 0);
        PlayerPrefs.SetInt("SelectedBagSkinIndex", bagSkinIndex);
        PlayerPrefs.Save();

        SceneManager.LoadScene("MainMenu");
    }

    // Update preview-specific UI (e.g., show player option buttons only when player preview is active)
    private void UpdatePreviewUI(GameObject active)
    {
        bool showPlayerOptions = (active != null && previewController != null && active == previewController.previewPlayer);
        if (playerOptionsPanel != null) playerOptionsPanel.SetActive(showPlayerOptions);
    }

    // Preview input (drag-to-rotate) is handled by `PreviewInputHandler`.
    // No per-frame input handling is required in CustomizeUI after refactor.


    /// <summary>
    /// UI 표시/비표시
    /// </summary>
    public void Show(bool visible)
    {
        gameObject.SetActive(visible);
    }

    /// <summary>
    /// 헬리콥터 스킨 선택 (버튼에서 호출)
    /// </summary>
    public void SelectHelicopterSkin(int index)
    {
        if (Customize.Instance != null) Customize.Instance.EquipHelicopterSkinNumber(index);
    }


    /// <summary>
    /// Reset preview local rotation to identity
    /// </summary>
    public void ResetPreviewRotation()
    {
        if (previewController != null)
            previewController.ResetPreviewRotation();
    }
    /// <summary>
    /// 훅 스킨 선택 (버튼에서 호출)
    /// </summary>
    public void SelectHookSkin(int index)
    {
        if (Customize.Instance != null) Customize.Instance.EquipHookSkinNumber(index);
    }

    /// <summary>
    /// Player 미리보기 활성화
    /// </summary>
    public void ShowPlayerPreview()
    {
        if (previewController != null) previewController.SetActivePreview(previewController.previewPlayer);
        if (playerOptionsPanel != null) playerOptionsPanel.SetActive(true);
    }

    /// <summary>
    /// Helicopter 미리보기 활성화
    /// </summary>
    public void ShowHelicopterPreview()
    {
        if (previewController != null) previewController.SetActivePreview(previewController.previewHelicopter);
        if (playerOptionsPanel != null) playerOptionsPanel.SetActive(false);
    }

    /// <summary>
    /// Hook 미리보기 활성화
    /// </summary>
    public void ShowHookPreview()
    {
        if (previewController != null) previewController.SetActivePreview(previewController.previewHook);
        if (playerOptionsPanel != null) playerOptionsPanel.SetActive(false);
    }

    // Preview lifecycle implementation moved to `CustomizePreviewController`.
    // The per-preview methods (SetActivePreview, MakePreviewPhysicsStable, EnforceStableWhileActive,
    // RestorePhysicsState, RestoreAllModifiedPreviews, etc.) have been moved to the new controller to
    // keep `CustomizeUI` focused on UI responsibilities. Use `previewController` to manage previews.

    /// <summary>
    /// 버튼 활성화 설정
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        if (cancelButton != null) cancelButton.interactable = interactable;
        if (applyButton != null) applyButton.interactable = interactable;
    }
}
