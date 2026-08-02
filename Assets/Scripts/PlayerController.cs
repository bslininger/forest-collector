using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _turnSpeed;
    [SerializeField] private Animator _animator;
    [SerializeField] private InputActionReference _moveAction;
    private float _verticalVelocity;
    private CharacterController _characterController;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _verticalVelocity = -2f;
    }

    private void OnEnable()
    {
        _moveAction.action.Enable();
    }

    private void OnDisable()
    {
        _moveAction.action.Disable();
    }

    void Update()
    {
        Vector2 input = _moveAction.action.ReadValue<Vector2>();
        float forwardInput = input.y;
        float turnInput = input.x;
        transform.Rotate(0f, turnInput * _turnSpeed * Time.deltaTime, 0f);

        if (_characterController.isGrounded && _verticalVelocity < 0f)
        {
            _verticalVelocity = -2f;
        }
        _verticalVelocity += -9.81f * Time.deltaTime;

        Vector3 movement = transform.forward * forwardInput * _moveSpeed * Time.deltaTime;
        movement.y = _verticalVelocity * Time.deltaTime;
        
        _characterController.Move(movement);

        _animator.SetFloat("Vert", forwardInput, 0.15f, Time.deltaTime);
        _animator.SetFloat("Hor", turnInput, 0.15f, Time.deltaTime);
        _animator.SetFloat("State", 0f);
        _animator.SetBool("IsJump", false);
    }

    public void SetMovementEnabled(bool enabled)
    {
        if (enabled)
            _moveAction.action.Enable();
        else
            _moveAction.action.Disable();
    }
}
