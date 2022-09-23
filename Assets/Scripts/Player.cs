using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    public GameObject[] weapons;
    public bool[] hasWeapons;
    public Camera followCamera;
    public GameObject grenadeObj;
    public GameManager manager;

    public GameObject missile;
    public int m_shotCount = 12; // 총 몇 개 발사할건지.
    [Range(0, 1)] public float m_interval = 0.15f;
    public int m_shotCountEveryInterval = 2; // 한번에 몇 개씩 발사할건지.
    public Transform target;

    int weaponIndex;

    public int score;

    public int ammo;
    public int coin;
    public int health;
    public int hasGrenade;

    public GameObject[] grenades;

    public int maxAmmo;
    public int maxCoin;
    public int maxHealth;
    public int maxHasGrenade;

    float hAxis;
    float vAxis;

    bool wDown;
    bool jDown;
    bool iDown;
    bool sDown1;
    bool sDown2;
    bool sDown3;
    bool fDown;
    bool rDown;
    bool gDown;
    bool qDown;

    bool isJump;
    bool isDodge;
    bool isSwap;
    bool isFireReady;
    bool isReload;
    bool isBorder;
    bool isDamage;
    bool isShop;
    bool isDead;
    bool isQ;

    Vector3 moveVec;
    Vector3 dodgeVec;

    Rigidbody rigid;
    Animator anim;
    MeshRenderer[] meshs;

    GameObject nearObject;
    public Weapon equipWeapon;
    float fireDelay;
    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        weaponIndex = -1;
        isFireReady = true;
        meshs = GetComponentsInChildren<MeshRenderer>();

        // PlayerPrefs.SetInt("MaxScore", 112500);
        // Debug.Log(PlayerPrefs.GetInt("MaxScore"));
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Dodge();
        Interation();
        Swap();
        Attack();
        Reload();
        Grenade();
        SkillQ();
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        iDown = Input.GetButtonDown("Interation");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");
        fDown = Input.GetButton("Fire1");
        gDown = Input.GetButtonDown("Fire2");
        rDown = Input.GetButtonDown("Reload");
        qDown = Input.GetButtonDown("SkillQ");
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;
        if(isDodge)
        {
            moveVec = dodgeVec;
        }

        if(isSwap || !isFireReady || isDead)
            moveVec = Vector3.zero;

        if(!isBorder)
            transform.position += moveVec * speed * (wDown?0.3f:1f) * Time.deltaTime;
        //rigid.MovePosition(moveVec * speed * (wDown?0.3f:1f) * Time.deltaTime);
        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);
    }

    void Turn()
    {
        //#1. 키보드에 의한 회전
        transform.LookAt(transform.position + moveVec);

        //#2. 마우스에 의한 회전
        if(fDown && !isDead)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if(Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0;
                transform.LookAt(transform.position + nextVec);
            }
        }
        
    }

    void Jump()
    {
        if(jDown && moveVec == Vector3.zero && !isJump && !isDodge && !isSwap && !isDead)
        {
            rigid.AddForce(Vector3.up * 9, ForceMode.Impulse);
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }

    void Dodge()
    {
        if(jDown && moveVec != Vector3.zero && !isDodge && !isJump && !isSwap && !isReload && !isDead)
        {
            dodgeVec = moveVec;
            speed *= 2;
            anim.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.4f);
        }
    }

    void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }

    void Interation()
    {
        if(iDown && nearObject != null && !isJump && !isDodge && !isDead)
        {
            if(nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                Destroy(nearObject);
            }
            else if(nearObject.tag == "Shop")
            {
                Shop shop = nearObject.GetComponent<Shop>();
                shop.Enter(this);
                isShop = true;
            }
        }
    }

    void Swap()
    {
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;

        if(weaponIndex != -1)
        {
            if (weapons[weaponIndex].activeSelf==true) //(weaponindex)번째 무기가 활성화 되어있는가? 
                return; 
        }
        

        if((sDown1 || sDown2 || sDown3) && !isJump && !isDodge && hasWeapons[weaponIndex])
        {   
            if(equipWeapon != null)
                equipWeapon.gameObject.SetActive(false);
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);

            anim.SetTrigger("doSwap");

            isSwap = true;

            Invoke("SwapOut", 0.4f);
        }
    }

    void SwapOut()
    {
        isSwap = false;
    }

    void Attack()
    {
        if(equipWeapon == null)
            return;

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        if(fDown && isFireReady && !isDodge && !isSwap && !isShop)
        {
            equipWeapon.Use();
            anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee ?"doSwing":"doShot");
            fireDelay = 0;
        }
    }

    void Reload()
    {
        if(equipWeapon == null)
            return;
        
        if(equipWeapon.type == Weapon.Type.Melee)
            return;

        if(ammo == 0)
            return;
        
        if(rDown && !isJump && !isDodge && !isSwap && isFireReady && !isShop)
        {
            anim.SetTrigger("doReload");
            isReload = true;

            Invoke("ReloadOut", 3f);  
        }
    }

    void ReloadOut()
    {
        int reAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo;
        equipWeapon.curAmmo = reAmmo;
        ammo -= reAmmo;
        isReload = false;
    }

    void StopToWall()
    {
        isBorder=Physics.Raycast(transform.position, transform.forward, 5, LayerMask.GetMask("Wall"));
    }

    void FreezeRotation()
    {
        rigid.angularVelocity = Vector3.zero;
    }

    private void FixedUpdate() {
        FreezeRotation();
        StopToWall(); 
    }

    void Grenade()
    {
        if(hasGrenade == 0)
            return;
        if(gDown && !isReload && !isSwap)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if(Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 10;
                GameObject instantGrenade = Instantiate(grenadeObj, transform.position, transform.rotation);
                Rigidbody rigidGrenade = instantGrenade.GetComponent<Rigidbody>();
                rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
                rigidGrenade.AddTorque(Vector3.back * 10, ForceMode.Impulse);
            
                hasGrenade--;
                grenades[hasGrenade].SetActive(false);
            }
        }
    }

    void SkillQ()
    {
        if(qDown && target != null && !isQ)
        {
            StartCoroutine(CreateMissile());
            isQ=true;

            manager.inCoolDown();
            Invoke("CoolDownQ", 5f);
            
        }

        if(qDown && target != null && isQ)
        {
            Debug.Log("CoolDown!");
        }
        
    }

    void CoolDownQ()
    {
        isQ= false;
    }
    
    IEnumerator CreateMissile()
    {
        int _shotCount = m_shotCount;
        while(_shotCount > 0)
        {
            for(int i=0; i<m_shotCountEveryInterval;i++)
            {
                if(_shotCount>0)
                {
                    GameObject instantMissile =Instantiate(missile);
                    instantMissile.GetComponent<BezierMissile>().init(this.gameObject.transform, target);

                    _shotCount--;
                }
            }
            yield return new WaitForSeconds(m_interval);
        }
        yield return null;
    }

    void OnCollisionEnter(Collision other) 
    {
        if(other.gameObject.tag == "Floor")
        {
            anim.SetBool("isJump", false);
            isJump =false;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if(other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch(item.type)
            {
                case Item.Type.Ammo:
                    ammo += item.value;
                    if(ammo > maxAmmo)
                        ammo = maxAmmo;
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if(coin > maxCoin)
                        coin = maxCoin;
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if(health > maxHealth)
                        health = maxHealth;
                    break;
                case Item.Type.Grenade:
                    grenades[hasGrenade].SetActive(true);
                    hasGrenade += item.value;
                    if(hasGrenade > maxHasGrenade)
                        hasGrenade = maxHasGrenade;
                    break;
            }
            Destroy(other.gameObject);
        }
        else if(other.tag == "EnemyBullet")
        {
            if(!isDamage)
            {
                Bullet enemyBullet = other.GetComponent<Bullet>();
                health -= enemyBullet.damage;
                
                bool isBossAtk = (other.name == "BossMeleeArea");
                StartCoroutine(OnDmage(isBossAtk));
            } 
            if(other.GetComponent<Rigidbody>() != null)
                Destroy(other.gameObject);
        }
    }

    IEnumerator OnDmage(bool isBossAtk)
    {
        isDamage = true;
        foreach(MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.yellow;
        }
        if(isBossAtk)
            rigid.AddForce(transform.forward * -25, ForceMode.Impulse);
        yield return new WaitForSeconds(1f);
        isDamage = false;
        foreach(MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.white;
        }
        if(isBossAtk)
            rigid.velocity = Vector3.zero;
        if(health <= 0 && !isDead)
            OnDie();
    }

    void OnDie()
    {
        anim.SetTrigger("doDie");
        isDead = true;
        manager.GameOver();
    }

    private void OnTriggerStay(Collider other) {
        if(other.tag == "Weapon" || other.tag == "Shop")
        {
            nearObject = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other) {
        if(other.tag == "Weapon")
        {
            nearObject = null;
        }
        else if(other.tag == "Shop")
        {
            Shop shop = nearObject.GetComponent<Shop>();
            if(shop != null)
                shop.Exit();
            isShop=false;
            nearObject = null;
        }
    }
}
