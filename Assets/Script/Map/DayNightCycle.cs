using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour  // 낮과 밤 주기 스크립트
{
    public float dayDuration = 10f;
    public float sunsetDuration = 5f;
    public float nightDuration = 10f;
    public float sunriseDuration = 5f;

    public Light directionalLight;

    private Material skyboxMaterial;

    public bool isNight = false; // 밤인지 확인용

    void Start()
    {
        // Skybox 머테리얼 가져오기
        skyboxMaterial = RenderSettings.skybox;

        StartCoroutine(DayNight());
    }

    IEnumerator DayNight()
    {
        while (true)
        {
            // 낮
            isNight = false;
            directionalLight.intensity = 1f;
            skyboxMaterial.SetFloat("_Exposure", 1f);
            skyboxMaterial.SetFloat("_Blend", 0f);
            yield return RotateLightOverTime(50f, 50f, dayDuration);

            // 낮->밤
            isNight = false;
            yield return RotateLightOverTimeWithIntensityAndSkybox(50f, 230f, sunsetDuration, 1f, 0f, 1f, 0.8f, 0f, 1f);

            // 밤
            isNight = true;
            directionalLight.intensity = 0f;
            skyboxMaterial.SetFloat("_Exposure", 0.8f);
            skyboxMaterial.SetFloat("_Blend", 1f);
            yield return RotateLightOverTime(230f, 230f, nightDuration);

            // 밤->낮
            isNight = false;
            yield return RotateLightOverTimeWithIntensityAndSkybox(230f, 410f, sunriseDuration, 0f, 1f, 0.8f, 1f, 1f, 0f);
        }
    }

    // 빛의 각도를 시간에 따라 변화 (낮 또는 밤)
    IEnumerator RotateLightOverTime(float startAngle, float endAngle, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float angle = Mathf.Lerp(startAngle, endAngle, elapsed / duration);
            directionalLight.transform.rotation = Quaternion.Euler(angle, 0f, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        directionalLight.transform.rotation = Quaternion.Euler(endAngle, 0f, 0f);
    }

    // 빛의 각도를 시간에 따라 변화 (스카이박스 / 강도 포함)
    IEnumerator RotateLightOverTimeWithIntensityAndSkybox(float startAngle, float endAngle, float duration,
        float startIntensity, float endIntensity, float startExposure, float endExposure, float startBlend, float endBlend)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float angle = Mathf.Lerp(startAngle, endAngle, t);
            float intensity = Mathf.Lerp(startIntensity, endIntensity, t);
            float exposure = Mathf.Lerp(startExposure, endExposure, t);
            float blend = Mathf.Lerp(startBlend, endBlend, t);

            directionalLight.transform.rotation = Quaternion.Euler(angle, 0f, 0f);
            directionalLight.intensity = intensity;
            skyboxMaterial.SetFloat("_Exposure", exposure);
            skyboxMaterial.SetFloat("_Blend", blend);

            elapsed += Time.deltaTime;
            yield return null;
        }
        directionalLight.transform.rotation = Quaternion.Euler(endAngle, 0f, 0f);
        directionalLight.intensity = endIntensity;
        skyboxMaterial.SetFloat("_Exposure", endExposure);
        skyboxMaterial.SetFloat("_Blend", endBlend);
    }
}
