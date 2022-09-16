using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee, Range };
    public Type type;
    public int damage;
    public float rate;
    public int maxAmmo;
    public int curAmmo;

    public BoxCollider meleeArea;
    public TrailRenderer trailEffect;
    public Transform bulletsPos;
    public GameObject bullets;
    public Transform bulletsCasePos;
    public GameObject bulletsCase;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Use()
    {
        if(type == Type.Melee){
            StartCoroutine("Swing");
        }
        else if(type ==Type.Range && curAmmo > 0)
        {
            curAmmo--;
            StartCoroutine("Shot");
        }
    }

    IEnumerator Swing()
    {
        yield return new WaitForSeconds(0.4f); //0.1초 대기
        meleeArea.enabled = true;
        trailEffect.enabled = true;

        yield return new WaitForSeconds(0.1f); 
        meleeArea.enabled = false;
        
        yield return new WaitForSeconds(0.3f); 
        trailEffect.enabled = false;        
    }

    //Use() 메인루틴 -> Swing() 서브루틴 -> Use() 메인루틴
    //Use() 메인루틴 + Swing() 코루틴 (Co-Op)
    IEnumerator Shot()
    {
        //#1.총알 발사
        GameObject intantBullet = Instantiate(bullets, bulletsPos.position, bulletsPos.rotation);
        Rigidbody bulletRigid = intantBullet.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletsPos.forward * 50;

        yield return null;
        //#2. 탄피 배출
        GameObject intantCase = Instantiate(bulletsCase, bulletsCasePos.position, bulletsCasePos.rotation);
        Rigidbody caseRigid = intantCase.GetComponent<Rigidbody>();
        Vector3 caseVec = bulletsCasePos.forward*Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3);
        caseRigid.AddForce(caseVec, ForceMode.Impulse);
        caseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse);
    }
}
