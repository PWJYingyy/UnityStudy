using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageCaster : MonoBehaviour
{
    private Collider _castCollider;

    public int Damage = 30;

    public string TargetTag;

    private List<Collider> _targetList;

    private void Awake() {
        _castCollider = GetComponent<Collider>();
        _castCollider.enabled = false;
        _targetList = new List<Collider>();
    }

    private void OnTriggerEnter(Collider other) {
        if(other.tag == TargetTag && !_targetList.Contains(other)){
            Character  targetCC = other.GetComponent<Character>();
            if(targetCC != null){
                targetCC.ApplyDamage(Damage, transform.parent.position);
                PlayerVFXManager playerVfx = GetComponentInParent<PlayerVFXManager>();
                if(playerVfx != null){
                    RaycastHit hit;
                    Vector3 orignalPos = transform.position + (-_castCollider.bounds.extents.z) * transform.forward;
                    bool isHit = Physics.BoxCast(orignalPos, _castCollider.bounds.extents/2, transform.forward, out hit, transform.rotation, _castCollider.bounds.extents.z, 1<<6);
                    if(isHit){
                        playerVfx.PlaySlash(hit.point+ new Vector3(0,1f,0));
                    }
                }
            }
            _targetList.Add(other);
        }
    }

    public void EnableDamage(){
        _castCollider.enabled = true;
        _targetList.Clear();
    }

    public void DisableDamage(){
        _castCollider.enabled = false;
        _targetList.Clear();
    }

    private void OnDrawGizmos() {
        if(_castCollider == null){
            _castCollider = GetComponent<Collider>();
        }
        RaycastHit hit;
        Vector3 orignalPos = transform.position + (-_castCollider.bounds.extents.z) * transform.forward;
        bool isHit = Physics.BoxCast(orignalPos, _castCollider.bounds.extents/2, transform.forward, out hit, transform.rotation, _castCollider.bounds.extents.z, 1<<6);
        if(isHit){
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(hit.point, 0.5f);
        }
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
