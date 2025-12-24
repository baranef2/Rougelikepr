using UnityEngine;


[RequireComponent(typeof(PlayerController))]
public class PlayerAnimator : MonoBehaviour
{
    // === REFERANSLAR ===
    private Animator _animator;
    private PlayerController _playerController;

    // === PARAMETRE ID'LERÝ (Performans için) ===
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDVerticalSpeed;
    private int _animIDWallSlide;
    private int _animIDAttack;
    private int _animIDDamage;
    private int _animIDDeath;
    private int _animIDIsClimbing;
    private int _animIDClimbX;
    private int _animIDClimbY;

    private void Awake()
    {
       
        _playerController = GetComponent<PlayerController>();


        
        _animator = GetComponent<Animator>();

        if (_animator == null)
        {
            Debug.LogError("PlayerAnimator: Alt objelerde bir Animator component'ý bulunamadý!", this);
        }

        
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDVerticalSpeed = Animator.StringToHash("VerticalSpeed");
        _animIDWallSlide = Animator.StringToHash("WallSlide");
        _animIDAttack = Animator.StringToHash("Attack");
        _animIDDamage = Animator.StringToHash("Damage");
        _animIDDeath = Animator.StringToHash("Death");
        _animIDIsClimbing = Animator.StringToHash("IsClimbing");
        _animIDClimbX = Animator.StringToHash("ClimbX");
        _animIDClimbY = Animator.StringToHash("ClimbY");
    }


    private void OnEnable()
    {
        
        _playerController.OnPlayerJump += HandlePlayerJump;
        _playerController.OnPlayerAttack += HandlePlayerAttack;
        _playerController.OnPlayerDamage += HandlePlayerDamage;
        _playerController.OnPlayerDeath += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        
        _playerController.OnPlayerJump -= HandlePlayerJump;
        _playerController.OnPlayerAttack -= HandlePlayerAttack;
        _playerController.OnPlayerDamage -= HandlePlayerDamage;
        _playerController.OnPlayerDeath -= HandlePlayerDeath;
    }

    
    private void LateUpdate()
    {
        if (_animator == null || _playerController == null) return;

        
        _animator.SetFloat(_animIDSpeed, _playerController.InputMagnitude);
        _animator.SetBool(_animIDGrounded, _playerController.IsGrounded);
        _animator.SetFloat(_animIDVerticalSpeed, _playerController.VerticalVelocity);

        
        bool isWallSliding = _playerController.IsWallSliding;
        _animator.SetBool(_animIDWallSlide, isWallSliding);

        
        bool isClimbing = _playerController.IsClimbing;
        _animator.SetBool(_animIDIsClimbing, isClimbing);

        
        if (isClimbing || isWallSliding)
        {
            
            Vector3 rawInput = _playerController.CurrentInput;

           
            float inputX = rawInput.x;
            float inputY = rawInput.z;

            
            if (Mathf.Abs(inputX) < 0.1f) inputX = 0f;
            if (Mathf.Abs(inputY) < 0.1f) inputY = 0f;

            
            _animator.SetFloat(_animIDClimbX, inputX, 0.1f, Time.deltaTime);
            _animator.SetFloat(_animIDClimbY, inputY, 0.1f, Time.deltaTime);
        }
    }

    
    private void HandlePlayerJump()
    {
        if (_animator == null) return;

        
        _animator.SetTrigger(_animIDJump);
    }

    
    private void HandlePlayerAttack()
    {
        if (_animator == null) return;

        
        _animator.SetTrigger(_animIDAttack);
    }

    private void HandlePlayerDamage()
    {
        if (_animator == null) return;

        
        _animator.SetTrigger(_animIDDamage);
    }

    private void HandlePlayerDeath()
    {
        if (_animator == null) return;

        
        _animator.SetTrigger(_animIDDeath);
    }
}