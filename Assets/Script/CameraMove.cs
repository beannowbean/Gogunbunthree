using UnityEngine;

/// <summary>
/// 카메라 이동 스크립트
/// </summary>
public class CameraMove : MonoBehaviour
{
    public TileGenerate tileGenerate;   // 타일 생성기 참조
    public GameObject[] designer;   // 디자이너
    public GameObject varDesigner;  // MovingCar 디자이너
    public Transform target;    // 따라갈 플레이어
    public Vector3 offset = new Vector3(0, 3, -5);   // 기본 오프셋
    public float followSpeed = 5f;    // 위치 따라가기 속도 

    // [추가] 헬리콥터 시점 조정을 위한 변수
    private Vector3 defaultOffset; // 게임 시작 시 설정된 오프셋 저장용
    private Player playerScript;   // 플레이어 상태(헬기 탑승 여부) 확인용

    // 내부 변수
    Vector3[] designerOffsets;  // 디자이너 초기 오프셋
    Vector3 varDesignerOffset;  // MovingCar 디자이너 초기 오프셋

    void Start()
    {
        // [추가] 시작할 때 설정된 오프셋을 기본값으로 저장
        defaultOffset = offset;

        // 디자이너들의 초기 상대 오프셋 저장
        designerOffsets = new Vector3[designer.Length];
        for (int i = 0; i < designer.Length; i++)
        {
            designerOffsets[i] = designer[i].transform.position - transform.position;
        }
        varDesignerOffset = varDesigner.transform.position - transform.position;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                // [추가] 플레이어 스크립트 캐싱 (매번 GetComponent 하지 않도록)
                playerScript = player.GetComponent<Player>();
            }
            else
            {
                return;
            }
        }

        // [추가] 헬리콥터 탑승 여부에 따라 오프셋 조정
        // 헬리콥터 탑승 시: y -0.5, z -0.5 적용
        Vector3 targetOffset = defaultOffset;

        if (playerScript != null && playerScript.isHelicopter)
        {
            // 헬리콥터 탈 때의 목표 오프셋 (기본값에서 y-0.5, z-0.5)
            targetOffset = defaultOffset + new Vector3(0, -0.5f, -0.5f);
        }

        // 현재 오프셋을 목표 오프셋으로 부드럽게 변경 (Lerp 사용)
        offset = Vector3.Lerp(offset, targetOffset, Time.deltaTime * 2f);

        // 목표 위치 = 플레이어 위치 + (조정된) 오프셋
        Vector3 targetPos = target.position + offset;
        float smoothX = Mathf.Lerp(transform.position.x, targetPos.x, Time.deltaTime * followSpeed);
        float smoothY = Mathf.Lerp(transform.position.y, targetPos.y, Time.deltaTime * followSpeed);

        // 카메라 위치 부드럽게 이동 (z축은 고정 -> targetPos.z 사용)
        transform.position = new Vector3(smoothX, smoothY, targetPos.z);

        // 디자이너 오브젝트 이동
        for (int i = 0; i < designer.Length; i++)
        {
            Vector3 desiredPos = transform.position + designerOffsets[i];
            desiredPos.y = 0;

            designer[i].transform.position = desiredPos;
        }
        Vector3 varDesiredPos = transform.position + varDesignerOffset;

        // 속도에 따라 MovingCar 디자이너 z좌표 조정
        switch (tileGenerate.carSpeed)
        {
            case 20:
                break;
            case 25:
                varDesiredPos.z += 2.5f;
                break;
            case 30:
                varDesiredPos.z += 5.0f;
                break;
            case 35:
                varDesiredPos.z += 7.5f;
                break;
            case 40:
                varDesiredPos.z += 10.0f;
                break;
        }
        varDesiredPos.y = 0;
        varDesigner.transform.position = varDesiredPos;
    }
}