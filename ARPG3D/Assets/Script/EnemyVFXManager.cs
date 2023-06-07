using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
public class EnemyVFXManager : MonoBehaviour
{
    public VisualEffect FootStep;
    public VisualEffect Attack;
    public ParticleSystem BeHitVFX;
    public VisualEffect beHitSplashVFX;

    public void BurstFootStep(){
        FootStep.SendEvent("OnPlay");
    }

    public void PlayAttackVFX(){
        Attack.Play();
    }

    public void PlayBeHitVFX(Vector3 attckPos){
        Vector3 forceForward = transform.position - attckPos;
        forceForward.Normalize();
        forceForward.y=0;
        BeHitVFX.transform.rotation = Quaternion.LookRotation(forceForward);
        BeHitVFX.Play();

        Vector3 splashPos = transform.position;
        splashPos.y += 2f;
        VisualEffect newSplash = Instantiate(beHitSplashVFX,splashPos,Quaternion.identity);
        newSplash.SendEvent("OnPlay");
        Destroy(newSplash.gameObject, 5f);
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
