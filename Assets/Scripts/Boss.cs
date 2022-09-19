using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Boss : Enemy
{
    public GameObject missile;
    public Transform missileSiloA;
    public Transform missileSiloB;

    Vector3 lookVec;
    Vector3 tauntVec;
    public bool isLook;

    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        meshs = GetComponentsInChildren<MeshRenderer>();
        nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        nav.isStopped = true;
        StartCoroutine(Think());
    }

    // Update is called once per frame
    void Update()
    {
        if(isDead){
            StopAllCoroutines();
            return;
        }

        if(isLook){
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            lookVec = new Vector3(h,0,v)*5;
            transform.LookAt(target.position + lookVec);
        }
        else{
            nav.SetDestination(tauntVec);
        }
    }

    IEnumerator Think()
    {
        yield return new WaitForSeconds(0.1f);
        
        int ranAction = Random.Range(0, 5);
        switch(ranAction){
            case 0:
            case 1:
                //미사일
                StartCoroutine(MissileShot());
                break;
            case 2:
            case 3:
                //기구한
                StartCoroutine(RockShot());
                break;
            case 4:
                //두부박살
                StartCoroutine(Taunt());
                break;
        }
    }

    IEnumerator MissileShot()
    {
        anim.SetTrigger("doShot");
        yield return new WaitForSeconds(0.2f);
        GameObject instantMissleA = Instantiate(missile, missileSiloA.position, missileSiloA.rotation);
        BossMissile bossMissileA = instantMissleA.GetComponent<BossMissile>();
        bossMissileA.target=target;

        yield return new WaitForSeconds(0.3f);
        GameObject instantMissleB = Instantiate(missile, missileSiloB.position, missileSiloB.rotation);
        BossMissile bossMissileB = instantMissleB.GetComponent<BossMissile>();
        bossMissileB.target=target;

        yield return new WaitForSeconds(2f);
        
        StartCoroutine(Think());
    }

    IEnumerator RockShot()
    {
        isLook = false;
        anim.SetTrigger("doBigShot");
        Instantiate(bullet, transform.position, transform.rotation);
        yield return new WaitForSeconds(3f);

        isLook = true;
        StartCoroutine(Think());
    }

    IEnumerator Taunt()
    {
        tauntVec = target.position + lookVec;
        
        isLook = false;
        nav.isStopped = false;
        boxCollider.enabled =false;
        anim.SetTrigger("doTaunt");

        yield return new WaitForSeconds(1f);
        meleeArea.enabled = true;
        
        yield return new WaitForSeconds(1f);
        isLook = true;
        nav.isStopped = true;
        boxCollider.enabled = true;
        meleeArea.enabled = false;
        StartCoroutine(Think());
    }
}
