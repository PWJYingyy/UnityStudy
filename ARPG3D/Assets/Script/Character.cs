using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        Dead,
        BeHit,
        Slide,
    }

    private CharacterState CurrentState;


    private MaterialPropertyBlock _materialPropertyBlock;
    private SkinnedMeshRenderer _skinnedMeshRenderer;

    public GameObject dropItem;

    private Vector3 impactOnCharacter;

    public int Coin =0 ;
    private float attackAnimTime;
    public float slideSpeed = 9.0f;
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
        if(_playerInput.MouseButtonDown){
            SwitchStateTo(CharacterState.Attacking);
            return;
        }
        if(_playerInput.SlideDown){
            SwitchStateTo(CharacterState.Slide);
            return;
        }

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

    public void SwitchStateTo(CharacterState newState){
        if(IsPlayer){
            _playerInput.Clear();
        }
        switch(CurrentState){
            case CharacterState.Normal:
                break;
            case CharacterState.Attacking:
                if(_damageCaster != null){
                    DisableDamage();
                }
                if(IsPlayer){
                    GetComponent<PlayerVFXManager>().StopBlade();
                }
                break;
            case CharacterState.Dead:
                return;
            case CharacterState.BeHit:
                break;
            case CharacterState.Slide:
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
            case CharacterState.Dead:
                _cc.enabled = false;
                _animator.SetTrigger("Dead");
                StartCoroutine(MateriaDissolve());
                break;
            case CharacterState.BeHit:
                _animator.SetTrigger("BeHit");
                break;
            case CharacterState.Slide:
                _animator.SetTrigger("Slide");
                break;
        }
        CurrentState = newState;
    }

    private void FixedUpdate() {
        switch(CurrentState){
            case CharacterState.Normal:
                if(IsPlayer){
                    CalculatePlayerMovement(); 
                }else{
                    CalculateEnemyMovement();
                }
                break;
            case CharacterState.Attacking:
                if(IsPlayer){
                    if(_playerInput.MouseButtonDown && _cc.isGrounded){
                        string name = _animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
                        attackAnimTime = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                        if(name!="LittleAdventurerAndie_ATTACK_03" && attackAnimTime >0.2f&& attackAnimTime<0.7f){
                            _playerInput.MouseButtonDown = false;
                            SwitchStateTo(CharacterState.Attacking);
                        }
                    }
                }
                break;
            case CharacterState.Dead:
                return;
            case CharacterState.BeHit:
                if(impactOnCharacter.magnitude > 0.2f){
                    _movementVelocity = impactOnCharacter * Time.deltaTime;
                }
                _movementVelocity = Vector3.Lerp(_movementVelocity, Vector3.zero, Time.deltaTime * 5);
                break;
            case CharacterState.Slide:
                _movementVelocity = transform.forward * slideSpeed * Time.deltaTime;
                break;
        }
        if(IsPlayer){
            CalculateVerticalMove();
            _cc.Move(_movementVelocity);
            _movementVelocity = Vector3.zero;
        }
    }

    public void ApplyDamage(int damage, Vector3 attackPos = new Vector3()){
        _health.ApplyDamage(damage);
        if(!IsPlayer){
            GetComponent<EnemyVFXManager>().PlayBeHitVFX(attackPos);
        }
        StartCoroutine(MaterialBlink());
        if(IsPlayer){
            SwitchStateTo(CharacterState.BeHit);
            AddImpact(attackPos, 1.0f);
        }
    }

    private void AddImpact(Vector3 attackerPos, float force)
    {
        Vector3 impactDir = transform.position- attackerPos;
        impactDir.Normalize();
        impactDir.y =0;
        impactOnCharacter = impactDir * force;
    }

    public void EnableDamage(){
        if(_damageCaster){
             Debug.Log(gameObject.name + "cur:" + _damageCaster.Damage);
        }
        _damageCaster.EnableDamage();
    }

    public void DisableDamage(){
        _damageCaster.DisableDamage();
    }

    public void AttackEnd(){
        SwitchStateTo(CharacterState.Normal);
    }

    public void BeHitEnd(){
        SwitchStateTo(CharacterState.Normal);
    }

    public void SlideEnd(){
        SwitchStateTo(CharacterState.Normal);
    }

    IEnumerator MaterialBlink(){
        _materialPropertyBlock.SetFloat("_blink", 0.4f);
        _skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);
        yield return new WaitForSeconds(0.2f);
        _materialPropertyBlock.SetFloat("_blink", 0f);
        _skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);
    }

    IEnumerator MateriaDissolve(){
        yield return new WaitForSeconds(2);
        float duration = 2f;
        float curTime =0;
        float startH = 20f;
        float endH = -10f;
        float tempH;
        _materialPropertyBlock.SetFloat("_enableDissolve",1f);
        _skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);
        while(curTime<duration){
            curTime += Time.deltaTime;
            tempH = Mathf.Lerp(startH, endH, curTime/duration);
            _materialPropertyBlock.SetFloat("_dissolve_height", tempH);
            _skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);
            yield return null;
        }
        DropItem();
    }

    private void DropItem(){
        if(dropItem != null){
            Instantiate(dropItem, transform.position, Quaternion.identity);
        }
    }

    public void PickUpItem(PickUp item){
        switch(item.Type){
            case PickUp.PickUpType.Heal:
                AddHealth(item.Value);
                break;
            case PickUp.PickUpType.Coin:
                AddCoin(item.Value);
                break;
        }
    }

    private void AddHealth(int hp){
        _health.AddHealth(hp);
        GetComponent<PlayerVFXManager>().PlayHeal();
    }

    private void AddCoin(int coin){
        Coin += coin;
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
