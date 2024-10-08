using Cinemachine;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class EntityFX : MonoBehaviour
{
    protected Player player; // 플레이어 참조
    protected SpriteRenderer sr; // 스프라이트 렌더러 참조

    [Header("Pop Up Text")]
    // 팝업 텍스트 프리팹
    [SerializeField] private GameObject popUpTextPrefab;

    [Header("Flash FX")]
    // 플래시 효과 관련 변수
    [SerializeField] private float flashDuration; // 플래시 지속 시간
    [SerializeField] private Material hitMat; // 피격 시 사용할 재질
    private Material originalMat; // 원래 재질 저장

    [Header("Ailment colors")]
    // 상태 이상(ignite, chill, shock) 색상
    [SerializeField] private Color[] igniteColor;
    [SerializeField] private Color[] chillColor;
    [SerializeField] private Color[] shockColor;

    [Header("Ailment particles")]
    // 상태 이상(ignite, chill, shock) 파티클
    [SerializeField] private ParticleSystem igniteFx;
    [SerializeField] private ParticleSystem chillFx;
    [SerializeField] private ParticleSystem shockFx;

    [Header("Hit FX")]
    // 피격 시 사용할 효과
    [SerializeField] private GameObject hitFx;
    [SerializeField] private GameObject criticalHitFx; // 치명타 시 효과

    private GameObject myHealthBar; // 체력바 객체 참조

    // Start 메서드: 초기 설정
    protected virtual void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>(); // 자식 오브젝트의 SpriteRenderer 컴포넌트 가져오기
        player = PlayerManager.instance.player; // 플레이어 참조

        originalMat = sr.material; // 원래 재질 저장

        myHealthBar = GetComponentInChildren<UI_HealthBar>(true).gameObject; // 체력바 찾기
    }

    // 팝업 텍스트 생성
    public void CreatePopUpText(string _text)
    {
        float randomX = Random.Range(-1, 1); // X축 오프셋 랜덤 설정
        float randomY = Random.Range(1.5f, 3); // Y축 오프셋 랜덤 설정

        Vector3 positionOffset = new Vector3(randomX, randomY, 0); // 텍스트 위치 오프셋

        // 팝업 텍스트 인스턴스 생성
        GameObject newText = Instantiate(popUpTextPrefab, transform.position + positionOffset, Quaternion.identity);

        newText.GetComponent<TextMeshPro>().text = _text; // 텍스트 설정
    }

    // 엔티티를 투명하게 만들거나 원래대로 되돌림
    public void MakeTransprent(bool _transprent)
    {
        if (_transprent)
        {
            myHealthBar.SetActive(false); // 체력바 비활성화
            sr.color = Color.clear; // 투명하게 설정
        }
        else
        {
            myHealthBar.SetActive(true); // 체력바 활성화
            sr.color = Color.white; // 흰색으로 설정
        }
    }

    // 피격 시 플래시 효과를 나타내는 코루틴
    private IEnumerator FlashFX()
    {
        sr.material = hitMat; // 피격 재질로 변경
        Color currentColor = sr.color; // 현재 색상 저장
        sr.color = Color.white; // 흰색으로 설정

        yield return new WaitForSeconds(flashDuration); // 플래시 지속 시간만큼 대기

        sr.color = currentColor; // 원래 색상 복원
        sr.material = originalMat; // 원래 재질 복원
    }

    // 빨간색으로 깜박이는 효과
    private void RedColorBlink()
    {
        if (sr.color != Color.white)
            sr.color = Color.white; // 흰색으로 변경
        else
            sr.color = Color.red; // 빨간색으로 변경
    }

    // 색상 변경을 취소하고 상태 이상 파티클 중지
    private void CancelColorChange()
    {
        CancelInvoke(); // 모든 Invoke 호출 취소
        sr.color = Color.white; // 흰색으로 설정

        igniteFx.Stop(); // Ignite 파티클 중지
        chillFx.Stop(); // Chill 파티클 중지
        shockFx.Stop(); // Shock 파티클 중지
    }

    // Ignite 상태 효과를 일정 시간 동안 실행
    public void IgniteFxFor(float _seconds)
    {
        igniteFx.Play(); // Ignite 파티클 실행

        InvokeRepeating("IgniteColorFx", 0, .3f); // 색상 변경 반복
        Invoke("CancelColorChange", _seconds); // 일정 시간 후 색상 변경 취소
    }

    // Chill 상태 효과를 일정 시간 동안 실행
    public void ChillFxFor(float _seconds)
    {
        chillFx.Play(); // Chill 파티클 실행
        InvokeRepeating("ChillColorFx", 0, .3f); // 색상 변경 반복
        Invoke("CancelColorChange", _seconds); // 일정 시간 후 색상 변경 취소
    }

    // Shock 상태 효과를 일정 시간 동안 실행
    public void ShockFxFor(float _seconds)
    {
        shockFx.Play(); // Shock 파티클 실행
        InvokeRepeating("ShockColorFx", 0, .3f); // 색상 변경 반복
        Invoke("CancelColorChange", _seconds); // 일정 시간 후 색상 변경 취소
    }

    // Ignite 색상 전환 효과
    private void IgniteColorFx()
    {
        if (sr.color != igniteColor[0])
            sr.color = igniteColor[0]; // 첫 번째 Ignite 색상으로 변경
        else
            sr.color = igniteColor[1]; // 두 번째 Ignite 색상으로 변경
    }

    // Chill 색상 전환 효과
    private void ChillColorFx()
    {
        if (sr.color != chillColor[0])
            sr.color = chillColor[0]; // 첫 번째 Chill 색상으로 변경
        else
            sr.color = chillColor[1]; // 두 번째 Chill 색상으로 변경
    }

    // Shock 색상 전환 효과
    private void ShockColorFx()
    {
        if (sr.color != shockColor[0])
            sr.color = shockColor[0]; // 첫 번째 Shock 색상으로 변경
        else
            sr.color = shockColor[1]; // 두 번째 Shock 색상으로 변경
    }

    // 피격 효과 생성 (치명타 여부 포함)
    public void CreateHitFx(Transform _target, bool _critical)
    {
        float zRotation = Random.Range(-90, 90); // Z축 회전 랜덤 설정
        float xPosition = Random.Range(-.5f, .5f); // X축 위치 오프셋 랜덤 설정
        float yPosition = Random.Range(-.5f, .5f); // Y축 위치 오프셋 랜덤 설정

        Vector3 hitFxRotaion = new Vector3(0, 0, zRotation); // 히트 이펙트 회전 설정

        GameObject hitPrefab = hitFx; // 기본 피격 효과 설정

        if (_critical)
        {
            hitPrefab = criticalHitFx; // 치명타 피격 효과 설정

            float yRotation = 0;
            zRotation = Random.Range(-45, 45); // 치명타 시 Z축 회전 범위 축소

            if (GetComponent<Entity>().facingDir == -1)
                yRotation = 180; // 방향에 따라 Y축 회전 변경

            hitFxRotaion = new Vector3(0, yRotation, zRotation); // 치명타 피격 이펙트 회전 설정
        }

        // 피격 효과 생성 후 0.5초 후 파괴
        GameObject newHitFx = Instantiate(hitPrefab, _target.position + new Vector3(xPosition, yPosition), Quaternion.identity);
        newHitFx.transform.Rotate(hitFxRotaion); // 피격 이펙트 회전 적용
        Destroy(newHitFx, .5f); // 0.5초 후 파괴
    }
}
