using System.Collections;
using UnityEngine;

public class Entity : MonoBehaviour
{
    #region Components
    // Animator, Rigidbody2D, SpriteRenderer 등 컴포넌트에 접근하기 위한 속성
    public Animator anim { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public SpriteRenderer sr { get; private set; }
    public CharacterStats stats { get; private set; }
    public CapsuleCollider2D cd { get; private set; }
    #endregion

    [Header("Knockback info")]
    // 피격 시 넉백(뒤로 밀리는 효과)에 대한 정보
    [SerializeField] protected Vector2 knockbackPower = new Vector2(7,12); // 넉백의 힘 (X, Y 방향)
    [SerializeField] protected Vector2 knockbackOffset = new Vector2(.5f,2); // 넉백 시 오프셋 범위
    [SerializeField] protected float knockbackDuration = .07f; // 넉백 지속 시간
    protected bool isKnocked; // 넉백 상태 여부

    [Header("Collision info")]
    // 충돌 감지 관련 정보
    public Transform attackCheck; // 공격 범위 체크를 위한 트랜스폼
    public float attackCheckRadius = 1.2f; // 공격 체크 범위 반경
    [SerializeField] protected Transform groundCheck; // 바닥 감지용 트랜스폼
    [SerializeField] protected float groundCheckDistance = 1; // 바닥 감지 거리
    [SerializeField] protected Transform wallCheck; // 벽 감지용 트랜스폼
    [SerializeField] protected float wallCheckDistance = .8f; // 벽 감지 거리
    [SerializeField] protected LayerMask whatIsGround; // 바닥으로 인식할 레이어

    public int knockbackDir { get; private set; } // 넉백 방향
    public int facingDir { get; private set; } = 1; // 캐릭터의 바라보는 방향 (1: 오른쪽, -1: 왼쪽)
    protected bool facingRight = true; // 오른쪽을 보고 있는지 여부

    public System.Action onFlipped; // 캐릭터 방향이 바뀔 때 실행되는 콜백

    // 컴포넌트를 초기화하는 함수 (자식 클래스에서 오버라이드 가능)
    protected virtual void Awake()
    {
    }

    // 게임 오브젝트가 처음 활성화될 때 호출되는 함수
    protected virtual void Start()
    {
        // 컴포넌트 초기화
        sr = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<CharacterStats>();
        cd = GetComponent<CapsuleCollider2D>();
    }

    // 매 프레임마다 호출되는 함수 (자식 클래스에서 오버라이드 가능)
    protected virtual void Update()
    {
    }

    // 엔티티의 속도를 느리게 하는 함수
    public virtual void SlowEntityBy(float _slowPercentage, float _slowDuration)
    {
    }

    // 애니메이션 속도를 기본값으로 되돌리는 함수
    protected virtual void ReturnDefaultSpeed()
    {
        anim.speed = 1;
    }

    // 피격 시 넉백 효과를 적용하는 함수
    public virtual void DamageImpact() => StartCoroutine("HitKnockback");

    // 공격 방향에 따른 넉백 방향 설정
    public virtual void SetupKnockbackDir(Transform _damageDirection)
    {
        if (_damageDirection.position.x > transform.position.x)
            knockbackDir = -1; // 공격이 오른쪽에서 들어오면 왼쪽으로 넉백
        else if (_damageDirection.position.x < transform.position.x)
            knockbackDir = 1; // 공격이 왼쪽에서 들어오면 오른쪽으로 넉백
    }

    // 넉백의 힘을 설정하는 함수
    public void SetupKnockbackPower(Vector2 _knockbackpower) => knockbackPower = _knockbackpower;

    // 피격 시 넉백을 적용하는 코루틴
    protected virtual IEnumerator HitKnockback()
    {
        isKnocked = true; // 넉백 상태 활성화

        float xOffset = Random.Range(knockbackOffset.x, knockbackOffset.y); // 넉백 오프셋 값 랜덤 설정

        // 넉백의 힘이 0 이상이면 넉백 적용
        if(knockbackPower.x > 0 || knockbackPower.y > 0)
            rb.velocity = new Vector2((knockbackPower.x + xOffset) * knockbackDir, knockbackPower.y);

        // 넉백 지속 시간만큼 대기
        yield return new WaitForSeconds(knockbackDuration);
        isKnocked = false; // 넉백 상태 비활성화
        SetupZeroKnockbackPower(); // 넉백 종료 후 넉백 힘 초기화
    }

    // 넉백의 힘을 초기화하는 함수
    protected virtual void SetupZeroKnockbackPower()
    {
    }

    #region Velocity
    // 엔티티의 속도를 0으로 설정하는 함수
    public void SetZeroVelocity()
    {
        if (isKnocked)
            return;

        rb.velocity = new Vector2(0, 0);
    }

    // 엔티티의 속도를 설정하는 함수
    public void SetVelocity(float _xVelocity, float _yVelocity)
    {
        if (isKnocked)
            return;

        rb.velocity = new Vector2(_xVelocity, _yVelocity);
        FlipController(_xVelocity); // 방향 전환 감지
    }
    #endregion

    #region Collision
    // 바닥이 감지되었는지 확인하는 함수
    public virtual bool IsGroundDetected() => Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);

    // 벽이 감지되었는지 확인하는 함수
    public virtual bool IsWallDetected() => Physics2D.Raycast(wallCheck.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround);

    // 편집기에서 충돌 감지 영역을 시각적으로 표시하는 함수
    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawLine(groundCheck.position, new Vector3(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance * facingDir, wallCheck.position.y));
        Gizmos.DrawWireSphere(attackCheck.position, attackCheckRadius);
    }
    #endregion

    #region Flip
    // 캐릭터의 방향을 전환하는 함수
    public virtual void Flip()
    {
        facingDir = facingDir * -1; // 바라보는 방향 반전
        facingRight = !facingRight; // 오른쪽/왼쪽 여부 변경
        transform.Rotate(0, 180, 0); // 캐릭터의 시각적 회전

        // 방향이 바뀔 때 호출할 콜백 실행
        if(onFlipped != null)
            onFlipped();
    }

    // 이동 속도에 따라 방향을 전환하는 함수
    public virtual void FlipController(float _x)
    {
        if (_x > 0 && !facingRight)
            Flip(); // 오른쪽으로 이동할 때 오른쪽을 보도록 전환
        else if (_x < 0 && facingRight)
            Flip(); // 왼쪽으로 이동할 때 왼쪽을 보도록 전환
    }

    // 기본 바라보는 방향을 설정하는 함수
    public virtual void SetupDefailtFacingDir(int _direction)
    {
        facingDir = _direction;

        if (facingDir == -1)
            facingRight = false;
    }
    #endregion

    // 엔티티가 사망할 때 호출되는 함수 (자식 클래스에서 오버라이드 가능)
    public virtual void Die()
    {
    }
}
