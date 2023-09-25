using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHp;

    public int curHp;

    private Character _cc;

    private void Awake() {
        curHp = maxHp;
        _cc = GetComponent<Character>();
    }

    public void ApplyDamage(int damage){
        curHp -= damage;
        CheckDead();
        Debug.Log(gameObject.name + "cur:" + curHp +"damage:"+ damage);
    }

    public void AddHealth(int hp){
        curHp += hp;
        if(curHp>maxHp) curHp = maxHp;
        Debug.Log(gameObject.name + "cur:" + curHp +"heal:"+ hp);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void CheckDead(){
        if(curHp<=0){
            _cc.SwitchStateTo(Character.CharacterState.Dead);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
