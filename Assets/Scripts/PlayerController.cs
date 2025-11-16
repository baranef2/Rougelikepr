using UnityEngine;
using UnityEngine.InputSystem; 
using Unity.Cinemachine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using System;
using System.Collections;

public enum PowerupType
{
    Health,
    SpeedBoost,
    DamageBoost
}

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{

    public event System.Action OnPlayerDamage;
    public event System.Action OnPlayerDeath;
    #region MOVEMENT

    [SerializeField] private float speed = 5;
    [SerializeField] private float rotationSpeed = 720f;
    [Header("Speed Caps")]
    [SerializeField] private float maxHorizontalSpeed = 15f;

    #endregion

    #region POWERUPS
    private bool _isSpeedBoosted = false;
    private bool _isDamageBoosted = false;
    private float _originalSpeed;
    private float _originalBulletDamage; 
    private float _damageMultiplier = 1f; 
    #endregion


    #region VISUALS

    [SerializeField] private GameObject armObject;
    [SerializeField] private bool startWithWeapon = false; 
    #endregion

    #region Shooting
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float baseBulletDamage = 10f;

    public event Action OnPlayerAttack;
    #endregion

    #region Jumping
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.6f;   
    [SerializeField] private float coyoteTime = 0.12f;  
    [SerializeField] private float jumpBuffer = 0.12f;  
    [SerializeField] private int maxAirJumps = 0;
    [SerializeField] private GameObject jumpSmokeVFX;
    public event Action OnPlayerJump;
    
    private float _verticalVelocity;
    private float _lastGroundedTime;
    private float _lastJumpPressed;
    private int _airJumpCount;
    #endregion

    #region DASH
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.18f;
    [SerializeField] private float dashCooldown = 0.6f;
    private bool _isDashing;
    private float _dashEndTime;
    private float _nextDashTime;
    private Vector3 _dashDir;
    #endregion

    #region WALLJUMP
    [SerializeField] private LayerMask wallMask;     
    [SerializeField] private float wallCheckDistance = 0.6f;
    [SerializeField] private float wallSlideGravity = -3.5f; 
    [SerializeField] private float wallJumpHorizontal = 6f;  
    [SerializeField] private float wallJumpVertical = 1.4f;
    [SerializeField] private float wallKickDecay = 12f;   
    [SerializeField] private bool wallKickLegacyDisplacement = false; 
    private bool _isWallSliding;
    private Vector3 _lastWallNormal;

    
    private float _wallKickTimeLeft;
    private Vector3 _wallKickDir;
    

    private Vector3 _wallKickVelocity;
    #endregion

    #region STUN FEEDBACK
    [SerializeField] private float stunDuration = 0.15f;
    private bool _isStunned;
    private bool _isDead;
    [SerializeField] private GameObject bloodSplashVFX;
    #endregion

    #region INPUTSYSTEM ACTIONS
    private InputSystem_Actions _PlayerInputActions;
    #endregion

    #region dahili _input ve charactercontroller
    private Vector3 _input;
    private CharacterController _characterController;
    private Vector3 _moveDirectionWorld;

    #endregion

    #region HOLSTER
    private enum HandState { Empty, Weapon }
    private HandState _state;

    private Health _health;
    private Camera _cam;
    #endregion

    #region AWAKE
    private void Awake()
    {
        _PlayerInputActions = new InputSystem_Actions();
        _characterController = GetComponent<CharacterController>();
        _health = GetComponent<Health>();
        _originalSpeed = speed;
        _originalBulletDamage = baseBulletDamage;
        _state = startWithWeapon ? HandState.Weapon : HandState.Empty;
        _cam = Camera.main;
        _lastGroundedTime = -1f;
        _lastJumpPressed = -1f;

    }
    #endregion
    #region OnEnable-OnDisable
    private void OnEnable()
    {
        _PlayerInputActions.Player.Enable();

        
        _PlayerInputActions.Player.SelectSlot1.started += OnSelectSlot1; 
        _PlayerInputActions.Player.SelectSlot2.started += OnSelectSlot2; 

        _PlayerInputActions.Player.Fire.started += OnFire;
        _PlayerInputActions.Player.Jump.performed += ctx => _lastJumpPressed = Time.time;
        _PlayerInputActions.Player.Dash.performed += _ => TryStartDash();
        _PlayerInputActions.Player.Jump.performed += _ => TryWallJumpInstant();
       
        if(_health != null)
        {
            _health.OnTakeDamage.AddListener(OnDamaged);
            _health.OnDeath.AddListener(HandlePlayerDeath);
        }
        
        ApplyVisualState();

        
    }

    private void OnDisable()
    {
        _PlayerInputActions.Player.SelectSlot1.started -= OnSelectSlot1;
        _PlayerInputActions.Player.SelectSlot2.started -= OnSelectSlot2;
        _PlayerInputActions.Player.Fire.started -= OnFire;
        _PlayerInputActions.Player.Jump.performed -= ctx => _lastJumpPressed = Time.time;
        _PlayerInputActions.Player.Dash.performed -= _ => TryStartDash();
        _PlayerInputActions.Player.Jump.performed -= _ => TryWallJumpInstant();
        if(_health != null)
        {
            _health.OnTakeDamage.RemoveListener(OnDamaged);
            _health.OnDeath.RemoveListener(HandlePlayerDeath);
        }   

        _PlayerInputActions.Player.Disable();

        
    }
    #endregion
    #region UPDATES
    private void Update()
    {

        if (_isDead) return;

        GatherInput();
        CalculateMoveDirection();
        ApplyGravityAndGroundCheck();
        ApplyWallSlide();
        HandleDash();
        ApplyRotation();
        TryConsumeJumpBuffer();
        Move();
        
    }
    #endregion
    #region METHODS
    private void GatherInput()
    {
        Vector2 input = _PlayerInputActions.Player.Move.ReadValue<Vector2>();
        _input = new Vector3(input.x, 0f, input.y);

        if (_isStunned)
            _input = Vector3.zero;
    }

    private void CalculateMoveDirection()
    {
        if(_input == Vector3.zero) { 
        
        _moveDirectionWorld = Vector3.zero;
            return;
        }

        Matrix4x4 iso = Matrix4x4.Rotate(Quaternion.Euler(0f, 45f,0f));

        Vector3 dir = iso.MultiplyPoint3x4(_input);
        _moveDirectionWorld = dir.normalized;
    }


    private void ApplyRotation()
    {
        if (_isWallSliding)
        {
            Vector3 wallLookDir = -_lastWallNormal;
            wallLookDir.y = 0;

            if (wallLookDir.sqrMagnitude < 0.001f) return;

            Quaternion targetRot = Quaternion.LookRotation(wallLookDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRot, rotationSpeed *  Time.deltaTime);
            return;
        }

        if(_moveDirectionWorld != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(_moveDirectionWorld, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation , targetRot, rotationSpeed * Time.deltaTime);
        }
    }
        
        

    private void Move()
    {
        //MOEVEMENT
        Vector3 moveDir = _moveDirectionWorld * speed * _input.magnitude * Time.deltaTime;
       
        moveDir += new Vector3(_wallKickVelocity.x, 0f, _wallKickVelocity.z) * Time.deltaTime;


        
        if (_wallKickVelocity.sqrMagnitude > 0.0001f)
        {
            _wallKickVelocity = Vector3.MoveTowards(
                _wallKickVelocity, Vector3.zero, wallKickDecay * Time.deltaTime);
        }

        _characterController.Move(moveDir);
        //JUMP
        Vector3 vertical = new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime;
        _characterController.Move(vertical);
        //DASH
        if (_isDashing)
        {
            Vector3 dashDisplacement = _dashDir * dashSpeed *Time.deltaTime;
            _characterController.Move(dashDisplacement);
        }
        //WALL JUMP
        if(_wallKickTimeLeft > 0)
        {
            _characterController.Move(_wallKickDir * wallJumpHorizontal * Time.deltaTime);
            _wallKickTimeLeft -= Time.deltaTime;
        }

    }
    private void OnFire(InputAction.CallbackContext ctx)
    {
        if (_state != HandState.Weapon) return;

        if (bulletPrefab == null || firePoint == null) return;

        

        //if (_cam == null) _cam = Camera.main;

        if (TryGetMouseWorldPoint(out Vector3 hitPoint))
        {
            Vector3 dir = (hitPoint - firePoint.position);
            dir.y = 0;
            if (dir.sqrMagnitude < 0.0001f) return;

            GameObject go = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(dir, Vector3.up));
            if (go.TryGetComponent<Bullet>(out var bullet))
            {
                float finalDamage = _originalBulletDamage * _damageMultiplier;
                bullet.Init(dir, finalDamage);
            }

            OnPlayerAttack?.Invoke();

        }
    }

    private bool TryGetMouseWorldPoint(out Vector3 point)
    {
        point = default;
        if (_cam == null) return false;

        Ray ray = _cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        LayerMask combinedMask = groundMask | wallMask;

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, combinedMask, QueryTriggerInteraction.Ignore))
        {
            point = hit.point;
            return true;
        }

        //  YEDEK OYUNCUNUN AYAK HİZASINDA SONSUZ DÜZLEM
        Plane plane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));
        if (plane.Raycast(ray, out float enter))
        {
            point = ray.GetPoint(enter);
            return true;
        }
        return false;
    }
    private void ApplyGravityAndGroundCheck()
    {
        if (_characterController.isGrounded)
        {
            _lastGroundedTime = Time.time;
            _airJumpCount = 0;
            if (_verticalVelocity < 0f) _verticalVelocity = -2f;
        }
        _verticalVelocity += gravity * Time.deltaTime;
    }

    private void TryConsumeJumpBuffer()
    {
        if (_isStunned) return;
        bool canGroundJump = (Time.time - _lastGroundedTime) <= coyoteTime;
        bool bufferedPress = (Time.time - _lastJumpPressed) <= jumpBuffer;
        bool canAirJump = (!canGroundJump && _airJumpCount < maxAirJumps);
        if ((bufferedPress && canGroundJump) || (bufferedPress && canAirJump))
        {
            PerformJump();
            _lastJumpPressed = -999f;
        }
    }

    private void PerformJump()
    {
        if (bloodSplashVFX != null)
        {
            Vector3 spawnPosition = transform.position;
            Instantiate(jumpSmokeVFX, spawnPosition, Quaternion.identity);
        }
        _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        OnPlayerJump?.Invoke();

        if ((Time.time - _lastGroundedTime) > coyoteTime)
            _airJumpCount++;
    }


    private void TryStartDash()
    {

        if (_isStunned) return;
        if (Time.time < _nextDashTime) return;
        if(_isDashing) return;

        _dashDir = transform.forward;
        _isDashing = true;
        _dashEndTime = Time.time + dashDuration;
        _nextDashTime = Time.time + dashCooldown;
    }


    private void HandleDash()
    {
        if(!_isDashing) return;

        // dash sırasında hafifçe düşme fonksiyonu (DENENECEK)
        // _verticalVelocity = Mathf.Max(_verticalVelocity, wallSlideGravity);

        if (Time.time >= _dashEndTime)
        {
            _isDashing = false;
        }
    }


    private bool IsTouchingWall(out Vector3 wallNormal)
    {
        wallNormal = Vector3.zero;
        if (_characterController.isGrounded) return false;

        Vector3 origin = transform.position + Vector3.up * (_characterController.height * 0.5f);
        Vector3[] dirs = { transform.forward, transform.right, -transform.right };

        foreach (var dir in dirs)
        {
            if(Physics.Raycast(origin , dir , out RaycastHit hit , wallCheckDistance , wallMask , QueryTriggerInteraction.Ignore))
            {
                wallNormal = hit.normal;
                return true;
            }
        }
        return false;
    }


    private void ApplyWallSlide()
    {
        _isWallSliding = false;

        if (IsTouchingWall(out _lastWallNormal))
        {
            if(_verticalVelocity < 0f)
            {
                _verticalVelocity = Mathf.Max(_verticalVelocity, wallSlideGravity);
                _isWallSliding = true;
            }
        }
    }


    private void TryWallJumpInstant()
    {

        if (_isStunned) return;
        // Yerde değilsek ve duvara değiyorsak anında wall jump uygulama
        if (_characterController.isGrounded) return;
        if (!IsTouchingWall(out var normal)) return;

        // Yukarı hız ver
        _verticalVelocity = Mathf.Sqrt(wallJumpVertical * -2f * gravity);
        OnPlayerJump?.Invoke();
        _isDashing = false;
        _dashEndTime = 0f;

        // Kısa süreli yana itiş için pencere aç
        _wallKickDir = normal;      
        _wallKickTimeLeft = 0.12f;  

        // Havadaki zıplama haklarını sıfırlamak 
        // _airJumpCount = 0;
        
        _lastJumpPressed = -999f;

        _wallKickVelocity = _wallKickDir.normalized * wallJumpHorizontal;
        
        if (!wallKickLegacyDisplacement)
            _wallKickTimeLeft = 0f;
    }

    private void OnDamaged(float currentHealthPercent)
    {
        //blood VFX
        if(bloodSplashVFX != null)
        {
            Vector3 spawnPosition = transform.position + _characterController.center;
            Instantiate(bloodSplashVFX, spawnPosition, Quaternion.identity);
        }        
        
        
        
        if (_isStunned) return;
        
        if (currentHealthPercent <= 0 ) return;

        StartCoroutine(StunRoutine());
        OnPlayerDamage?.Invoke();
    }


    private IEnumerator StunRoutine()
    {   _isStunned = true;
        //HASAR ALMA ANİMASYONUNU BURADA TETİKLE
        //animator.SetTrigger("Hit");

        yield return new WaitForSeconds(stunDuration);
        _isStunned = false;

    }

    public void ApplyPowerup(PowerupType type , float amount , float duration)
    {
        switch (type)
        {
            case PowerupType.Health:
                _health.Heal(amount); 
                break;

            case PowerupType.SpeedBoost:
                if (!_isSpeedBoosted)
                {
                    StartCoroutine(SpeedBoostRoutine(amount,duration));
                }
                break;
            case PowerupType.DamageBoost:
                if (!_isDamageBoosted)
                {
                    StartCoroutine(DamageBoostRoutine(amount,duration));       
                }
                break;
        }
    }

    private IEnumerator SpeedBoostRoutine(float amountPercent, float duration)
    {
        _isSpeedBoosted = true;

        speed= _originalSpeed*amountPercent;
        yield return new WaitForSeconds(duration);
        speed = _originalSpeed;
        _isSpeedBoosted= false;
    }


    private IEnumerator DamageBoostRoutine(float amountPercent, float duration)
    {
        _isDamageBoosted = true;
        _damageMultiplier = amountPercent;

        yield return new WaitForSeconds(duration);
        _damageMultiplier = 1f;
        _isDamageBoosted= false;
    }

    private void HandlePlayerDeath()
    {
        
        if (_isDead) return;

        _isDead = true;

        OnPlayerDeath?.Invoke();
        _PlayerInputActions.Player.Disable();

        

        Debug.Log("Oyuncu Öldü!");

        

        // TODO:
        // Burası "Game Over" ekranını 2-3 saniye sonra tetikleyeceğin yer.
        // Örn: FindObjectOfType<GameManager>().ShowGameOverScreen(2f);
    }
    #endregion

    #region PUBLIC GETTERS (Animasyon Script'i için)



    public float InputMagnitude => _input.magnitude; 

    
    public bool IsGrounded => _characterController.isGrounded; 

    
    public float VerticalVelocity => _verticalVelocity; 
    public bool IsWallSliding => _isWallSliding; 



    
    #endregion
    #region CALLBACKS

    private void OnSelectSlot1(InputAction.CallbackContext ctx)
    {
        _state = HandState.Empty;   // 1 = boş el
        ApplyVisualState();
    }

    private void OnSelectSlot2(InputAction.CallbackContext ctx)
    {
        _state = HandState.Weapon;  // 2 = silah
        ApplyVisualState();
    }

    private void ApplyVisualState()
    {
        if (armObject != null)
            armObject.SetActive(_state == HandState.Weapon); // Empty: kapalı, Weapon: açık
    }
    #endregion
   
    



}
