using UnityEngine;
using UnityEngine.InputSystem; 
using Unity.Cinemachine;

using System;
using System.Collections;
using UnityEngine.SceneManagement;
public enum PowerupType
{
    Health,
    SpeedBoost,
    DamageBoost,
    DoubleJump
}

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{

    public enum MovementType
    {
        Isometric,
        WallClimb,
        SideScroller,
        ThirdPerson
    }
    public MovementType currentMovementType = MovementType.Isometric;

    public event System.Action OnPlayerDamage;
    public event System.Action OnPlayerDeath;
    public event System.Action OnPlayerDash;
    #region MOVEMENT
    [HideInInspector] public bool is2DMode = false;
    [SerializeField] private float speed = 5;
    [SerializeField] private float wallClimbSpeed = 2;
    [SerializeField] private float rotationSpeed = 720f;
   
    [SerializeField] private float maxHorizontalSpeed = 15f;
    [SerializeField] private float tpsRotationSmoothTime = 0.1f;
    private float _rotationVelocity;

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
    [SerializeField] private GameObject arm2Object;
    [SerializeField] private bool startWithWeapon = false; 
    #endregion

    #region Shooting
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float baseBulletDamage = 10f;
    [SerializeField] private float attackCoolDown = 1f;
    [SerializeField] private float attackRotationLockTime = 0.5f;
    //meleeCombat
    [SerializeField] private float inputBufferWindow = 0.5f;
    [SerializeField] private float attackRecoveryDelay = 0.2f;
    private float _lastMeleeInputTime = -999f; 
    private Coroutine _activeAttackCoroutine;

    [SerializeField] private float swordDamage = 50f;
    [SerializeField] private float swordAttackRange = 1.5f;
    [SerializeField] private float comboResetTime = 0.8f;
    [SerializeField] private float swordComboDelay = 0.3f;
    [SerializeField] private LayerMask enemyLayer;
    //meleemovement
    [SerializeField] private float attackLungeSpeed = 8f;
    [SerializeField] private float attackLungeDuration = 0.2f;
    [SerializeField] private TrailRenderer weaponTrail;

    private int _currentComboStep = 0;
    private float _lastAttackTime = -999f;
    private bool _isAttacking = false;
    private bool isAttacking = false;
    private Vector3 _attackLungeVelocity;

    private float _rotationUnlockTime; 
    private float _nextFireTime = 0f;
    public event Action OnPlayerAttack;
    #endregion

    #region Jumping
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float fallGravityMultiplier = 2.5f;
    [SerializeField] private float jumpHeight = 1.6f;   
    [SerializeField] private float coyoteTime = 0.12f;  
    [SerializeField] private float jumpBuffer = 0.12f;  
    [SerializeField] private int maxAirJumps = 0;
    [SerializeField] private float jumpCooldown = 1f; 
    private float _lastJumpPerformTime = -999f;
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
    [SerializeField] private GameObject dashSmokeVFX;
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
    private enum HandState { Empty, Weapon, Weapon2 }
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
        _PlayerInputActions.Player.SelectSlot3.started += OnSelectSlot3;

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
        _PlayerInputActions.Player.SelectSlot3.started -= OnSelectSlot3;
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
        if (_isAttacking)
        {
            _moveDirectionWorld = Vector3.zero;
            return;
        }

        if (_input.magnitude < 0.1f)
        {
            _moveDirectionWorld = Vector3.zero;
            return;
        }

        switch (currentMovementType)
        {
            case MovementType.Isometric:
                Matrix4x4 iso = Matrix4x4.Rotate(Quaternion.Euler(0f, 45f, 0f));
                _moveDirectionWorld = iso.MultiplyPoint3x4(_input).normalized;
                break;

            case MovementType.WallClimb:
                Transform camTransform = _cam.transform;
                Vector3 cameraRight = camTransform.right;
                cameraRight.y = 0;
                cameraRight.Normalize();

                Vector3 targetMove = (cameraRight * _input.x) + (Vector3.up * _input.z);

                if (targetMove.sqrMagnitude > 0.001f)
                    _moveDirectionWorld = targetMove.normalized;
                else
                    _moveDirectionWorld = Vector3.zero;

                break;

            case MovementType.SideScroller: 
                _moveDirectionWorld = Vector3.right * _input.x;
                break;
            case MovementType.ThirdPerson:
                Vector3 camFwd = _cam.transform.forward;
                Vector3 camRgt = _cam.transform.right;

                camFwd.y = 0f;
                camRgt.y = 0f;
                camFwd.Normalize();
                camRgt.Normalize();

                _moveDirectionWorld = (camFwd * _input.z + camRgt * _input.x).normalized;
                break;
        }

    }


    private void ApplyRotation()
    {
        if (Time.time < _rotationUnlockTime) return;

        if (currentMovementType == MovementType.SideScroller)
        {
            
            if (_input.x > 0) transform.rotation = Quaternion.Euler(0, 90, 0);
            if (_input.x < 0) transform.rotation = Quaternion.Euler(0, -90, 0);
            return;
        }

        if (is2DMode)
        {
            transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            return;
        }
        
        
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
            if (currentMovementType == MovementType.ThirdPerson)
            {
                
                float targetAngle = Mathf.Atan2(_moveDirectionWorld.x, _moveDirectionWorld.z) * Mathf.Rad2Deg;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _rotationVelocity, tpsRotationSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }
            else
            {
               
                Quaternion targetRot = Quaternion.LookRotation(_moveDirectionWorld, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }
    }
        
        

    private void Move()
    {
        float currentSpeed = speed;
        if (currentMovementType == MovementType.WallClimb)
        {
            currentSpeed = wallClimbSpeed;
        }



        //MOEVEMENT
        Vector3 moveDir = _moveDirectionWorld * currentSpeed * _input.magnitude * Time.deltaTime;
       
        moveDir += new Vector3(_wallKickVelocity.x, 0f, _wallKickVelocity.z) * Time.deltaTime;

        moveDir += _attackLungeVelocity * Time.deltaTime;
        
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
        if (_isDead || _isStunned) return;

        if (_state == HandState.Weapon2)
        {
            _lastMeleeInputTime = Time.time;
            if (_activeAttackCoroutine == null)
            {
                _activeAttackCoroutine = StartCoroutine(AttackSequenceRoutine());
            }
            return;
        }


        if (Time.time < _nextFireTime) return;

        if (_state == HandState.Empty) return;

        if (bulletPrefab == null || firePoint == null) return;

        

        //if (_cam == null) _cam = Camera.main;

        if (TryGetMouseWorldPoint(out Vector3 hitPoint))
        {
            Vector3 lookDirection = hitPoint - transform.position;
            lookDirection.y = 0;
            if (lookDirection.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
                _rotationUnlockTime = Time.time + attackRotationLockTime;
            }
            Vector3 targetPosition = hitPoint + Vector3.up * 0.3f;
            Vector3 bulletDir = (targetPosition - firePoint.position).normalized;
            

            GameObject go = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(bulletDir, Vector3.up));
            if (go.TryGetComponent<Bullet>(out var bullet))
            {
                float finalDamage = _originalBulletDamage * _damageMultiplier;
                bullet.Init(bulletDir, finalDamage);
            }
            
            OnPlayerAttack?.Invoke();
            _nextFireTime = Time.time + attackCoolDown;
        }
    }
    private IEnumerator AttackSequenceRoutine()
    {
        _isAttacking = true;

        while (true)
        {
            if (_currentComboStep >= 3) _currentComboStep = 0;
            _currentComboStep++;

            RotateToMouseCursor();

            // Visuals
            if (weaponTrail != null) weaponTrail.emitting = true;
            OnPlayerAttack?.Invoke();

            float lungeTimer = 0f;
            while (lungeTimer < attackLungeDuration)
            {
                float t = lungeTimer / attackLungeDuration;
                float currentSpeed = Mathf.Lerp(attackLungeSpeed, 0f, t);

                _attackLungeVelocity = transform.forward * currentSpeed;

                lungeTimer += Time.deltaTime;
                yield return null;
            }
            _attackLungeVelocity = Vector3.zero;

            yield return new WaitForSeconds(attackRecoveryDelay);

            if (weaponTrail != null) weaponTrail.emitting = false;

            bool hasBufferedInput = (Time.time - _lastMeleeInputTime) <= inputBufferWindow;

            if (hasBufferedInput)
            {
                _lastMeleeInputTime = -999f;
                continue;
            }
            else
            {
                break;
            }
        }

        _isAttacking = false;
        _activeAttackCoroutine = null;

        yield return new WaitForSeconds(comboResetTime);
        _currentComboStep = 0;
    }
    private bool TryGetMouseWorldPoint(out Vector3 point)
    {
        point = default;
        if (_cam == null) return false;

        
        if (currentMovementType == MovementType.SideScroller)
        {
            
            Plane verticalPlane = new Plane(Vector3.forward, new Vector3(0, 0, transform.position.z));

            Ray ray = _cam.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (verticalPlane.Raycast(ray, out float enter))
            {
                point = ray.GetPoint(enter);
                return true;
            }
            return false; 
        }



        Ray mouseRay = _cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        
        LayerMask combinedMask = groundMask | wallMask | enemyLayer;

        if (Physics.Raycast(mouseRay, out RaycastHit hit, Mathf.Infinity, combinedMask, QueryTriggerInteraction.Ignore))
        {
            point = hit.point;
            return true;
        }

        
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));
        if (groundPlane.Raycast(mouseRay, out float planeEnter))
        {
            point = mouseRay.GetPoint(planeEnter);
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

        if (_characterController.isGrounded)
        {
            _lastGroundedTime = Time.time;
            _airJumpCount = 0;
            if (_verticalVelocity < 0f) _verticalVelocity = -2f;
        }

        
        if (currentMovementType == MovementType.WallClimb)
        {
            _verticalVelocity = 0f;
            return;
        }

        
        if (_verticalVelocity < 0)
        {
            _verticalVelocity += gravity * fallGravityMultiplier * Time.deltaTime;
        }
        else
        {
            _verticalVelocity += gravity * Time.deltaTime;
        }
    }

    private void TryConsumeJumpBuffer()
    {
        if (_isStunned) return;

        
        if (Time.time < _lastJumpPerformTime + jumpCooldown)
        {
            return;
        }
        

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
        _lastJumpPerformTime = Time.time;
    }


    private void TryStartDash()
    {

        if (_isStunned) return;
        if (Time.time < _nextDashTime) return;
        if(_isDashing) return;
        if (dashSmokeVFX != null)
        {
            // Efekti karakterin pozisyonunda yaratıyoruz.
            // Quaternion.LookRotation(-transform.forward) kullanarak 
            // efektin karakterin baktığı yönün TERSİNE (arkasına) bakmasını sağlayabiliriz.
            // Eğer VFX'in zaten daireselse Quaternion.identity kullanabilirsin.
            Vector3 vfxSpawnPos = transform.position + Vector3.up * 0.5f;
            Instantiate(dashSmokeVFX, vfxSpawnPos, Quaternion.LookRotation(-transform.forward),transform);
        }
        _dashDir = transform.forward;
        _isDashing = true;
        _dashEndTime = Time.time + dashDuration;
        _nextDashTime = Time.time + dashCooldown;
        OnPlayerDash?.Invoke();
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

        // Karakterin merkezinden başla
        Vector3 origin = transform.position + Vector3.up * (_characterController.height * 0.5f);

        // Yanal yönleri kontrol et (Forward, Right, -Right, -Forward)
        Vector3[] dirs = { transform.forward, transform.right, -transform.right, -transform.forward };

        // Raycast yerine SphereCast kullanalım (Daha kalın bir tarama yapar)
        float checkRadius = 0.25f; // Işın kalınlığı
        float checkDistance = wallCheckDistance + 0.1f; // Mesafeyi biraz toleranslı yapalım

        foreach (var dir in dirs)
        {
            // Raycast yerine SphereCast
            if (Physics.SphereCast(origin, checkRadius, dir, out RaycastHit hit, checkDistance, wallMask, QueryTriggerInteraction.Ignore))
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
            if (_verticalVelocity < 0f)
            {
                _verticalVelocity = Mathf.Max(_verticalVelocity, wallSlideGravity); // Yavaşça aşağı kay
                _isWallSliding = true;

                // EKLENECEK KISIM: STABILIZASYON
                // Karakteri duvara doğru hafifçe it ki temas kopmasın
                Vector3 pushToWall = -_lastWallNormal * 1f * Time.deltaTime;
                _characterController.Move(pushToWall);
            }
        }
    }


    private void TryWallJumpInstant()
    {

        if (_isStunned) return;
        
        if (_characterController.isGrounded) return;
        if (!IsTouchingWall(out var normal)) return;

        
        _verticalVelocity = Mathf.Sqrt(wallJumpVertical * -2f * gravity);
        OnPlayerJump?.Invoke();
        _isDashing = false;
        _dashEndTime = 0f;

        
        _wallKickDir = normal;      
        _wallKickTimeLeft = 0.12f;  

        
        
        _lastJumpPressed = -999f;

        _wallKickVelocity = _wallKickDir.normalized * wallJumpHorizontal;
        
        if (!wallKickLegacyDisplacement)
            _wallKickTimeLeft = 0f;
    }

    private void OnDamaged(float currentHealthPercent)
    {
        
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
            case PowerupType.DoubleJump:
                maxAirJumps = (int)amount;
                Debug.Log("Double jump aktif edildi.");
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

        

        Debug.Log("Oyuncu Öldü! Sahne yeniden başlatılıyor...");

        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);

        // TODO:
        // Burası "Game Over" ekranını 2-3 saniye sonra tetikleyeceğin yer.
        // Örn: FindObjectOfType<GameManager>().ShowGameOverScreen(2f);
    }

    private void HandleMeleeAttack()
    {
        if (_isAttacking && Time.time < _lastAttackTime + 0.3f) return;
        if (Time.time < _lastAttackTime + swordComboDelay)
        {
            return;
        }  
        
        float timeSinceLastAttack = Time.time - _lastAttackTime;

        
        if (timeSinceLastAttack > comboResetTime || _currentComboStep >= 3)
        {
            _currentComboStep = 0;
        }        
        _currentComboStep++;
        _lastAttackTime = Time.time;
       
        RotateToMouseCursor();

        StartCoroutine(PerformAttackLungeRoutine());
        

        OnPlayerAttack?.Invoke();
    }
    private void RotateToMouseCursor()
    {
        if (TryGetMouseWorldPoint(out Vector3 hitPoint))
        {
            Vector3 lookDirection = hitPoint - transform.position;
            lookDirection.y = 0;
            if (lookDirection.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }

    private IEnumerator PerformAttackLungeRoutine()
    {
        // Hareketi kilitle
        _isAttacking = true;

        // Trail'i aç
        if (weaponTrail != null) weaponTrail.emitting = true;

        // === 1. FİZİKSEL ATILIM (LUNGE) KISMI ===
        float lungeTimer = 0f;
        while (lungeTimer < attackLungeDuration)
        {
            // Lerp ile hızı yavaşça düşür
            float currentSpeed = Mathf.Lerp(attackLungeSpeed, 0f, lungeTimer / attackLungeDuration);
            _attackLungeVelocity = transform.forward * currentSpeed;

            lungeTimer += Time.deltaTime;
            yield return null; // Bir sonraki kareye bekle
        }

        _attackLungeVelocity = Vector3.zero;

        // === 2. ANİMASYON BEKLEME (MOVEMENT LOCK) KISMI ===
        // Sorunun Çözümü: Lunge bitse bile animasyonun bitmesini bekle!
        // Buradaki 0.4f değerini animasyonun uzunluğuna göre artırıp azaltabilirsin.
        // Toplam kilit süresi = attackLungeDuration + bu bekleme süresi olur.
        yield return new WaitForSeconds(0.4f);

        // Trail'i kapat
        if (weaponTrail != null) weaponTrail.emitting = false;

        // Hareketi serbest bırak
        _isAttacking = false;
    }
    public void AnimEvent_MeleeHit()
    {
        // Kılıcın etki alanını belirle
        Vector3 hitCenter = transform.position + transform.forward * (swordAttackRange / 2);

        // Alandaki tüm collider'ları topla
        Collider[] hitEnemies = Physics.OverlapSphere(hitCenter, swordAttackRange, enemyLayer);

        // Hiçbir şeye çarpmadıysa konsola yaz (Debug için)
        if (hitEnemies.Length == 0)
        {
            Debug.Log("Kılıç savruldu ama kimseye değmedi (LayerMask'ı kontrol et).");
        }

        foreach (Collider enemyCollider in hitEnemies)
        {
            // ÖNEMLİ DEĞİŞİKLİK BURADA:
            // Sadece çarptığımız parçaya değil, onun bağlı olduğu ana objeye (Parent) de bakıyoruz.
            Health hp = enemyCollider.GetComponent<Health>();

            if (hp == null)
            {
                hp = enemyCollider.GetComponentInParent<Health>();
            }

            // Eğer Health scripti bulunduysa hasar ver
            if (hp != null)
            {
                // İstersen 3. vuruşta (Final) ekstra hasar ver
                float dmg = _currentComboStep == 3 ? swordDamage * 2f : swordDamage;

                // Damage Boost powerup kontrolü
                dmg *= _damageMultiplier;

                hp.TakeDamage(dmg);

                // Görsel efekt (Blood VFX) veya ses burada çalınabilir
                Debug.Log($"DÜŞMAN VURULDU! Vurulan Parça: {enemyCollider.name}, Hasar: {dmg}");    
            }
            else
            {
                Debug.LogWarning($"Collider'a çarpıldı ({enemyCollider.name}) ama Health scripti bulunamadı!");
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 hitCenter = transform.position + transform.forward * (swordAttackRange / 2);
        Gizmos.DrawWireSphere(hitCenter, swordAttackRange);
    }
    #endregion

    #region PUBLIC GETTERS (Animasyon Script'i için)



    public float InputMagnitude => _input.magnitude; 

    
    public bool IsGrounded => _characterController.isGrounded; 

    
    public float VerticalVelocity => _verticalVelocity; 
    public bool IsWallSliding => _isWallSliding;
    public bool IsClimbing => is2DMode;
    public Vector3 CurrentInput => _input;
    public int CurrentComboStep => _currentComboStep;
    public int CurrentWeaponType
    {
        get
        {
            if (_state == HandState.Weapon) return 1;  
            if (_state == HandState.Weapon2) return 2; 
            return 0; 
        }
    }





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

    private void OnSelectSlot3(InputAction.CallbackContext ctx)
    {
        _state = HandState.Weapon2;  // 3 = silah
        ApplyVisualState();
    }

    private void ApplyVisualState()
    {
        if (armObject != null)
            armObject.SetActive(_state == HandState.Weapon); // Empty: kapalı, Weapon: açık

        if (arm2Object != null)
            arm2Object.SetActive(_state == HandState.Weapon2);
    }
    #endregion


   

}
