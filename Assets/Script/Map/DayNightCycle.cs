using System.Collections;
using UnityEngine;

/// <summary>
/// 낮밤 변환 스크립트 (난이도 변경 확인 용)
/// </summary>
public class DayNightCycle : MonoBehaviour
{
    public float dayToNightTime = 15f;  // 낮->밤 변환 시간
    public float nightToDayTime = 15f;  // 밤->낮 변환 시간
    public Light directionalLight;  // 게임 라이트 참조
    public bool isNight = false;    // 밤인지 확인

    // 내부 변수
    Material skyboxMaterial;    // 스카이박스 머테리얼 참조

    void Start()
    {
        // 참조 설정
        skyboxMaterial = RenderSettings.skybox;

        // 시작 시 낮으로 설정
        StartDay();
    }

    // 낮으로 강제 설정
    private void StartDay()
    {
        // 모든 코루틴 중지 (변환 중복 방지)
        StopAllCoroutines();

        // 낮 설정
        isNight = false;

        // 라이트 및 스카이박스 초기값 설정
        directionalLight.intensity = 1f;
        directionalLight.transform.rotation = Quaternion.Euler(50f, 0f, 0f);

        skyboxMaterial.SetFloat("_Exposure", 1f);
        skyboxMaterial.SetFloat("_Blend", 0f);
    }

    // 낮에서 밤으로 변경
    public void DayToNight()
    {
        // 모든 코루틴 중지 (변환 중복 방지)
        StopAllCoroutines();

        // 낮에서 밤으로 변환 코루틴 시작
        StartCoroutine(TimeChange(50f, 230f, dayToNightTime, 1f, 0f, 1f, 0.8f, 0f, 1f));

        // 밤 설정
        isNight = true;
    }

    // 밤에서 낮으로 변경
    public void NightToDay()
    {
        // 모든 코루틴 중지 (변환 중복 방지)
        StopAllCoroutines();

        // 밤에서 낮으로 변환 코루틴 시작
        StartCoroutine(TimeChange(230f, 410f, nightToDayTime, 0f, 1f, 0.8f, 1f, 1f, 0f));

        // 낮 설정
        isNight = false;
    }

    // 시간에 따른 배경 변경 (light, skybox, shader blend)
    IEnumerator TimeChange(float startAngle, float endAngle, float duration,
        float startIntensity, float endIntensity, float startExposure, float endExposure, float startBlend, float endBlend)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            // 보간 계산
            float t = elapsed / duration;
            float angle = Mathf.Lerp(startAngle, endAngle, t);
            float intensity = Mathf.Lerp(startIntensity, endIntensity, t);
            float exposure = Mathf.Lerp(startExposure, endExposure, t);
            float blend = Mathf.Lerp(startBlend, endBlend, t);

            // 값 적용  
            directionalLight.transform.rotation = Quaternion.Euler(angle, 0f, 0f);
            directionalLight.intensity = intensity;

            skyboxMaterial.SetFloat("_Exposure", exposure);
            skyboxMaterial.SetFloat("_Blend", blend);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 최종 값 적용 (정확한 마무리용)
        directionalLight.transform.rotation = Quaternion.Euler(endAngle, 0f, 0f);
        directionalLight.intensity = endIntensity;
        
        skyboxMaterial.SetFloat("_Exposure", endExposure);
        skyboxMaterial.SetFloat("_Blend", endBlend);
    }
}
