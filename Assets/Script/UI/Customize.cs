/*
    이 함수들을 복사해서 붙여넣은 후.
    인스펙터에 스킨들을 대입시키기.
    커스터마이즈 탭에서는 각 번호를 개별 메소드로 호출한.
 */


using UnityEngine;


// [추가] 스킨들 리스트로 저장하기 위해
using System.Collections.Generic; 

public class Customize : MonoBehaviour
{
    public static Customize Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    [Header("PlayerSkin")]
    // 실제 적용되는 텍스처 리스트
    public List<Texture> playerSkins = new List<Texture>();
    // UI에 표시할 아이콘(Sprite) 리스트 (인덱스 매칭)
    public List<Sprite> playerSkinIcons = new List<Sprite>();

    // [각 스킨마다 개별 호출] 이 함수로 번호 호출해서 착용시키도록
    public void EquipPlayerSkinNumber(int index)
    {
        if (index >= 0 && index < playerSkins.Count)
        {
            Player.Instance.ChangeSkinTexture(playerSkins[index]);
        }
    }


    /*
     */


    [Header("PlayerRope")]
    public List<Material> ropeSkins = new List<Material>();
    public List<Sprite> ropeSkinIcons = new List<Sprite>();

    // [각 로프 머터리얼마다 개별 호출] 이 함수로 번호 호출해서 색깔 바꾸도록
    public void EquipRopeSkinNumber(int index)
    {
        if (index >= 0 && index < ropeSkins.Count)
        {
            Player.Instance.ChangeRopeMaterial(ropeSkins[index]);
        }
    }


    /*
     */


    [Header("PlayerHook")]
    public List<Material> hookSkins = new List<Material>();
    public List<Sprite> hookSkinIcons = new List<Sprite>();

    // [각 로프 머터리얼마다 개별 호출] 이 함수로 번호 호출해서 색깔 바꾸도록
    public void EquipHookSkinNumber(int index)
    {
        if (index >= 0 && index < hookSkins.Count)
        {
            Hook.currentSkin = hookSkins[index];
            // 즉시 씬의 Hook 오브젝트에 적용
            Hook.ApplyCurrentSkinToAll();
        }
    }


    /*
     */


    [Header("Helicopter")]
    public List<Texture> helicopterSkins = new List<Texture>();
    public List<Sprite> helicopterSkinIcons = new List<Sprite>();

    public void EquipHelicopterSkinNumber(int index)
    {
        if (index >= 0 && index < helicopterSkins.Count)
        {
            Helicopter.currentSkin = helicopterSkins[index];
            // 즉시 씬의 Helicopter 오브젝트에 적용
            Helicopter.ApplyCurrentSkinToAll();
        }
    }

    /*
     */


    [Header("beanie")]
    public GameObject beaniePrefab; 
    public List<Texture> beanieSkins = new List<Texture>();
    public List<Sprite> beanieSkinIcons = new List<Sprite>();

    public void EquipBeanie()
    {
        if (Player.Instance != null && beaniePrefab != null)
        {
            Player.Instance.EquipBeanie(beaniePrefab);
        }
    }

    public void UnequipBeanie()
    {
        if (Player.Instance != null)
        {
            Player.Instance.UnequipBeanie();
        }
    }

    public void ChangeBeanieSkinNumber(int index)
    {
        if (index >= 0 && index < beanieSkins.Count)
        {
            Player.Instance.ChangeBeanieSkin(beanieSkins[index]);
        }
    }

    /*
     */


    [Header("bag")]
    public GameObject bagPrefab;
    public List<Texture> bagSkins = new List<Texture>();
    public List<Sprite> bagSkinIcons = new List<Sprite>();

    public void EquipBag()
    {
        if (Player.Instance != null && bagPrefab != null)
        {
            Player.Instance.EquipBag(bagPrefab);
        }
    }

    public void UnequipBag()
    {
        if (Player.Instance != null)
        {
            Player.Instance.UnequipBag();
        }
    }

    public void EquipBagSkinNumber(int index)
    {
        if (index >= 0 && index < bagSkins.Count)
        {
            Player.Instance.ChangeBagSkin(bagSkins[index]);
        }
    }

    // 인덱스 기반으로 스킨을 해금 표시 (PlayerPrefs에 저장)
    public void UnlockPlayerSkinByIndex(int index)
    {
        if (index >= 0 && index < playerSkins.Count)
        {
            PlayerPrefs.SetInt($"PlayerSkin_{index}_Unlocked", 1);
            PlayerPrefs.Save();
            Debug.Log($"[Customize] Player skin index {index} unlocked");
        }
    }

    public void UnlockRopeSkinByIndex(int index)
    {
        if (index >= 0 && index < ropeSkins.Count)
        {
            PlayerPrefs.SetInt($"RopeSkin_{index}_Unlocked", 1);
            PlayerPrefs.Save();
            Debug.Log($"[Customize] Rope skin index {index} unlocked");
        }
    }

    public bool IsPlayerSkinUnlocked(int index)
    {
        return PlayerPrefs.GetInt($"PlayerSkin_{index}_Unlocked", 0) == 1;
    }

    public bool IsRopeSkinUnlocked(int index)
    {
        return PlayerPrefs.GetInt($"RopeSkin_{index}_Unlocked", 0) == 1;
    }
} 