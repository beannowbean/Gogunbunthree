using UnityEngine;

/// <summary>
/// 아이템 맵 가로등 위치 조정용 스크립트
/// </summary>
public class StreetLightPosition : MonoBehaviour
{
    public bool isBus = false; // 버스인지 여부

    // 내부 변수
    TileGenerate tileGenerate; // 타일 생성 스크립트 참조
    Vector3 initialPos;  // 초기 위치 저장 변수

    void Awake()
    {
        tileGenerate = GameObject.FindGameObjectWithTag("TileGenerator").GetComponent<TileGenerate>();
        initialPos = transform.localPosition;
    }

    void OnEnable()
    {
        transform.localPosition = initialPos;
        if(isBus == true)
        {
            if(tileGenerate.carSpeed >= 40)
            {
                transform.localPosition = new Vector3(initialPos.x, initialPos.y, initialPos.z - 17.1f);
            }
            else if(tileGenerate.carSpeed >= 35)
            {
                transform.localPosition = new Vector3(initialPos.x, initialPos.y, initialPos.z - 14.6f);
            }
            else if(tileGenerate.carSpeed >= 30)
            {
                transform.localPosition = new Vector3(initialPos.x, initialPos.y, initialPos.z - 13.1f);
            }
            else if(tileGenerate.carSpeed >= 25)
            {
                transform.localPosition = new Vector3(initialPos.x, initialPos.y, initialPos.z - 11.6f);
            }
            else if(tileGenerate.carSpeed >= 20)
            {
                transform.localPosition = new Vector3(initialPos.x, initialPos.y, initialPos.z - 10.1f);
            }
        }
        else if (isBus == false)
        {
            if(tileGenerate.carSpeed >= 40)
            {
                transform.localPosition = new Vector3(initialPos.x, initialPos.y, initialPos.z - 13.85f);
            }
            else if(tileGenerate.carSpeed >= 35)
            {
                transform.localPosition = new Vector3(initialPos.x, initialPos.y, initialPos.z - 11.45f);
            }
            else if(tileGenerate.carSpeed >= 30)
            {
                transform.localPosition = new Vector3(initialPos.x, initialPos.y, initialPos.z - 10.05f);
            }
            else if(tileGenerate.carSpeed >= 25)
            {
                transform.localPosition = new Vector3(initialPos.x, initialPos.y, initialPos.z - 8.65f);
            }
            else if(tileGenerate.carSpeed >= 20)
            {
                transform.localPosition = new Vector3(initialPos.x, initialPos.y, initialPos.z - 7.25f);
            }
        }
    }
}
