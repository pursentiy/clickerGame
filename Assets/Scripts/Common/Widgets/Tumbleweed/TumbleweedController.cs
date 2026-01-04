using DG.Tweening;
using UnityEngine;
using Utilities.Disposable;

namespace Common.Widgets.Tumbleweed
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class TumbleweedController : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private Transform holder; 
        [SerializeField] private Transform visualSprite; 
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Shadow Settings")]
        [SerializeField] private Transform shadowTransform;
        [SerializeField] private float maxShadowScale = 1.2f;
        [SerializeField] private float minShadowScale = 0.3f;
        [Tooltip("Distance from ground where shadow is smallest")]
        [SerializeField] private float maxDistanceForShadow = 5f;

        [Header("Lifespan Settings")]
        [SerializeField] private float lifeTime = 10f;
        [SerializeField] private float popOutDuration = 0.4f;

        [Header("Movement Settings")]
        [SerializeField] private float windMinForce = 2f;
        [SerializeField] private float windMaxForce = 6f;
        [SerializeField] private float initialMinJumpForce = 1f;
        [SerializeField] private float initialMaxJumpForce = 4f;
        [SerializeField] private float maxSpeed = 10f;

        [Header("Animation Settings")]
        [SerializeField] private float rotationMultiplier = 50f;
        [SerializeField] private Vector3 squashScale = new Vector3(1.3f, 0.7f, 1f);
        [SerializeField] private float squashDuration = 0.2f;
        [SerializeField] private float squashIntensityRandomness = 0.2f;
        [SerializeField] private float squashDurationRandomness = 0.1f;
    
        private bool isSquashing = false;
        private float _groundY;

        public void SetSpriteSortingOrder(int order)
        {
            spriteRenderer.sortingOrder = order;
            // Ensure shadow is slightly behind the tumbleweed
            if(shadowTransform.TryGetComponent<SpriteRenderer>(out var shadowSR))
            {
                shadowSR.sortingOrder = order - 1;
            }
        }
    
        public void SetScale(float scale)
        {
            holder.localScale = new Vector3(scale, scale, 1);
        }
    
        public void SetColor(Color color)
        {
            spriteRenderer.color = color;
        }

        private void Start()
        {
            // Capture initial ground Y (assuming it spawns near ground or hits it)
            _groundY = transform.position.y;

            var randomJump = Random.Range(initialMinJumpForce, initialMaxJumpForce);
            var randomForward = Random.Range(0.3f, 2f);
            var windForce = Random.Range(windMinForce, windMaxForce);
        
            rb.AddForce(new Vector2(windForce * randomForward, randomJump), ForceMode2D.Impulse);
    
            // Start the lifespan timer
            Invoke(nameof(PopAndDestroy), lifeTime);
        }

        private void FixedUpdate()
        {
            if (rb.velocity.x < maxSpeed)
            {
                rb.AddForce(Vector2.right * Random.Range(windMinForce, windMaxForce));
            }
        }

        private void Update()
        {
            HandleVisualRotation();
            HandleShadowScaling();
        }

        private void HandleShadowScaling()
        {
            if (shadowTransform == null) return;

            // 1. Lock shadow Y to the ground level so it doesn't fly with the tumbleweed
            shadowTransform.position = new Vector3(transform.position.x, _groundY, transform.position.z);

            // 2. Calculate distance from ground
            var distance = Mathf.Abs(transform.position.y - _groundY);
        
            // 3. Scale based on distance: Closer = Larger
            // Normalized value (0 at ground, 1 at maxDistance)
            var t = Mathf.Clamp01(distance / maxDistanceForShadow);
            var currentScale = Mathf.Lerp(maxShadowScale, minShadowScale, t);

            shadowTransform.localScale = new Vector3(currentScale, currentScale * 0.5f, 1f);
        }

        private void HandleVisualRotation()
        {
            if (visualSprite == null) return;
            var currentSpeed = rb.velocity.x;
            var rotationAmount = -currentSpeed * rotationMultiplier * Time.deltaTime;
            visualSprite.Rotate(0, 0, rotationAmount);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Update ground reference to actual collision point for better accuracy
            _groundY = collision.contacts[0].point.y;

            if (!isSquashing && collision.relativeVelocity.magnitude > 2f)
            {
                ApplySquash();
            }
        }

        private void ApplySquash()
        {
            isSquashing = true;
            var randomIntensity = Random.Range(-squashIntensityRandomness, squashIntensityRandomness);
            var finalSquashVector = new Vector3(
                squashScale.x + randomIntensity, 
                squashScale.y - randomIntensity, 
                1f
            );

            var finalDuration = squashDuration + Random.Range(-squashDurationRandomness, squashDurationRandomness);

            transform.DOPunchScale(finalSquashVector - Vector3.one, finalDuration, 1, 0.5f)
                .OnComplete(() => isSquashing = false)
                .KillWith(this);;
        }

        private void PopAndDestroy()
        {
            // "Pop" effect: Quickly scale up slightly then shrink to zero
            transform.DOScale(transform.localScale * 1.2f, 0.1f)
                .OnComplete(() => {
                    transform.DOScale(Vector3.zero, popOutDuration)
                        .SetEase(Ease.InBack)
                        .OnComplete(() => Destroy(gameObject));
                })
                .KillWith(this);
        }
    }
}