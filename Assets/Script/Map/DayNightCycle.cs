using UnityEngine;
using System.Collections;

public class DayNightCycle : MonoBehaviour
{
    public float dayDuration = 10f;
    public float sunsetDuration = 5f;
    public float nightDuration = 10f;
    public float sunriseDuration = 5f;

    public Light directionalLight;
    public float dayIntensity = 1f;    // 낮 Light 강도
    public float nightIntensity = 0f;  // 밤 Light 강도

    public float daySkyboxExposure = 1f;   // 낮 Skybox 노출
    public float nightSkyboxExposure = 0f; // 밤 Skybox 노출

    private Material skyboxMaterial;

    public bool isNight = false; // 밤인지 여부 확인용

    void Start()
    {
        // Skybox 재질 가져오기
        skyboxMaterial = RenderSettings.skybox;

        StartCoroutine(DayNightRoutine());
    }

    IEnumerator DayNightRoutine()
    {
        while (true)
        {
            // 1. 낮 (고정 밝기)
            isNight = false;
            directionalLight.intensity = dayIntensity;
            skyboxMaterial.SetFloat("_Exposure", daySkyboxExposure);
            yield return RotateLightOverTime(50f, 50f, dayDuration);

            // 2. 낮->밤 (회전과 함께 밝기 감소)
            isNight = false;
            yield return RotateLightOverTimeWithIntensityAndSkybox(50f, 230f, sunsetDuration, dayIntensity, nightIntensity, daySkyboxExposure, nightSkyboxExposure);

            // 3. 밤 (고정 어둡게)
            isNight = true;
            directionalLight.intensity = nightIntensity;
            skyboxMaterial.SetFloat("_Exposure", nightSkyboxExposure);
            yield return RotateLightOverTime(230f, 230f, nightDuration);

            // 4. 밤->낮 (회전과 함께 밝기 증가)
            isNight = false;
            yield return RotateLightOverTimeWithIntensityAndSkybox(230f, 410f, sunriseDuration, nightIntensity, dayIntensity, nightSkyboxExposure, daySkyboxExposure);
        }
    }

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

    IEnumerator RotateLightOverTimeWithIntensityAndSkybox(float startAngle, float endAngle, float duration,
        float startIntensity, float endIntensity, float startExposure, float endExposure)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float angle = Mathf.Lerp(startAngle, endAngle, t);
            float intensity = Mathf.Lerp(startIntensity, endIntensity, t);
            float exposure = Mathf.Lerp(startExposure, endExposure, t);

            directionalLight.transform.rotation = Quaternion.Euler(angle, 0f, 0f);
            directionalLight.intensity = intensity;
            skyboxMaterial.SetFloat("_Exposure", exposure);

            elapsed += Time.deltaTime;
            yield return null;
        }

        directionalLight.transform.rotation = Quaternion.Euler(endAngle, 0f, 0f);
        directionalLight.intensity = endIntensity;
        skyboxMaterial.SetFloat("_Exposure", endExposure);
    }
}
