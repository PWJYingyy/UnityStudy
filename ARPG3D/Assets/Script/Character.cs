using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    private CharacterController _cc;
    public float MoveSpeed = 5f;
    private Vector3 _movementVelocity;
    private PlayerInput _playerInput;
    private float _verticalVelocity;
    private Animator _animator;
    public float Gravity = -9.8f;
    public bool IsPlayer = false;
    private Transform TargetPlayer;
    private UnityEngine.AI.NavMeshAgent _navMeshAgent;

    private Health _health;

    private DamageCaster _damageCaster;

    public enum CharacterState{
        Normal,
        Attacking,
    }

    private CharacterState CurrentState;


    private MaterialPropertyBlock _materialPropertyBlock;
    private SkinnedMeshRenderer _skinnedMeshRenderer;

    private void Awake() {
        _cc = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _health = GetComponent<Health>();
        _damageCaster = GetComponentInChildren<DamageCaster>();

        _skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        _materialPropertyBlock = new MaterialPropertyBlock();
        _skinnedMeshRenderer.GetPropertyBlock(_materialPropertyBlock);

        if(IsPlayer){
            _playerInput = GetComponent<PlayerInput>();    
        }else{
            _navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            TargetPlayer = GameObject.FindWithTag("Player").transform;
            _navMeshAgent.speed = MoveSpeed;
        }
    }

    private void CalculatePlayerMovement(){
        _movementVelocity.Set(_playerInput.HorizontalInput, 0f,_playerInput.VerticalInput);
        _movementVelocity.Normalize();
        //修正角度,因为相机的Y倾斜了45
        _movementVelocity = Quaternion.Euler(0,-45f,0) * _movementVelocity;
         _animator.SetFloat("Speed", _movementVelocity.magnitude);
        _movementVelocity *= MoveSpeed * Time.deltaTime;
        if(_movementVelocity != Vector3.zero){
            transform.rotation = Quaternion.LookRotation(_movementVelocity);
        }
    }

    private void CalculateVerticalMove(){
        if(_cc.isGrounded == false){
            _verticalVelocity = Gravity;
        }else{
            _verticalVelocity = Gravity * 0.3f;
        }
        _movementVelocity += _verticalVelocity * Vector3.up * Time.deltaTime;
         _animator.SetBool("AirBorne", !_cc.isGrounded);
    }

    private void CalculateEnemyMovement(){
        if(TargetPlayer == null){
            return;
        }
        if(Vector3.Distance(TargetPlayer.position, transform.position) >= _navMeshAgent.stoppingDistance){
            _navMeshAgent.SetDestination(TargetPlayer.position);
            _animator.SetFloat("Speed", 0.2f);
        }else{
             _navMeshAgent.SetDestination(transform.position);
            SwitchStateTo(CharacterState.Attacking);
            _animator.SetFloat("Speed", 0);
        }
    }

    private void SwitchStateTo(CharacterState newState){
        if(IsPlayer){
            _playerInput.MouseButtonDown = false;
        }
        switch(CurrentState){
            case CharacterState.Normal:
                break;
            case CharacterState.Attacking:
                if(_damageCaster != null){
                    DisableDamage();
                }
                break;
        }
        switch(newState){
            case CharacterState.Normal:
                break;
            case CharacterState.Attacking:
                if(!IsPlayer){
                    transform.rotation = Quaternion.LookRotation(TargetPlayer.position- transform.position); 
                }
                _animator.SetTrigger("Attack");
                break;
        }
        CurrentState = newState;
    }

    private void FixedUpdate() {
        switch(CurrentState){
            case CharacterState.Normal:
                if(IsPlayer){
                    if(_playerInput.MouseButtonDown){
                        SwitchStateTo(CharacterState.Attacking);
                        break;
                    }
                    CalculatePlayerMovement();
                    CalculateVerticalMove();
                    _cc.Move(_movementVelocity);    
                }else{
                    CalculateEnemyMovement();
                }
                break;
            case CharacterState.Attacking:
                break;
        }
    }

    public void ApplyDamage(int damage, Vector3 attackPos = new Vector3()){
        _health.ApplyDamage(damage);
        if(!IsPlayer){
            GetComponent<EnemyVFXManager>().PlayBeHitVFX(attackPos);
        }
        StartCoroutine(MaterialBlink());
    }

    public void EnableDamage(){
        _damageCaster.EnableDamage();
    }

    public void DisableDamage(){
        _damageCaster.DisableDamage();
    }

    public void AttackEnd(){
        SwitchStateTo(CharacterState.Normal);
    }

    IEnumerator MaterialBlink(){
        _materialPropertyBlock.SetFloat("_blink", 0.4f);
        _skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);
        yield return new WaitForSeconds(0.2f);
        _materialPropertyBlock.SetFloat("_blink", 0f);
        _skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
