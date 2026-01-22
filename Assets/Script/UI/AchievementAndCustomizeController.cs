using UnityEngine;

/// <summary>
/// AchievementAndCustomize 씬에서 Achievement와 Customize 패널을 관리하는 컨트롤러
/// </summary>
public class AchievementAndCustomizeController : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject achievementPanel;
    [SerializeField] private GameObject customizePanel;

    private void Start()
    {
        // PlayerPrefs에서 모드 읽기
        string currentMode = PlayerPrefs.GetString("AchievementCustomizeMode", "Achievement");
        
        // 모드에 따라 패널 표시
        if (currentMode == "Achievement")
        {
            ShowAchievementPanel();
        }
        else if (currentMode == "Customize")
        {
            ShowCustomizePanel();
        }
    }

    /// <summary>
    /// Achievement 패널 표시
    /// </summary>
    private void ShowAchievementPanel()
    {
        if (achievementPanel != null) achievementPanel.SetActive(true);
        if (customizePanel != null) customizePanel.SetActive(false);
    }

    /// <summary>
    /// Customize 패널 표시
    /// </summary>
    private void ShowCustomizePanel()
    {
        if (achievementPanel != null) achievementPanel.SetActive(false);
        if (customizePanel != null) customizePanel.SetActive(true);
    }
}
