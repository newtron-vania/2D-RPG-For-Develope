using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : Entity
{
    [Header("Attack details")]
    // 공격 이동 패턴과 카운터 공격 지속 시간
    public Vector2[] attackMovement;
    public float counterAttackDuration = .2f;

    public bool isBusy { get; private set; } // 플레이어가 바쁜 상태인지 여부

    [Header("Move info")]
    // 이동 관련 정보
    public float moveSpeed = 12f; // 이동 속도
    public float jumpForce; // 점프 힘
    public float swordReturnImpact; // 검 회수 시 영향
    private float defaultMoveSpeed; // 기본 이동 속도 저장
    private float defaultJumpForce; // 기본 점프 힘 저장

    [Header("Dash info")]
    // 대시 관련 정보
    public float dashSpeed; // 대시 속도
    public float dashDuration; // 대시 지속 시간
    private float defaultDashSpeed; // 기본 대시 속도 저장
    public float dashDir { get; private set; } // 대시 방향

    // 스킬 매니저, 검 객체, 플레이어 특수 효과 관리
    public SkillManager skill { get; private set; }
    public GameObject sword { get; private set; }
    public PlayerFX fx { get; private set; }

    #region States
    // 플레이어 상태 관리
    public PlayerStateMachine stateMachine { get; private set; }

    public PlayerIdleState idleState { get; private set; }
    public PlayerMoveState moveState { get; private set; }
    public PlayerJumpState jumpState { get; private set; }
    public PlayerAirState airState { get; private set; }
    public PlayerWallSlideState wallSlide { get; private set; }
    public PlayerWallJumpState wallJump { get; private set; }
    public PlayerDashState dashState { get; private set; }

    public PlayerPrimaryAttackState primaryAttack { get; private set; }
    public PlayerCounterAttackState counterAttack { get; private set; }

    public PlayerAimSwordState aimSowrd { get; private set; }
    public PlayerCatchSwordState catchSword { get; private set; }
    public PlayerBlackholeState blackHole { get; private set; }
    public PlayerDeadState deadState { get; private set; }
    #endregion

    protected override void Awake()
    {
        base.Awake();
        // 상태 머신과 상태 초기화
        stateMachine = new PlayerStateMachine();

        idleState = new PlayerIdleState(this, stateMachine, "Idle");
        moveState = new PlayerMoveState(this, stateMachine, "Move");
        jumpState = new PlayerJumpState(this, stateMachine, "Jump");
        airState = new PlayerAirState(this, stateMachine, "Jump");
        dashState = new PlayerDashState(this, stateMachine, "Dash");
        wallSlide = new PlayerWallSlideState(this, stateMachine, "WallSlide");
        wallJump = new PlayerWallJumpState(this, stateMachine, "Jump");

        primaryAttack = new PlayerPrimaryAttackState(this, stateMachine, "Attack");
        counterAttack = new PlayerCounterAttackState(this, stateMachine, "CounterAttack");

        aimSowrd = new PlayerAimSwordState(this, stateMachine, "AimSword");
        catchSword = new PlayerCatchSwordState(this, stateMachine, "CatchSword");
        blackHole = new PlayerBlackholeState(this, stateMachine, "Jump");

        deadState = new PlayerDeadState(this, stateMachine, "Die");
    }

    protected override void Start()
    {
        base.Start();

        // 플레이어 특수 효과 및 스킬 매니저 초기화
        fx = GetComponent<PlayerFX>();
        skill = SkillManager.instance;

        // 초기 상태를 idle 상태로 설정
        stateMachine.Initialize(idleState);

        // 기본 속도 값 저장
        defaultMoveSpeed = moveSpeed;
        defaultJumpForce = jumpForce;
        defaultDashSpeed = dashSpeed;
    }

    protected override void Update()
    {
        if (Time.timeScale == 0)
            return; // 시간이 멈췄을 때 업데이트 중지

        base.Update();

        // 현재 상태의 업데이트 메서드 호출
        stateMachine.currentState.Update();

        CheckForDashInput(); // 대시 입력 확인

        // 스킬을 사용할 수 있는지 확인
        if (Input.GetKeyDown(KeyCode.F) && skill.crystal.crystalUnlocked)
            skill.crystal.CanUseSkill();

        // 인벤토리에서 물약 사용
        if (Input.GetKeyDown(KeyCode.Alpha1))
            Inventory.instance.UseFlask();
    }

    // 엔티티의 속도를 느리게 하는 함수
    public override void SlowEntityBy(float _slowPercentage, float _slowDuration)
    {
        // 슬로우 퍼센티지만큼 속도를 줄임
        moveSpeed = moveSpeed * (1 - _slowPercentage);
        jumpForce = jumpForce * (1 - _slowPercentage);
        dashSpeed = dashSpeed * (1 - _slowPercentage);
        anim.speed = anim.speed * (1 - _slowPercentage);

        // 일정 시간이 지나면 기본 속도로 복귀
        Invoke("ReturnDefaultSpeed", _slowDuration);
    }

    // 기본 속도로 복귀시키는 함수
    protected override void ReturnDefaultSpeed()
    {
        base.ReturnDefaultSpeed();

        moveSpeed = defaultMoveSpeed;
        jumpForce = defaultJumpForce;
        dashSpeed = defaultDashSpeed;
    }

    // 새로운 검을 할당하는 함수
    public void AssignNewSword(GameObject _newSword)
    {
        sword = _newSword;
    }

    // 검을 잡는 함수
    public void CatchTheSword()
    {
        stateMachine.ChangeState(catchSword);
        Destroy(sword); // 검을 파괴함
    }

    // 일정 시간 동안 플레이어를 바쁜 상태로 만드는 함수
    public IEnumerator BusyFor(float _seconds)
    {
        isBusy = true;

        yield return new WaitForSeconds(_seconds);
        isBusy = false;
    }

    // 애니메이션 트리거 호출
    public void AnimationTrigger() => stateMachine.currentState.AnimationFinishTrigger();

    // 대시 입력을 확인하는 함수
    private void CheckForDashInput()
    {
        if (IsWallDetected())
            return; // 벽이 감지되면 대시 금지

        if (skill.dash.dashUnlocked == false)
            return; // 대시 스킬이 잠금 해제되지 않으면 대시 금지

        // 왼쪽 쉬프트 키로 대시 시도
        if (Input.GetKeyDown(KeyCode.LeftShift) && SkillManager.instance.dash.CanUseSkill())
        {
            dashDir = Input.GetAxisRaw("Horizontal"); // 대시 방향 설정

            if (dashDir == 0)
                dashDir = facingDir; // 방향이 없으면 바라보는 방향으로 대시

            stateMachine.ChangeState(dashState); // 대시 상태로 전환
        }
    }

    // 플레이어가 죽을 때 호출되는 함수
    public override void Die()
    {
        base.Die();
        stateMachine.ChangeState(deadState); // 죽음 상태로 전환
    }

    // 넉백의 힘을 0으로 설정하는 함수
    protected override void SetupZeroKnockbackPower()
    {
        knockbackPower = new Vector2(0, 0);
    }
}
