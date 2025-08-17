using UnityEngine;
using UnityEngine.EventSystems;
using PuzzleGame.Core.Interfaces;
using PuzzleGame.Core.Helpers;

namespace PuzzleGame.Gameplay.Managers
{
    public class InputManager : Singleton<InputManager>
    {
        [Header("Input Settings")]
        [SerializeField] private LayerMask clickableLayer = ~0;
        [SerializeField] private float maxClickDistance = 100f;

        private Camera gameCamera;
        private IInputHandler inputHandler;

        protected override void Awake()
        {
            base.Awake();
            gameCamera = Camera.main ?? FindObjectOfType<Camera>();

#if UNITY_EDITOR || UNITY_STANDALONE
            inputHandler = new MouseInputHandler();
#elif UNITY_ANDROID || UNITY_IOS
            inputHandler = new TouchInputHandler();
#endif
        }

        private void Update()
        {
            if (inputHandler == null) return;

            if (inputHandler.IsInputDown())
            {
                Vector3 screenPos = inputHandler.GetInputPosition();

                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    return;

                HandleClick(screenPos);
            }
        }

        private void HandleClick(Vector3 screenPosition)
        {
            Ray ray = gameCamera.ScreenPointToRay(screenPosition);

            if (Physics.Raycast(ray, out var hit, maxClickDistance, clickableLayer))
            {
                var clickable = hit.collider.GetComponent<IClickable>();
                if (clickable != null && clickable.IsClickable)
                {
                    clickable.OnClick();
                }
            }
        }
    }
}