using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFX : EntityFX
{
    [Header("Screen shake FX")]
    // 스크린 흔들림 효과 관련 변수
    [SerializeField] private float shakeMultiplier; // 흔들림 강도 배수
    public Vector3 shakeSwordImpact; // 검 충돌 시 흔들림 세기
    public Vector3 shakeHighDamage; // 높은 피해 시 흔들림 세기
    private CinemachineImpulseSource screenShake; // 시네머신의 임펄스 소스

    [Header("After image fx")]
    // 잔상 효과 관련 변수
    [SerializeField] private GameObject afterImagePrefab; // 잔상 프리팹
    [SerializeField] private float colorLooseRate; // 잔상이 사라지는 속도
    [SerializeField] private float afterImageCooldown; // 잔상 생성 쿨다운 시간
    private float afterImageCooldownTimer; // 잔상 쿨다운 타이머
    [Space]
    [SerializeField] private ParticleSystem dustFx; // 먼지 효과

    // Start 메서드: 초기 설정
    protected override void Start()
    {
        base.Start();
        screenShake = GetComponent<CinemachineImpulseSource>(); // 시네머신 임펄스 소스 초기화
    }

    // 매 프레임마다 호출되는 Update 메서드
    private void Update()
    {
        afterImageCooldownTimer -= Time.deltaTime; // 잔상 쿨다운 타이머 감소
    }

    // 스크린 흔들림 효과 생성
    public void ScreenShake(Vector3 _shakePower)
    {
        // 플레이어의 바라보는 방향에 따라 X축 흔들림 세기 설정
        screenShake.m_DefaultVelocity = new Vector3(_shakePower.x * player.facingDir, _shakePower.y) * shakeMultiplier;
        screenShake.GenerateImpulse(); // 임펄스 생성
    }

    // 잔상 생성
    public void CreateAfterImage()
    {
        // 쿨다운 시간이 지나야 잔상 생성
        if (afterImageCooldownTimer < 0)
        {
            afterImageCooldownTimer = afterImageCooldown; // 쿨다운 타이머 초기화
            GameObject newAfterImage = Instantiate(afterImagePrefab, transform.position, transform.rotation); // 잔상 프리팹 생성
            newAfterImage.GetComponent<AfterImageFX>().SetupAfterImage(colorLooseRate, sr.sprite); // 잔상 설정
        }
    }

    // 먼지 효과 실행
    public void PlayDustFX()
    {
        if (dustFx != null)
            dustFx.Play(); // 먼지 효과 실행
    }
}
