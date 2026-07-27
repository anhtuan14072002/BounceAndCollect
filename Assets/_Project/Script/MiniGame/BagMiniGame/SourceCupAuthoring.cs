using Unity.Entities;
using UnityEngine;

namespace Wizard
{
    public class SourceCupAuthoring : MonoBehaviour
    {
        [SerializeField] private GameObject _bagPrefab;
        public bool _isDrag;

        public bool _isCreate;
        private const float StoneScale = 0.3f;
        private BallSpawnerAuthoring _stoneGame;
        private float _timer;

        private void Awake()
        {
            _stoneGame = GetComponent<BallSpawnerAuthoring>();
            if (_stoneGame != null)
                _timer = _stoneGame.ElapsedTime;
            Application.targetFrameRate = 60;
        }

        private void Update()
        {
            if (!_isDrag) return;

            bool isDragging = Input.GetMouseButton(0);
            if (isDragging)
            {
                MoveBag();
                TryCreateStone();
            }

            _isCreate = isDragging;
        }

        private void MoveBag()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null) return;

            Transform bagTransform = _bagPrefab != null ? _bagPrefab.transform : transform;
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = mainCamera.WorldToScreenPoint(bagTransform.position).z;

            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
            Vector3 position = bagTransform.position;
            position.x = Mathf.Clamp(worldPos.x, -2f, 2f);
            bagTransform.position = position;
        }

        private void TryCreateStone()
        {
            if (_stoneGame == null) return;

            _timer += Time.deltaTime;
            if (_timer < _stoneGame.ElapsedTime) return;
            _timer = 0f;

            Transform bagTransform = _bagPrefab != null ? _bagPrefab.transform : transform;
            _stoneGame.TrySpawnStone(bagTransform.position, StoneScale);
        }

        class Baker : Baker<SourceCupAuthoring>
        {
            public override void Bake(SourceCupAuthoring authoring)
            {
                var bagEntity = GetEntity(authoring._bagPrefab, TransformUsageFlags.Dynamic);
                AddComponent(bagEntity, new SourceCupComponent
                {
                    EntityBag = bagEntity,
                });
                AddComponent(bagEntity, new SourceCupDragComponent
                {
                    IsDrag = authoring._isDrag,
                    IsCreate = authoring._isCreate,
                });
            }
        }
    }
}
