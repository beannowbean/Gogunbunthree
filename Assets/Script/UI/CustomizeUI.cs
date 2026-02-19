using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

/// <summary>
/// Customize UI 컨트롤러
/// - 스킨 버튼 클릭 시 바로 설정이 저장
/// - Quit 버튼으로 메인 메뉴 돌아가기
/// </summary>
public class CustomizeUI : MonoBehaviour
{
    [Header("Skin List UI")]
    public Transform skinListContent; // 스크롤뷰 Content
    public GameObject skinButtonPrefab; // 버튼 프리팹 (Image 포함)

    // 프리뷰 타입별 스킨 리스트 동적 생성
    public void ShowSkinList(string type)
    {
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
                }
                continue;
            }
            Destroy(child.gameObject);
            removed++;
        }
        


        List<Sprite> spriteList = null;
        System.Action<int> onClick = null;

        if (type == "Player") {
            spriteList = Customize.Instance.GetUnlockedPlayerSkinIcons();
            onClick = (idx) => {
                Customize.Instance.EquipPlayerSkinNumber(idx);
                PlayerPrefs.SetInt("SelectedPlayerSkinIndex", idx);
                PlayerPrefs.Save();
            };
        }
        else if (type == "Helicopter") {
            spriteList = Customize.Instance.GetUnlockedHelicopterSkinIcons();
            onClick = (idx) => {
                Customize.Instance.EquipHelicopterSkinNumber(idx);
                PlayerPrefs.SetInt("SelectedHelicopterSkinIndex", idx);
                PlayerPrefs.Save();
            };
        }
        else if (type == "Hook") {
            spriteList = Customize.Instance.GetUnlockedHookSkinIcons();
            onClick = (idx) => {
                Customize.Instance.EquipHookSkinNumber(idx);
                PlayerPrefs.SetInt("SelectedHookSkinIndex", idx);
                PlayerPrefs.SetInt("SelectedRopeSkinIndex", idx);
                PlayerPrefs.Save();
            };
        }
        else if (type == "Beanie") {
            spriteList = Customize.Instance.GetUnlockedBeanieSkinIcons();
            onClick = (idx) => {
                Customize.Instance.EquipBeanie();
                Customize.Instance.ChangeBeanieSkinNumber(idx);
                PlayerPrefs.SetInt("SelectedBeanieEquipped", 1);
                PlayerPrefs.SetInt("SelectedBeanieSkinIndex", idx);
                PlayerPrefs.Save();
            };
        }
        else if (type == "Bag") {
            spriteList = Customize.Instance.GetUnlockedBagSkinIcons();
            onClick = (idx) => {
                Customize.Instance.EquipBag();
                Customize.Instance.EquipBagSkinNumber(idx);
                PlayerPrefs.SetInt("SelectedBagEquipped", 1);
                PlayerPrefs.SetInt("SelectedBagSkinIndex", idx);
                PlayerPrefs.Save();
            };
        }

        // 비니나 가방의 경우, "착용 안 함" 아이콘 버튼을 맨 앞에 추가
        if (type == "Beanie" && Customize.Instance.beanieUnequipIcon != null)
        {
            var btnObj = Instantiate(skinButtonPrefab, skinListContent);
            btnObj.name = "Beanie_UnequipButton";
            
            var btn = btnObj.GetComponentInChildren<UnityEngine.UI.Button>();
            var img = btn?.GetComponent<UnityEngine.UI.Image>();
            
            if (btnObj != null && !btnObj.activeSelf)
                btnObj.SetActive(true);
            if (img != null)
            {
                if (!img.enabled) img.enabled = true;
                img.sprite = Customize.Instance.beanieUnequipIcon;
            }
            if (btn != null)
            {
                if (!btn.enabled) btn.enabled = true;
                btn.interactable = true;
                if (btn.targetGraphic == null && img != null)
                    btn.targetGraphic = img;
                btn.onClick.AddListener(() => {
                    Customize.Instance.UnequipBeanie();
                    PlayerPrefs.SetInt("SelectedBeanieEquipped", 0);
                    PlayerPrefs.Save();
                });
            }
        }
        else if (type == "Bag" && Customize.Instance.bagUnequipIcon != null)
        {
            var btnObj = Instantiate(skinButtonPrefab, skinListContent);
            btnObj.name = "Bag_UnequipButton";
            
            var btn = btnObj.GetComponentInChildren<UnityEngine.UI.Button>();
            var img = btn?.GetComponent<UnityEngine.UI.Image>();
            
            if (btnObj != null && !btnObj.activeSelf)
                btnObj.SetActive(true);
            if (img != null)
            {
                if (!img.enabled) img.enabled = true;
                img.sprite = Customize.Instance.bagUnequipIcon;
            }
            if (btn != null)
            {
                if (!btn.enabled) btn.enabled = true;
                btn.interactable = true;
                if (btn.targetGraphic == null && img != null)
                    btn.targetGraphic = img;
                btn.onClick.AddListener(() => {
                    Customize.Instance.UnequipBag();
                    PlayerPrefs.SetInt("SelectedBagEquipped", 0);
                    PlayerPrefs.Save();
                });
            }
        }

        int count = spriteList != null ? spriteList.Count : 0;
        int created = 0;
        for (int i = 0; i < count; i++)
        {
            var btnObj = Instantiate(skinButtonPrefab, skinListContent);
            // 아이콘 파일명에서 숫자 추출
            string spriteName = spriteList[i]?.name ?? "";
            btnObj.name = $"{type}_SkinButton_{spriteName}";

            var btn = btnObj.GetComponentInChildren<UnityEngine.UI.Button>();
            var img = btn?.GetComponent<UnityEngine.UI.Image>();

            if (btnObj != null && !btnObj.activeSelf)
                btnObj.SetActive(true);
            if (img != null && !img.enabled)
                img.enabled = true;
            if (btn != null)
            {
                if (!btn.enabled) btn.enabled = true;
                btn.interactable = true;
                if (btn.targetGraphic == null && img != null)
                    btn.targetGraphic = img;
            }

            // 해금된 아이콘만 표시
            if (spriteList != null && i < spriteList.Count && spriteList[i] != null)
            {
                img.sprite = spriteList[i];
            }

            // 파일명에서 숫자 추출 (예: 12_abc → 12)
            int skinIndex = i;
            try {
                var underIdx = spriteName.IndexOf('_');
                if (underIdx > 0)
                {
                    string numPart = spriteName.Substring(0, underIdx);
                    if (int.TryParse(numPart, out int parsed))
                        skinIndex = parsed;
                }
            } catch { /* fallback: i */ }

            if (onClick != null)
            {
                btn.onClick.AddListener(() => onClick(skinIndex));
            }
            created++;
        }
    }

    // 버튼 클릭 이벤트에 연결
    // 버튼 클릭 이벤트에 연결 (Player, Helicopter, Hook)
    public void OnPlayerButtonClicked()
    {
        // 프리뷰 오브젝트 명시적 활성화(물리 안정화 보장)
        if (previewController != null) previewController.SetActivePreview(previewController.previewPlayer);
        if (playerOptionsPanel != null) playerOptionsPanel.SetActive(true);

        // 비니, 가방, 옷 모두 이전에 적용됐던 스킨으로 프리뷰 업데이트
        int playerIndex = PlayerPrefs.GetInt("SelectedPlayerSkinIndex", 0);
        var playerSkins = Customize.Instance.playerSkins;

        bool beanieEquipped = PlayerPrefs.GetInt("SelectedBeanieEquipped", 0) == 1;
        int beanieIndex = PlayerPrefs.GetInt("SelectedBeanieSkinIndex", 0);

        bool bagEquipped = PlayerPrefs.GetInt("SelectedBagEquipped", 0) == 1;
        int bagIndex = PlayerPrefs.GetInt("SelectedBagSkinIndex", 0);

        Customize.Instance.EquipPlayerSkinNumber(playerIndex);

        if (beanieEquipped)
        {
            Customize.Instance.EquipBeanie();
            Customize.Instance.ChangeBeanieSkinNumber(beanieIndex);
        }
        else
        {
            Customize.Instance.UnequipBeanie();
        }

        if (bagEquipped)
        {
            Customize.Instance.EquipBag();
            Customize.Instance.EquipBagSkinNumber(bagIndex);
        }
        else
        {
            Customize.Instance.UnequipBag();
        }

        ShowSkinList("Player");
    }
    public void OnHelicopterButtonClicked()
    {
        int helicopterIndex = PlayerPrefs.GetInt("SelectedHelicopterSkinIndex", 0);
        var helicopterSkins = Customize.Instance.helicopterSkins;
        Customize.Instance.EquipHelicopterSkinNumber(helicopterIndex);

        ShowSkinList("Helicopter");
    }
    public void OnHookButtonClicked()
    {
        int hookIndex = PlayerPrefs.GetInt("SelectedHookSkinIndex", 0);
        var hookSkins = Customize.Instance.hookSkins;
        Customize.Instance.EquipHookSkinNumber(hookIndex);
        
        ShowSkinList("Hook");
    }

    // 비니, 옷, 가방 버튼 클릭 시 스킨 목록 표시
    public void OnBeanieButtonClicked() => ShowSkinList("Beanie");
    public void OnBagButtonClicked() => ShowSkinList("Bag");
    // 옷(플레이어 스킨) 버튼은 OnPlayerButtonClicked() 재사용 가능
    [Header("Buttons")]
    public Button quitButton;

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

    // Tracks which buttons were auto-wired to avoid duplicate listeners
    private HashSet<int> autoWiredButtonIds = new HashSet<int>();

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
        // Player 버튼 클릭과 동일하게 동작 (프리뷰, 스킨, UI 모두 적용)
        OnPlayerButtonClicked();
    }
    

    private void InitializeButtons()
    {
        if (quitButton != null) quitButton.onClick.AddListener(OnCancelClicked);

        // Auto-wire common tab buttons (Player, Helicopter, Hook, Beanie, Bag) if present under this UI hierarchy.
        var childButtons = GetComponentsInChildren<Button>(true);
        foreach (var b in childButtons)
        {
            if (b == null) continue;
            int id = b.GetInstanceID();
            string n = b.gameObject.name.ToLowerInvariant();

            if (autoWiredButtonIds.Contains(id)) continue; // already wired by previous run

            if (n.Contains("helicopter"))
            {
                b.onClick.AddListener(() => OnHelicopterButtonClicked());
                autoWiredButtonIds.Add(id);
                continue;
            }
            if (n.Contains("hook"))
            {
                b.onClick.AddListener(() => OnHookButtonClicked());
                autoWiredButtonIds.Add(id);
                continue;
            }
            if (n.Contains("beanie"))
            {
                b.onClick.AddListener(() => OnBeanieButtonClicked());
                autoWiredButtonIds.Add(id);
                continue;
            }
            if (n.Contains("bag") || n.Contains("backpack") || n.Contains("backpack"))
            {
                b.onClick.AddListener(() => OnBagButtonClicked());
                autoWiredButtonIds.Add(id);
                continue;
            }
            if (n.Contains("player") || n.Contains("cloth") || n.Contains("clothes"))
            {
                b.onClick.AddListener(() => OnPlayerButtonClicked());
                autoWiredButtonIds.Add(id);
                continue;
            }
        }
    }

    private void OnDestroy()
    {
        if (quitButton != null) quitButton.onClick.RemoveListener(OnCancelClicked);

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
        Material currentRopeMat = Player.currentRopeMaterial;
        if(currentRopeMat == null) return -1;
        for (int i = 0; i < Customize.Instance.ropeSkins.Count; i++)
        {
            if (Customize.Instance.ropeSkins[i] == currentRopeMat) return i;
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
    /// - 현재 적용된 상태 그대로 MainMenu로 이동
    /// </summary>
    public void OnCancelClicked()
    {
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

    /// <summary>
    /// 버튼 활성화 설정
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        if (quitButton != null) quitButton.interactable = interactable;
    }
}
