using UnityEngine;

/// <summary>
/// 자동차 헤드라이트 스크립트
/// </summary>
public class HeadLight : MonoBehaviour
{
    // 내부 변수
    Light headLight;             // 헤드라이트 라이트 컴포넌트
    DayNightCycle dayNightCycle;   // 낮밤 전환 스크립트 참조
    float maxIntensity = 3f;      // 밤일 때 라이트 밝기
    float changeSpeed = 0.1f;        // 밝기 변화 속도 (인스펙터가 복잡해 여기서 변경 / 낮을수록 천천히 켜짐)

    void Start()
    {
        // 참조 설정
        headLight = GetComponent<Light>();
        dayNightCycle = GameObject.FindGameObjectWithTag("Light").GetComponent<DayNightCycle>(); // isNight 참조용

        // 시작 시 낮이면 라이트 끔
        if(dayNightCycle.isNight == false) headLight.intensity = 0f;
    }

    void Update()
    {
        // isNight에 따라 목표 밝기 결정
        float targetIntensity = dayNightCycle.isNight ? maxIntensity : 0f;

        // 점점 밝기 변화
        headLight.intensity = Mathf.Lerp(
            headLight.intensity,
            targetIntensity,
            Time.deltaTime * changeSpeed
        );
    }
}
