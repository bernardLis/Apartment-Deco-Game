using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ApartmentDeco
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] CinemachineCamera _camera;
        PlayerInput _playerInput;
        Vector3 _inputDirection;

        /* INPUT */
        void OnEnable()
        {
            if (_playerInput == null)
                _playerInput = GameManager.Instance.GetComponent<PlayerInput>();
            UnsubscribeInputActions();
            SubscribeInputActions();
        }

        void OnDisable()
        {
            if (_playerInput == null) return;

            UnsubscribeInputActions();
        }

        void FixedUpdate()
        {
            Move();
        }

        void GetMovementVector(InputAction.CallbackContext context)
        {
            Vector3 inputValue = context.ReadValue<Vector2>();
            inputValue = inputValue.normalized;

            _inputDirection = new(inputValue.x, inputValue.y);
        }

        void ResetMovementVector(InputAction.CallbackContext context)
        {
            if (this == null) return;
            _inputDirection = Vector3.zero;
        }

        void Move()
        {
            transform.localPosition += _inputDirection * (Time.fixedDeltaTime * 10);
        }

        void ZoomIn(InputAction.CallbackContext obj)
        {
            Zoom(-0.5f);
        }

        void ZoomOut(InputAction.CallbackContext obj)
        {
            Zoom(0.5f);
        }

        void Zoom(float f)
        {
            float targetZoom = _camera.Lens.OrthographicSize + f;
            _camera.Lens.OrthographicSize = Mathf.Clamp(targetZoom, 3, 8);
        }

        void SubscribeInputActions()
        {
            _playerInput.actions["Move"].performed += GetMovementVector;
            _playerInput.actions["Move"].canceled += ResetMovementVector;
            _playerInput.actions["ZoomIn"].performed += ZoomIn;
            _playerInput.actions["ZoomOut"].performed += ZoomOut;
        }


        void UnsubscribeInputActions()
        {
            _playerInput.actions["Move"].performed -= GetMovementVector;
            _playerInput.actions["Move"].canceled -= ResetMovementVector;
            _playerInput.actions["ZoomIn"].performed -= ZoomIn;
            _playerInput.actions["ZoomOut"].performed -= ZoomOut;
        }
    }
}