using UnityEngine;

public class HeartItem : MonoBehaviour
{
    [Tooltip("아이템이 유지되는 시간(초)")]
    [SerializeField] private float lifeTime = 6f;

    private void Awake()
    {
        EnsureVisual();
    }

    private void OnEnable()
    {
        if (lifeTime > 0f)
        {
            Invoke(nameof(SelfDestruct), lifeTime);
        }
    }

    public void Configure(float duration)
    {
        lifeTime = Mathf.Max(0.5f, duration);
        CancelInvoke(nameof(SelfDestruct));
        Invoke(nameof(SelfDestruct), lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var manager = GameManager.Instance;
        if (manager == null || !manager.IsGameRunning)
        {
            return;
        }

        if (!other.TryGetComponent(out PlayerMove player))
        {
            return;
        }

        manager.AddLife();
        Destroy(gameObject);
    }

    private void EnsureVisual()
    {
        if (!TryGetComponent(out SpriteRenderer renderer))
        {
            renderer = gameObject.AddComponent<SpriteRenderer>();
        }

        if (renderer.sprite == null)
        {
            renderer.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        }

        // ✅ 수정된 부분: 이미지의 원래 색상을 사용하기 위해 색상 설정 줄을 주석 처리함.
        // renderer.color = new Color(1f, 0.4f, 0.6f, 1f); 

        if (!TryGetComponent(out Collider2D collider))
        {
            collider = gameObject.AddComponent<CircleCollider2D>();
        }

        collider.isTrigger = true;
    }

    private void SelfDestruct()
    {
        Destroy(gameObject);
    }
}