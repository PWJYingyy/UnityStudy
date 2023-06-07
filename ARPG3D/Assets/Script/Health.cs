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
        //Debug.Log(gameObject.name + "cur:" + curHp +"damage:"+ damage);
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
