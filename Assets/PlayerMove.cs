using UnityEngine;
using System.Collections; // IEnumerator를 위해 필요

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove : MonoBehaviour
{
    [Tooltip("플레이어 이동 속도")]
    public float moveSpeed = 5f;

    [Header("Idle 스프라이트")]
    [SerializeField] private Sprite[] idleSprites; // 정면 Idle 애니메이션 프레임

    [Header("Walk 스프라이트 (4방향)")]
    [SerializeField] private Sprite[] walkDownSprites;
    [SerializeField] private Sprite[] walkUpSprites;
    [SerializeField] private Sprite[] walkSideSprites; // 좌우는 flipX로 처리

    [Header("애니메이션 설정")]
    [SerializeField] private float animationSpeed = 0.1f; // 각 프레임 전환 시간

    private Rigidbody2D rb;
    private SpriteRenderer sr; // SpriteRenderer 참조 추가
    private Vector2 movement;

    private Sprite[] currentAnimationSprites; // 현재 재생 중인 애니메이션 프레임 배열
    private Coroutine animationCoroutine; // 애니메이션 코루틴 참조
    private int currentFrameIndex; // 현재 프레임 인덱스

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>(); // SpriteRenderer 컴포넌트 가져오기
    }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameRunning)
        {
            movement = Vector2.zero;
            PlayAnimation(idleSprites, Vector2.zero); // 게임이 멈추면 idle로
            return;
        }

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // 이동 방향에 따른 애니메이션 선택 및 재생
        PlayAnimation(DetermineAnimationSprites(movement), movement);
    }

    private Sprite[] DetermineAnimationSprites(Vector2 currentMovement)
    {
        if (currentMovement == Vector2.zero)
        {
            return idleSprites;
        }
        else if (currentMovement.x != 0) // 좌우 이동 우선
        {
            sr.flipX = currentMovement.x < 0; // 왼쪽으로 이동하면 뒤집기
            return walkSideSprites;
        }
        else if (currentMovement.y > 0) // 위쪽 이동
        {
            sr.flipX = false;
            return walkUpSprites;
        }
        else // 아래쪽 이동
        {
            sr.flipX = false;
            return walkDownSprites;
        }
    }

    private void PlayAnimation(Sprite[] newSprites, Vector2 currentMovement)
    {
        // 현재 애니메이션과 다른 스프라이트 배열이거나,
        // 현재 애니메이션이 정지해 있는데 이동을 시작했을 경우에만 새 애니메이션 시작
        if (currentAnimationSprites != newSprites || (animationCoroutine == null && newSprites.Length > 0))
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            currentAnimationSprites = newSprites;
            if (currentAnimationSprites.Length > 0)
            {
                animationCoroutine = StartCoroutine(AnimateSprite(newSprites, currentMovement));
            }
            else // 스프라이트 배열이 비어있으면 기본 스프라이트 (예: idle의 첫 프레임)로 설정
            {
                sr.sprite = idleSprites.Length > 0 ? idleSprites[0] : null;
                animationCoroutine = null; // 코루틴을 실행할 필요 없음
            }
        }
        // 이동 중이 아니지만 애니메이션이 이미 재생 중인 경우 (ex: idle)
        else if (currentMovement == Vector2.zero && animationCoroutine == null && idleSprites.Length > 0)
        {
            sr.sprite = idleSprites[0]; // idle의 첫 프레임으로 고정
        }
    }

    private IEnumerator AnimateSprite(Sprite[] sprites, Vector2 currentMovement)
    {
        if (sprites == null || sprites.Length == 0) yield break;

        // 이동 중이 아니면 첫 프레임으로 고정하거나 idle 애니메이션 한 번만 재생
        if (currentMovement == Vector2.zero)
        {
            sr.sprite = sprites[0];
            yield break;
        }

        currentFrameIndex = 0;
        while (true)
        {
            sr.sprite = sprites[currentFrameIndex];
            currentFrameIndex = (currentFrameIndex + 1) % sprites.Length; // 다음 프레임으로 순환
            yield return new WaitForSeconds(animationSpeed);
        }
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameRunning)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 target = movement.sqrMagnitude > 1f ? movement.normalized : movement;
        rb.MovePosition(rb.position + target * moveSpeed * Time.fixedDeltaTime);
    }
}