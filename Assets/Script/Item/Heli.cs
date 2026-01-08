using UnityEngine;

public class Heli : MonoBehaviour
{
    public float rotateSpeed = 300.0f;

    // 헬리콥터 생성하기 위한 선언
    public GameObject helicopterPrefab;

    void Start()
    {

    }

    void Update()
    {
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (helicopterPrefab != null)
            {
                // 헬리콥터 생성
                Instantiate(helicopterPrefab);
            }

            SFXManager.Instance.Play("ItemGet"); 

            // 아이템 삭제
            Destroy(gameObject);
        }
    }
}