using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class HeadLight : MonoBehaviour  // 자동차 헤드라이트 스크립트
{
    DayNightCycle dayNightCycle;   // isNight 을 가진 스크립트

    public float maxIntensity = 2.5f;      // 밤일 때 밝기
    float changeSpeed = 0.2f;        // 밝기 변화 속도

    private Light headLight;

    void Start()
    {
        // 같은 오브젝트에 있는 Light 자동으로 가져오기
        headLight = GetComponent<Light>();
        dayNightCycle = GameObject.FindGameObjectWithTag("Light").GetComponent<DayNightCycle>();
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
