using UnityEngine;
using UnityEngine.EventSystems;
using PuzzleGame.Core.Interfaces;
using PuzzleGame.Core.Helpers;

namespace PuzzleGame.Gameplay.Managers
{
    public class InputManager : Singleton<InputManager>
    {
        [Header("Input Settings")]
        [SerializeField] private LayerMask clickableLayer = ~0; // Varsayılan olarak her layer
        [SerializeField] private float maxClickDistance = 100f;

        private Camera gameCamera;

        protected override void Awake()
        {
            base.Awake();
            gameCamera = Camera.main;
            if (gameCamera == null)
                gameCamera = FindObjectOfType<Camera>();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                HandleClick(Input.mousePosition);
            }
        }

        private void HandleClick(Vector3 screenPosition)
        {
            // UI üzerinde tıklama varsa atla
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Ray ray = gameCamera.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, maxClickDistance, clickableLayer))
            {
                var clickable = hit.collider.GetComponentInParent<IClickable>();
                if (clickable != null && clickable.IsClickable)
                {
                    clickable.OnClick();
                }
                
            }
        }
    }
}