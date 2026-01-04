using DG.Tweening;
using UnityEngine;

namespace Common.Widgets.Tumbleweed
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class TumbleweedController : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private Transform holder; // Drag your Child Sprite here
        [SerializeField] private Transform visualSprite; // Drag your Child Sprite here
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private SpriteRenderer spriteRenderer;
    
        [Header("Movement Settings")]
        [SerializeField] private float windMinForce = 2f;
        [SerializeField] private float windMaxForce = 6f;
        [SerializeField] private float initialJumpForce = 5f;
        [SerializeField] private float maxSpeed = 10f;
    
        [Header("Animation Settings")]
        [SerializeField] private float rotationMultiplier = 50f; // How fast it spins relative to speed
        [SerializeField] private Vector3 squashScale = new Vector3(1.3f, 0.7f, 1f); // Wider and shorter
        [SerializeField] private float squashDuration = 0.2f;
        [SerializeField] private float squashIntensityRandomness = 0.2f; // 20% variation in scale
        [SerializeField] private float squashDurationRandomness = 0.1f;
        
        private bool isSquashing = false;

        public void SetSpriteSortingOrder(int order)
        {
            spriteRenderer.sortingOrder = order;
        }
        
        public void SetScale(float scale)
        {
            holder.localScale = new Vector3(scale, scale, 0);
        }

        private void Start()
        {
            // Initial chaotic kick
            float randomJump = Random.Range(initialJumpForce * 0.8f, initialJumpForce * 1.2f);
            float randomForward = Random.Range(0.3f, 2f); // Slight forward variation
        
            var windForce = Random.Range(windMinForce, windMaxForce);
            rb.AddForce(new Vector2(windForce * randomForward, randomJump), ForceMode2D.Impulse);
        
            // Destroy after 10 seconds to save memory (or use a pool in production)
            Destroy(gameObject, 10f);
        }

        private void FixedUpdate()
        {
            // Apply constant wind force
            if (rb.velocity.x < maxSpeed)
            {
                rb.AddForce(Vector2.right * Random.Range(windMinForce, windMaxForce));
            }
        }

        private void Update()
        {
            HandleVisualRotation();
        }

        private void HandleVisualRotation()
        {
            if (visualSprite == null) return;

            // Manually rotate the child sprite based on the parent's X velocity
            // Negative sign assumes moving Right = Clockwise rotation
            float currentSpeed = rb.velocity.x;
            float rotationAmount = -currentSpeed * rotationMultiplier * Time.deltaTime;
        
            visualSprite.Rotate(0, 0, rotationAmount);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Only squash if we hit something "solid" (like the ground)
            // and we aren't already squashing (prevents glitchy overlapping tweens)
            if (!isSquashing && collision.relativeVelocity.magnitude > 2f)
            {
                ApplySquash();
            }
        }

        private void ApplySquash()
        {
            isSquashing = true;

            // 1. Randomize the Scale Intensity
            // We add/subtract a small amount from the base squashScale
            float randomIntensity = Random.Range(-squashIntensityRandomness, squashIntensityRandomness);
            Vector3 finalSquashVector = new Vector3(
                squashScale.x + randomIntensity, 
                squashScale.y - randomIntensity, // If it gets wider, it should get shorter
                1f
            );

            // 2. Randomize the Duration
            float finalDuration = squashDuration + Random.Range(-squashDurationRandomness, squashDurationRandomness);

            // 3. Apply the Punch
            // Vector3.one is subtracted because DOPunchScale adds the vector to the current scale
            transform.DOPunchScale(finalSquashVector - Vector3.one, finalDuration, 1, 0.5f)
                .OnComplete(() => isSquashing = false);
        }
    }
}