using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState 
{
    protected PlayerStateMachine stateMachine; // 플레이어 상태 머신 참조
    protected Player player; // 플레이어 객체 참조

    protected Rigidbody2D rb; // 플레이어의 Rigidbody2D 컴포넌트 참조

    protected float xInput; // 수평 입력 값
    protected float yInput; // 수직 입력 값
    private string animBoolName; // 애니메이션 bool 이름

    protected float stateTimer; // 상태 타이머
    protected bool triggerCalled; // 애니메이션 트리거가 호출되었는지 여부

    // 생성자: 상태 머신, 플레이어, 애니메이션 bool 이름을 받아 초기화
    public PlayerState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName)
    {
        this.player = _player;
        this.stateMachine = _stateMachine;
        this.animBoolName = _animBoolName;
    }

    // 상태에 진입할 때 호출되는 함수
    public virtual void Enter()
    {
        player.anim.SetBool(animBoolName, true); // 해당 상태의 애니메이션 활성화
        rb = player.rb; // 플레이어의 Rigidbody2D 컴포넌트 가져오기
        triggerCalled = false; // 트리거가 호출되지 않음
    }

    // 상태가 지속되는 동안 호출되는 함수 (매 프레임)
    public virtual void Update()
    {
        stateTimer -= Time.deltaTime; // 상태 타이머 감소

        // 입력 값 업데이트
        xInput = Input.GetAxisRaw("Horizontal"); // 수평 입력 값
        yInput = Input.GetAxisRaw("Vertical"); // 수직 입력 값

        // Y축 속도를 애니메이션 파라미터로 전달
        player.anim.SetFloat("yVelocity", rb.velocity.y);
    }

    // 상태를 종료할 때 호출되는 함수
    public virtual void Exit()
    {
        player.anim.SetBool(animBoolName, false); // 애니메이션 bool 비활성화
    }

    // 애니메이션이 완료될 때 호출되는 트리거 함수
    public virtual void AnimationFinishTrigger()
    {
        triggerCalled = true; // 트리거가 호출됨을 표시
    }
}