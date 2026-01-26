using UnityEngine;

/// <summary>
/// 차 아래로 움직이는 스크립트
/// </summary>
public class CarMove : MonoBehaviour
{
    public float changeTime = 3.0f; // 속도 변화 시간

    // 내부 변수
    TileGenerate tileGenerate;  // 타일 생성 스크립트 참조
    Player player;  // 플레이어 참조
    bool freezeStreetLight = false; // 가로등 고정 여부
    float currentCarSpeed;  // 현재 차 속도
    
    void Start()
    {
        // 참조 설정
        tileGenerate = GameObject.FindGameObjectWithTag("TileGenerator").GetComponent<TileGenerate>();   // carSpeed 참조용

        // 초기 차 속도
        currentCarSpeed = tileGenerate.carSpeed;
    }

    void Update()
    {
        // 차 속도 부드럽게 변경 및 이동
        currentCarSpeed = Mathf.Lerp(currentCarSpeed, tileGenerate.carSpeed, Time.deltaTime / changeTime);
        transform.position += new Vector3(0, 0, -currentCarSpeed * Time.deltaTime);

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.GetComponent<Player>();
            }
            return;
        }

        // 게임오버 시 가로등 고정
        if (player.isGameOver == true && freezeStreetLight == false)
        {
            // 가로등 찾아 저장 후 부모 해제하여 고정
            GameObject[] streetLights = GameObject.FindGameObjectsWithTag("StreetLight");
            foreach(GameObject streetLight in streetLights)
            {
                Vector3 curPos = streetLight.transform.position;
                streetLight.transform.SetParent(null);
                streetLight.transform.position = curPos;
            }
            freezeStreetLight = true;   // 고정 확인 및 중복 실행 방지
        }
    }
}
