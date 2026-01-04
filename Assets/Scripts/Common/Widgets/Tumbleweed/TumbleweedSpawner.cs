using UnityEngine;

namespace Common.Widgets.Tumbleweed
{
    public class TumbleweedSpawner: MonoBehaviour
    {
        [SerializeField] private TumbleweedController tumbleweedPrefab;
        [SerializeField] private Transform tumbleweedContainer;
        [SerializeField] private int tumbleweedSortingOrder;
        [SerializeField] private float tumbleweedScale;

        [Header("Spawn Offsets")]
        [Tooltip("Distance to the left of the screen edge (Negative values move it further left)")]
        [SerializeField] private float xOffset = -1f; 
        [Tooltip("Vertical offset from the center of the screen")]
        [SerializeField] private float yOffset = 0f;
        
        [Header("Floor Settings")]
        [SerializeField] private EdgeCollider2D floorCollider;
        [Tooltip("How far from the bottom of the screen the floor should be")]
        [SerializeField] private float floorYOffset = 0.5f;
        
        [Header("Spawn Timing")]
        [SerializeField] private float minSpawnInterval = 1f;
        [SerializeField] private float maxSpawnInterval = 6f;

        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void Start()
        {
            SetupFloorCollider();
            
            var initialDelay = Random.Range(minSpawnInterval, maxSpawnInterval);
            Invoke(nameof(SpawnNext), initialDelay);
        }
        
        private void SpawnNext()
        {
            Spawn();

            // Schedule the NEXT spawn with a brand new random time
            var nextDelay = Random.Range(minSpawnInterval, maxSpawnInterval);
            Invoke(nameof(SpawnNext), nextDelay);
        }
        
        private void SetupFloorCollider()
        {
            if (floorCollider == null) return;

            // 1. Get world positions for left and right edges
            // Viewport (0,0) is bottom-left, (1,0) is bottom-right
            var leftWorld = mainCamera.ViewportToWorldPoint(new Vector3(-0.25f, 0, 0));
            var rightWorld = mainCamera.ViewportToWorldPoint(new Vector3(1.25f, 0, 0));

            leftWorld.y = floorCollider.transform.position.y + floorYOffset;
            rightWorld.y = floorCollider.transform.position.y + floorYOffset;
            
            // 3. Convert these world points to the LOCAL space of the collider's transform
            // This is important if your container/collider isn't at (0,0,0)
            var localLeft = floorCollider.transform.InverseTransformPoint(new Vector3(leftWorld.x, leftWorld.y, 0));
            var localRight = floorCollider.transform.InverseTransformPoint(new Vector3(rightWorld.x, rightWorld.y, 0));

            // 4. Apply to the EdgeCollider2D
            var newPoints = new Vector2[] { localLeft, localRight };
            floorCollider.points = newPoints;
        }

        private void Spawn()
        {
            // 1. Get the world position of the left edge of the screen (Viewport X = 0)
            // ViewportToWorldPoint uses (x, y, z) where z is distance from camera
            var spawnPos = new Vector3(floorCollider.transform.position.x, floorCollider.transform.position.y, 0);

            // 2. Apply your custom offsets
            spawnPos.x += xOffset;
            spawnPos.y += yOffset;

            // 3. Instantiate
            var tumbleweed = Instantiate(tumbleweedPrefab, spawnPos, Quaternion.identity);
        
            if (tumbleweedContainer != null)
            {
                tumbleweed.transform.SetParent(tumbleweedContainer);
            }

            // Ensure your TumbleweedController has this public method
            tumbleweed.SetSpriteSortingOrder(tumbleweedSortingOrder);
            tumbleweed.SetScale(tumbleweedScale);
        }
    }
}