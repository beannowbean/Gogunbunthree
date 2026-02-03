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
    // 해금된 플레이어 스킨 아이콘 리스트 반환
    public List<Sprite> GetUnlockedPlayerSkinIcons()
    {
        List<Sprite> unlocked = new List<Sprite>();
        for (int i = 0; i < playerSkinIcons.Count; i++)
        {
            if (IsPlayerSkinUnlocked(i))
                unlocked.Add(playerSkinIcons[i]);
        }
        return unlocked;
    }

    // 해금된 훅 스킨 아이콘 리스트 반환
    public List<Sprite> GetUnlockedHookSkinIcons()
    {
        List<Sprite> unlocked = new List<Sprite>();
        for (int i = 0; i < hookSkinIcons.Count; i++)
        {
            if (IsHookSkinUnlocked(i))
                unlocked.Add(hookSkinIcons[i]);
        }
        return unlocked;
    }

    // 해금된 비니 스킨 아이콘 리스트 반환
    public List<Sprite> GetUnlockedBeanieSkinIcons()
    {
        List<Sprite> unlocked = new List<Sprite>();
        for (int i = 0; i < beanieSkinIcons.Count; i++)
        {
            if (IsBeanieSkinUnlocked(i))
                unlocked.Add(beanieSkinIcons[i]);
        }
        return unlocked;
    }

    // 해금된 가방 스킨 아이콘 리스트 반환
    public List<Sprite> GetUnlockedBagSkinIcons()
    {
        List<Sprite> unlocked = new List<Sprite>();
        for (int i = 0; i < bagSkinIcons.Count; i++)
        {
            if (IsBagSkinUnlocked(i))
                unlocked.Add(bagSkinIcons[i]);
        }
        return unlocked;
    }

    // 해금된 헬리콥터 스킨 아이콘 리스트 반환
    public List<Sprite> GetUnlockedHelicopterSkinIcons()
    {
        List<Sprite> unlocked = new List<Sprite>();
        for (int i = 0; i < helicopterSkinIcons.Count; i++)
        {
            if (IsHelicopterSkinUnlocked(i))
                unlocked.Add(helicopterSkinIcons[i]);
        }
        return unlocked;
    }

    public static Customize Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        // 기본 스킨(0번) 초기화: 비니와 가방 제외
        InitializeDefaultSkins();
    }

    private void InitializeDefaultSkins()
    {
        // Player 0번 스킨 unlock
        if (PlayerPrefs.GetInt("PlayerSkin_0_Unlocked", 0) == 0)
        {
            PlayerPrefs.SetInt("PlayerSkin_0_Unlocked", 1);
        }
        
        // Helicopter 0번 스킨 unlock
        if (PlayerPrefs.GetInt("HelicopterSkin_0_Unlocked", 0) == 0)
        {
            PlayerPrefs.SetInt("HelicopterSkin_0_Unlocked", 1);
        }
        
        // Hook 0번 스킨 unlock
        if (PlayerPrefs.GetInt("HookSkin_0_Unlocked", 0) == 0)
        {
            PlayerPrefs.SetInt("HookSkin_0_Unlocked", 1);
        }
        
        PlayerPrefs.Save();
    }

    [Header("PlayerSkin")]
    // 실제 적용되는 텍스처 리스트
    public List<Texture> playerSkins = new List<Texture>();
    // UI에 표시할 아이콘(Sprite) 리스트 (인덱스 매칭)
    public List<Sprite> playerSkinIcons = new List<Sprite>();

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
    public void EquipHookHeadSkinNumber(int index)
    {
        if (index >= 0 && index < hookSkins.Count)
        {
            Hook.currentSkin = hookSkins[index];
            // 즉시 씬의 Hook 오브젝트에 적용
            Hook.ApplyCurrentSkinToAll();
        }
    }

    // 훅과 로프 둘 다 동시에 바꾸기
    public void EquipHookSkinNumber(int index)
    {

        EquipHookHeadSkinNumber(index);
        EquipRopeSkinNumber(index);
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
    public Sprite beanieUnequipIcon; // 비니 착용 안 함 아이콘

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
    public Sprite bagUnequipIcon; // 가방 착용 안 함 아이콘

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

    public void UnlockBeanieSkinByIndex(int index)
    {
        if (index >= 0 && index < beanieSkins.Count)
        {
            PlayerPrefs.SetInt($"BeanieSkin_{index}_Unlocked", 1);
            PlayerPrefs.Save();
            Debug.Log($"[Customize] Beanie skin index {index} unlocked");
        }
    }

    public void UnlockBagSkinByIndex(int index)
    {
        if (index >= 0 && index < bagSkins.Count)
        {
            PlayerPrefs.SetInt($"BagSkin_{index}_Unlocked", 1);
            PlayerPrefs.Save();
            Debug.Log($"[Customize] Bag skin index {index} unlocked");
        }
    }

    public void UnlockHelicopterSkinByIndex(int index)
    {
        if (index >= 0 && index < helicopterSkins.Count)
        {
            PlayerPrefs.SetInt($"HelicopterSkin_{index}_Unlocked", 1);
            PlayerPrefs.Save();
            Debug.Log($"[Customize] Helicopter skin index {index} unlocked");
        }
    }

    public void UnlockHookSkinByIndex(int index)
    {
        if (index >= 0 && index < hookSkins.Count)
        {
            PlayerPrefs.SetInt($"HookSkin_{index}_Unlocked", 1);
            PlayerPrefs.Save();
            Debug.Log($"[Customize] Hook skin index {index} unlocked");
        }
    }

    public bool IsPlayerSkinUnlocked(int index)
    {
        return PlayerPrefs.GetInt($"PlayerSkin_{index}_Unlocked", 0) == 1;
    }

    public bool IsBeanieSkinUnlocked(int index)
    {
        return PlayerPrefs.GetInt($"BeanieSkin_{index}_Unlocked", 0) == 1;
    }

    public bool IsBagSkinUnlocked(int index)
    {
        return PlayerPrefs.GetInt($"BagSkin_{index}_Unlocked", 0) == 1;
    }

    public bool IsHelicopterSkinUnlocked(int index)
    {
        return PlayerPrefs.GetInt($"HelicopterSkin_{index}_Unlocked", 0) == 1;
    }

    public bool IsHookSkinUnlocked(int index)
    {
        return PlayerPrefs.GetInt($"HookSkin_{index}_Unlocked", 0) == 1;
    }
} 