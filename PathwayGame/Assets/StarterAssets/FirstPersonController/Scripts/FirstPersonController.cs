using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Player")]
        public float MoveSpeed = 4.0f;
        public float SprintSpeed = 6.0f;
        public float RotationSpeed = 1.0f;
        public float SpeedChangeRate = 10.0f;

        [Header("Head Bob Settings (Caminata)")]
        public float BobFrequency = 5.0f;
        public float BobAmount = 0.05f;
        public float BobHorizontalAmount = 0.03f;
        private float _bobTimer;
        private Vector3 _defaultCameraTargetPosition;

        [Header("Rotation Weight (Pesadez Mouse)")]
        [Range(0, 0.5f)] public float RotationSmoothTime = 0.12f; // Valores más altos = más pesado
        private float _xRotationVelocity;
        private float _yRotationVelocity;
        private float _currentPitch;
        private float _currentYaw;

        [Header("Jump & Gravity")]
        public float JumpHeight = 1.2f;
        public float Gravity = -15.0f;
        public float JumpTimeout = 0.1f;
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        public bool Grounded = true;
        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.5f;
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        public GameObject CinemachineCameraTarget;
        public float TopClamp = 90.0f;
        public float BottomClamp = -90.0f;

        // Internals
        private float _speed;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;
        private const float _threshold = 0.01f;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return false;
#endif
            }
        }

        private void Awake()
        {
            if (_mainCamera == null) _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#endif
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

            // Inicializamos valores de rotación y posición de Head Bob
            _currentYaw = transform.eulerAngles.y;
            if (CinemachineCameraTarget != null)
            {
                _defaultCameraTargetPosition = CinemachineCameraTarget.transform.localPosition;
            }
        }

        private void Update()
        {
            JumpAndGravity();
            GroundedCheck();
            Move();
            HandleHeadBob();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
        }

        private void CameraRotation()
        {
            if (_input.look.sqrMagnitude >= _threshold)
            {
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
                
                // Calculamos la rotación deseada (lo que pide el mouse)
                _currentYaw += _input.look.x * RotationSpeed * deltaTimeMultiplier;
                _currentPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;

                // Clamp del pitch (arriba/abajo)
                _currentPitch = ClampAngle(_currentPitch, BottomClamp, TopClamp);
            }

            // --- APLICAR PESADEZ (SmoothDamp) ---
            // Suavizamos la rotación actual hacia la rotación deseada
            float smoothedYaw = Mathf.SmoothDampAngle(transform.eulerAngles.y, _currentYaw, ref _xRotationVelocity, RotationSmoothTime);
            float smoothedPitch = Mathf.SmoothDampAngle(CinemachineCameraTarget.transform.localEulerAngles.x, _currentPitch, ref _yRotationVelocity, RotationSmoothTime);

            // Aplicamos rotación al cuerpo (Yaw) y al target de Cinemachine (Pitch)
            transform.rotation = Quaternion.Euler(0.0f, smoothedYaw, 0.0f);
            CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(smoothedPitch, 0.0f, 0.0f);
        }

        private void Move()
        {
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            Vector3 inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
            _controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }

        private void HandleHeadBob()
        {
            if (!Grounded) return;

            if (Mathf.Abs(_speed) > 0.1f)
            {
                _bobTimer += Time.deltaTime * (BobFrequency + (_speed * 1.5f));
                float newY = _defaultCameraTargetPosition.y + Mathf.Sin(_bobTimer) * BobAmount;
                float newX = _defaultCameraTargetPosition.x + Mathf.Cos(_bobTimer / 2) * BobHorizontalAmount;
                CinemachineCameraTarget.transform.localPosition = new Vector3(newX, newY, _defaultCameraTargetPosition.z);
            }
            else
            {
                _bobTimer = 0;
                CinemachineCameraTarget.transform.localPosition = Vector3.Lerp(
                    CinemachineCameraTarget.transform.localPosition, 
                    _defaultCameraTargetPosition, 
                    Time.deltaTime * 5f
                );
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                _fallTimeoutDelta = FallTimeout;
                if (_verticalVelocity < 0.0f) _verticalVelocity = -2f;
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                }
                if (_jumpTimeoutDelta >= 0.0f) _jumpTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                _jumpTimeoutDelta = JumpTimeout;
                if (_fallTimeoutDelta >= 0.0f) _fallTimeoutDelta -= Time.deltaTime;
                _input.jump = false;
            }

            if (_verticalVelocity < _terminalVelocity) _verticalVelocity += Gravity * Time.deltaTime;
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);
            Gizmos.color = Grounded ? transparentGreen : transparentRed;
            Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
        }
    }
}