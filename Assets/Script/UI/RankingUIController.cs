using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 랭킹 UI 컨트롤러
/// </summary>
public class RankingUIController : MonoBehaviour
{
    public Transform contentArea;        // Content 오브젝트 (랭킹 아이템들이 들어갈 곳)
    public GameObject rankItemPrefab;    // 랭킹 한 줄을 표시할 프리팹
    public GameObject loadingIndicator;   // 로딩 인디케이터 오브젝트

    [Header("Rank Icons")]
    public Sprite rank1Image;          // 1위 아이콘
    public Sprite rank2Image;          // 2위 아이콘
    public Sprite rank3Image;          // 3위 아이콘

    void Start()
    {
        loadingIndicator.SetActive(false);
        // 기본적으로 상위 랭킹 로드
        OnClickBestRank();
    }

    // 상위 랭킹 로드
    public void OnClickBestRank()
    {
        ClearList();
        loadingIndicator.SetActive(true);

        RankManager.Instance.GetTopRanking((rankDataArray) =>
        {
            loadingIndicator.SetActive(false);
            DisplayRanking(rankDataArray);
        });
    }

    // 내 주변 랭킹 로드
    public void OnClickMyRank()
    {
        ClearList();
        loadingIndicator.SetActive(true);

        RankManager.Instance.GetMyAroundRanking((rankDataArray) =>
        {
            loadingIndicator.SetActive(false);
            DisplayRanking(rankDataArray);
            StartCoroutine(SetScrollPosition());
        });
    }

    // 메인 씬으로 이동
    public void OnClickQuit()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // 서버 데이터를 UI 리스트로 생성
    private void DisplayRanking(RankData[] dataArray)
    {
        if (this == null) return; // 오브젝트가 파괴된 경우 방지
        foreach (var data in dataArray)
        {
            if(rankItemPrefab == null || contentArea == null) break;

            // 리스트 아이템 생성
            GameObject newItem = Instantiate(rankItemPrefab, contentArea);
            
            // 프리팹 내부의 Rank, Name, Score, Image 오브젝트 이름으로 찾아 할당
            TextMeshProUGUI rankTxt = newItem.transform.Find("Ranking/Rank").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI nameTxt = newItem.transform.Find("Name").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI scoreTxt = newItem.transform.Find("Score").GetComponent<TextMeshProUGUI>();
            Image rankImage = newItem.transform.Find("Ranking/Image").GetComponent<Image>();
            Outline outline = newItem.GetComponent<Outline>();

            // 데이터 적용
            rankTxt.text = data.rank.ToString();
            nameTxt.text = data.name;
            scoreTxt.text = data.score.ToString("N0");

            // 랭크 아이콘 설정
            if (data.rank == 1)
            {
                rankImage.sprite = rank1Image;
                rankImage.gameObject.SetActive(true);
                rankTxt.gameObject.SetActive(false);
            }
            else if (data.rank == 2)
            {
                rankImage.sprite = rank2Image;
                rankImage.gameObject.SetActive(true);
                rankTxt.gameObject.SetActive(false);
            }
            else if (data.rank == 3)
            {
                rankImage.sprite = rank3Image;
                rankImage.gameObject.SetActive(true);
                rankTxt.gameObject.SetActive(false);
            }
            else
            {
                rankImage.gameObject.SetActive(false);
                rankTxt.gameObject.SetActive(true);
            }

            // 내 등수 테두리 강조
            if (data.name == RankManager.Instance.currentNickname)
            {
                outline.enabled = true;
            }
            else
            {
                outline.enabled = false;
            }
        }
    }

    // 기존 리스트 삭제
    private void ClearList()
    {
        foreach (Transform child in contentArea)
        {
            Destroy(child.gameObject);
        }
    }

    // 내 랭킹 위치로 스크롤 이동
    IEnumerator SetScrollPosition()
    {
        yield return new WaitForEndOfFrame();
        ScrollRect scrollRect = GetComponentInChildren<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0.5f;
        }
    }
}