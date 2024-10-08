using System;
using System.Runtime.ExceptionServices;
using UnityEngine;
using UnityEngine.UI;

public enum SwordType
{
    Regular,  // 기본 검
    Bounce,   // 튕기는 검
    Pierce,   // 관통 검
    Spin      // 회전 검
}

public class Sword_Skill : Skill
{
    // 현재 검의 타입 (기본값은 Regular)
    public SwordType swordType = SwordType.Regular;

    [Header("Bounce info")]
    [SerializeField] private UI_SkillTreeSlot bounceUnlockButton; // Bounce 스킬 해제 버튼
    [SerializeField] private int bounceAmount; // 튕길 횟수
    [SerializeField] private float bounceGravity; // Bounce 중 중력 값
    [SerializeField] private float bounceSpeed; // Bounce 속도

    [Header("Pierce info")]
    [SerializeField] private UI_SkillTreeSlot pierceUnlockButton; // Pierce 스킬 해제 버튼
    [SerializeField] private int pierceAmount; // 관통 횟수
    [SerializeField] private float pierceGravity; // Pierce 중 중력 값

    [Header("Spin info")]
    [SerializeField] private UI_SkillTreeSlot spinUnlockButton; // Spin 스킬 해제 버튼
    [SerializeField] private float hitCooldown = .35f; // 공격 사이의 대기 시간
    [SerializeField] private float maxTravelDistance = 7; // 검이 이동할 최대 거리
    [SerializeField] private float spinDuration = 2; // Spin 지속 시간
    [SerializeField] private float spinGravity = 1; // Spin 중 중력 값

    [Header("Skill info")]
    [SerializeField] private UI_SkillTreeSlot swordUnlockButton; // 검 스킬 해제 버튼
    public bool swordUnlocked { get; private set; } // 검 스킬이 해제되었는지 여부
    [SerializeField] private GameObject swordPrefab; // 검 프리팹
    [SerializeField] private Vector2 launchForce; // 검 발사 시 힘
    [SerializeField] private float swordGravity; // 검 중력 값
    [SerializeField] private float freezeTimeDuration; // 검이 적중 시 멈추는 시간
    [SerializeField] private float returnSpeed; // 검이 돌아오는 속도

    [Header("Passive skills")]
    [SerializeField] private UI_SkillTreeSlot timeStopUnlockButton; // 시간 정지 스킬 해제 버튼
    public bool timeStopUnlocked { get; private set; } // 시간 정지 스킬이 해제되었는지 여부
    [SerializeField] private UI_SkillTreeSlot vulnerableUnlockButton; // 취약 상태 스킬 해제 버튼
    public bool vulnerableUnlocked { get; private set; } // 취약 상태 스킬이 해제되었는지 여부

    private Vector2 finalDir; // 검이 날아갈 최종 방향

    [Header("Aim dots")]
    // 조준 도트 관련 정보
    [SerializeField] private int numberOfDots; // 도트의 개수
    [SerializeField] private float spaceBeetwenDots; // 도트 간 거리
    [SerializeField] private GameObject dotPrefab; // 도트 프리팹
    [SerializeField] private Transform dotsParent; // 도트 부모 객체

    private GameObject[] dots; // 도트를 저장할 배열

    // Start 메서드에서 도트 생성 및 중력 설정
    protected override void Start()
    {
        base.Start();
        GenereateDots();
        SetupGraivty();

        // 스킬 해제 버튼에 이벤트 리스너 추가
        swordUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockSword);
        bounceUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockBounceSword);
        pierceUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockPierceSword);
        spinUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockSpinSword);
        timeStopUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockTimeStop);
        vulnerableUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockVulnurable);
    }

    // 검 타입에 따라 중력 값 설정
    private void SetupGraivty()
    {
        if (swordType == SwordType.Bounce)
            swordGravity = bounceGravity;
        else if (swordType == SwordType.Pierce)
            swordGravity = pierceGravity;
        else if (swordType == SwordType.Spin)
            swordGravity = spinGravity;
    }

    protected override void Update()
    {
        // 마우스 우클릭을 떼면 최종 방향을 설정
        if (Input.GetKeyUp(KeyCode.Mouse1))
            finalDir = new Vector2(AimDirection().normalized.x * launchForce.x, AimDirection().normalized.y * launchForce.y);

        // 마우스 우클릭을 누르고 있으면 조준 도트를 표시
        if (Input.GetKey(KeyCode.Mouse1))
        {
            for (int i = 0; i < dots.Length; i++)
            {
                dots[i].transform.position = DotsPosition(i * spaceBeetwenDots);
            }
        }
    }

    // 검을 생성하는 함수
    public void CreateSword()
    {
        GameObject newSword = Instantiate(swordPrefab, player.transform.position, transform.rotation);
        Sword_Skill_Controller newSwordScript = newSword.GetComponent<Sword_Skill_Controller>();

        // 검 타입에 따라 다른 설정 적용
        if (swordType == SwordType.Bounce)
            newSwordScript.SetupBounce(true, bounceAmount, bounceSpeed);
        else if (swordType == SwordType.Pierce)
            newSwordScript.SetupPierce(pierceAmount);
        else if (swordType == SwordType.Spin)
            newSwordScript.SetupSpin(true, maxTravelDistance, spinDuration, hitCooldown);

        // 검의 방향, 중력 등 설정
        newSwordScript.SetupSword(finalDir, swordGravity, player, freezeTimeDuration, returnSpeed);

        // 플레이어에게 새 검 할당
        player.AssignNewSword(newSword);

        // 조준 도트 비활성화
        DotsActive(false);
    }

    #region Unlock region
    // 스킬 해제 여부 확인
    protected override void CheckUnlock()
    {
        UnlockSword();
        UnlockBounceSword();
        UnlockSpinSword();
        UnlockPierceSword();
        UnlockTimeStop();
        UnlockVulnurable();
    }

    // 시간 정지 스킬 해제
    private void UnlockTimeStop()
    {
        if (timeStopUnlockButton.unlocked)
            timeStopUnlocked = true;
    }

    // 취약 상태 스킬 해제
    private void UnlockVulnurable()
    {
        if (vulnerableUnlockButton.unlocked)
            vulnerableUnlocked = true;
    }

    // 기본 검 해제
    private void UnlockSword()
    {
        if (swordUnlockButton.unlocked)
        {
            swordType = SwordType.Regular;
            swordUnlocked = true;
        }
    }

    // Bounce 검 해제
    private void UnlockBounceSword()
    {
        if (bounceUnlockButton.unlocked)
            swordType = SwordType.Bounce;
    }

    // Pierce 검 해제
    private void UnlockPierceSword()
    {
        if (pierceUnlockButton.unlocked)
            swordType = SwordType.Pierce;
    }

    // Spin 검 해제
    private void UnlockSpinSword()
    {
        if (spinUnlockButton.unlocked)
            swordType = SwordType.Spin;
    }
    #endregion

    #region Aim region
    // 플레이어와 마우스의 위치로부터 조준 방향 계산
    public Vector2 AimDirection()
    {
        Vector2 playerPosition = player.transform.position;
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mousePosition - playerPosition;

        return direction;
    }

    // 조준 도트를 활성화 또는 비활성화
    public void DotsActive(bool _isActive)
    {
        for (int i = 0; i < dots.Length; i++)
        {
            dots[i].SetActive(_isActive);
        }
    }

    // 조준 도트 생성
    private void GenereateDots()
    {
        dots = new GameObject[numberOfDots];
        for (int i = 0; i < numberOfDots; i++)
        {
            dots[i] = Instantiate(dotPrefab, player.transform.position, Quaternion.identity, dotsParent);
            dots[i].SetActive(false);
        }
    }

    // 도트의 위치 계산
    private Vector2 DotsPosition(float t)
    {
        Vector2 position = (Vector2)player.transform.position + new Vector2(
            AimDirection().normalized.x * launchForce.x,
            AimDirection().normalized.y * launchForce.y) * t + .5f * (Physics2D.gravity * swordGravity) * (t * t);

        return position;
    }
    #endregion
}
