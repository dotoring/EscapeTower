using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterCtrl : MonoBehaviour
{
    //������ ���� ������ �ִ� Enumerable ���� ����
    public enum MonsterState { idle, trace, attack, die };
    //������ ���� ���� ������ ������ Enum ����
    public MonsterState monsterState = MonsterState.idle;

    private Transform playerTr;
    //private NavMeshAgent nvAgent;
    private Animator animator;

    //���� �����Ÿ�
    public float traceDist = 10.0f;
    //���� �����Ÿ�
    public float attackDist = 2.0f;

    //������ ��� ����
    private bool isDie = false;

    //���� ȿ�� ������
    public GameObject bloodEffect;
    //���� ��Į ȿ�� ������
    public GameObject bloodDecal;

    //���� ���� ����
    private int hp = 100;
    Rigidbody m_Rigid = null;

    //--- �Ѿ� �߻� ���� ����
    public GameObject bullet;  //�Ѿ� ������
    float m_BLTime = 0.0f;
    LayerMask m_LaserMask = -1;
    //--- �Ѿ� �߻� ���� ����

    // Start is called before the first frame update
    void Awake()
    {
        traceDist = 10.0f;
        attackDist = 1.6f;

        //���� ����� Player�� Transform �Ҵ�
        playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();
        //NavMeshAgent ������Ʈ �Ҵ�
        //nvAgent = this.gameObject.GetComponent<NavMeshAgent>();

        //���� ����� ��ġ�� �����ϸ� �ٷ� ���� ����
        //nvAgent.destination = playerTr.position;

        //Animator ������Ʈ �Ҵ�
        animator = this.gameObject.GetComponent<Animator>();

        this.m_Rigid = GetComponent<Rigidbody>();
    }

    //void OnEnable() //��ũ��Ʈ �Ǵ� ���ӿ�����Ʈ�� ��Ȱ��ȭ�� ���¿��� �ٽ� Ȱ��ȭ�� ������ �߻��ϴ� �ݹ� �Լ���.
    //{
    //    //������ �������� ������ �ൿ ���¸� üũ�ϴ� �ڷ�ƾ �Լ� ����
    //    StartCoroutine(this.CheckMonsterState());

    //    //������ ���¿� ���� �����ϴ� ��ƾ�� �����ϴ� �ڷ�ƾ �Լ� ����
    //    StartCoroutine(this.MonsterAction());
    //}

    void Start()
    {
        m_LaserMask = 1 << LayerMask.NameToLayer("Default");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (GameMgr.s_GameState == GameState.GameEnd)
            return;

        if (playerTr.gameObject.activeSelf == false)
            playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();

        CheckMonStateUpdate();
        MonActionUpdate();

        if (isDie == false)
            this.m_Rigid.AddForce(Vector3.down * 100.0f);  //�߷°� ������ �ֱ�
    }

    float m_AI_Delay = 0.0f;

    //������ �������� ������ �ൿ ���¸� üũ�ϰ� monsterState �� ����
    void CheckMonStateUpdate()
    {
        if (isDie == true)
            return;

        //0.1�� �ֱ�θ� üũ�ϱ� ���� ������ ��� �κ�
        m_AI_Delay -= Time.deltaTime;
        if (0.0f < m_AI_Delay)
            return;

        m_AI_Delay = 0.1f;
        //0.1�� �ֱ�θ� üũ�ϱ� ���� ������ ��� �κ�

        //���Ϳ� �÷��̾� ������ �Ÿ� ����
        float dist = Vector3.Distance(playerTr.position, transform.position);

        if (dist <= attackDist) //���ݰŸ� ���� �̳��� ���Դ��� Ȯ��
        {
            monsterState = MonsterState.attack;
        }
        else if (dist <= traceDist) //�����Ÿ� ���� �̳��� ���Դ��� Ȯ��
        {
            monsterState = MonsterState.trace;  //������ ���¸� �������� ����
        }
        else
        {
            monsterState = MonsterState.idle;   //������ ���¸� idle ���� ����
        }

    }

    //������ ���°��� ���� ������ ������ �����ϴ� �Լ�
    void MonActionUpdate()
    {
        if (isDie == true)
            return;

        switch (monsterState)
        {
            //idle ����
            case MonsterState.idle:
                //Animator�� IsTrace ������ false�� ����
                animator.SetBool("IsTrace", false);
                break;

            //���� ����
            case MonsterState.trace:
                {
                    //--- �̵�����
                    float a_MoveVelocity = 2.0f;    //��� �ʴ� �̵� �ӵ�...
                    Vector3 a_MoveDir = Vector3.zero;
                    a_MoveDir = playerTr.position - this.transform.position;
                    a_MoveDir.y = 0.0f;

                    Vector3 a_StepVec = a_MoveDir.normalized * Time.deltaTime * a_MoveVelocity;
                    transform.Translate(a_StepVec, Space.World);

                    //--- �̵� ������ �ٶ� ������ ȸ�� ó��
                    if(0.0f < a_MoveDir.magnitude)
                    {
                        float a_RotSpeed = 7.0f;    //�ʴ� ȸ�� �ӵ�
                        Quaternion a_TargetRot = Quaternion.LookRotation(a_MoveDir.normalized);
                        transform.rotation = Quaternion.Slerp(transform.rotation,
                                                a_TargetRot, Time.deltaTime * a_RotSpeed);
                    }
                    //--- �̵�����

                    //Animator�� IsAttack ������ false�� ����
                    animator.SetBool("IsAttack", false);
                    //Animator�� IsTrace ������ true ����
                    animator.SetBool("IsTrace", true);
                }
                break;

            //���� ����
            case MonsterState.attack:
                {
                    //IsAttack�� true�� ������ attack State�� ����
                    animator.SetBool("IsAttack", true);

                    //���Ͱ� ���ΰ��� �����ϸ鼭 �ٶ� ������ �ؾ� �Ѵ�.
                    float m_RotSpeed = 6.0f;        //�ʴ� ȸ�� �ӵ�
                    Vector3 a_CacVDir = playerTr.position - transform.position;
                    a_CacVDir.y = 0.0f;
                    if (0.0f < a_CacVDir.magnitude)
                    {
                        Quaternion a_TargetRot =
                                        Quaternion.LookRotation(a_CacVDir.normalized);
                        transform.rotation = Quaternion.Slerp(transform.rotation,
                                                   a_TargetRot, Time.deltaTime * m_RotSpeed);
                    }
                    //���Ͱ� ���ΰ��� �����ϸ鼭 �ٶ� ������ �ؾ� �Ѵ�.
                }
                break;
        }//switch(monsterState)

        //--- �Ѿ� �߻�
        FireUpdate();
        //--- �Ѿ� �߻�

    }//void MonActionUpdate()

    //Bullet�� �浹 üũ
    void OnCollisionEnter(Collision coll)
    {
        if(coll.gameObject.tag == "BULLET")
        {
            //���� ȿ�� �Լ� ȣ��
            CreateBloodEffect(coll.transform.position);

            //���� �Ѿ��� Damage�� ������ ���� hp ����
            hp -= coll.gameObject.GetComponent<BulletCtrl>().damage;
            if(hp <= 0)
            {
                MonsterDie();
            }

            //Bullet ����
            Destroy(coll.gameObject);

            //IsHit Trigger�� �߻���Ű�� Any State���� gothit�� ���̵�
            animator.SetTrigger("IsHit");
        }
    }//void OnCollisionEnter(Collision coll)

    //���� ��� �� ó�� ��ƾ
    void MonsterDie()
    {
        //����� ������ �±׸� Untagged �� ����
        gameObject.tag = "Untagged";

        //��� �ڷ�ƾ�� ����
        StopAllCoroutines();

        isDie = true;
        monsterState = MonsterState.die;
        //nvAgent.isStopped = true;
        animator.SetTrigger("IsDie");

        m_Rigid.useGravity = false;

        //���Ϳ� �߰��� Collider�� ��Ȱ��ȭ
        gameObject.GetComponentInChildren<CapsuleCollider>().enabled = false;

        foreach (Collider coll in gameObject.GetComponentsInChildren<SphereCollider>())
        {
            coll.enabled = false;
        }

        //GameMgr�� ���ھ� ������ ���ھ� ǥ�� �Լ� ȣ��
        GameMgr.Inst.DispScore(1); 

        //���� ������Ʈ Ǯ�� ȯ����Ű�� �ڷ�ƾ �Լ� ȣ��
        StartCoroutine(this.PushObjectPool());

        //���� ����� ������ ���
        if(GameMgr.Inst.m_CoinItem != null)
        {
            GameObject a_CoinObj = Instantiate(GameMgr.Inst.m_CoinItem) as GameObject;
            a_CoinObj.transform.position = this.transform.position;
            Destroy(a_CoinObj, 10.0f);
        }

    }//void MonsterDie()

    IEnumerator PushObjectPool()
    {
        yield return new WaitForSeconds(3.0f);

        //���� ���� �ʱ�ȭ
        isDie = false;
        hp = 100;
        gameObject.tag = "MONSTER";
        monsterState = MonsterState.idle;

        m_Rigid.useGravity = true;

        //���Ϳ� �߰��� Collider�� �ٽ� Ȱ��ȭ
        gameObject.GetComponentInChildren<CapsuleCollider>().enabled = true;

        foreach(Collider coll in gameObject.GetComponentsInChildren<SphereCollider>())
        {
            coll.enabled = true;
        }

        //���͸� ��Ȱ��ȭ
        gameObject.SetActive(false);
    }

    void CreateBloodEffect(Vector3 pos)
    {
        //���� ȿ�� ����
        GameObject blood1 = (GameObject)Instantiate(bloodEffect, pos, Quaternion.identity);
        blood1.GetComponent<ParticleSystem>().Play();
        Destroy(blood1, 3.0f);

        //��Į ���� ��ġ - �ٴڿ��� ���� �ø� ��ġ ����
        Vector3 decalPos = transform.position + (Vector3.up * 0.05f);
        //��Į�� ȸ������ �������� ����
        Quaternion decalRot = Quaternion.Euler(90, 0, Random.Range(0, 360));

        //��Į ������ ����
        GameObject blood2 = (GameObject)Instantiate(bloodDecal, decalPos, decalRot);
        //��Į�� ũ�⵵ �ұ�Ģ������ ��Ÿ���Բ� ������ ����
        float scale = Random.Range(1.5f, 3.5f);
        blood2.transform.localScale = Vector3.one * scale;

        //5�� �Ŀ� ����ȿ�� �������� ����
        Destroy(blood2, 5.0f);
    }

    //void OnTriggerEnter(Collider coll)
    //{
    //    Debug.Log(coll.gameObject.tag);
    //}

    //�÷��̾ ������� �� ����Ǵ� �Լ�
    void OnPlayerDie()
    {
        //������ ���¸� üũ�ϴ� �ڷ�ƾ �Լ����� ��� ������Ŵ
        StopAllCoroutines();
        //������ �����ϰ� �ִϸ��̼��� ����
        //nvAgent.isStopped = true;
        animator.SetTrigger("IsPlayerDie");
    }

    void FireUpdate() //�ֱ������� �Ѿ� �߻��ϴ� �Լ�
    {
        Vector3 a_PlayerPos = playerTr.position;
        a_PlayerPos.y = a_PlayerPos.y + 1.5f;
        Vector3 a_MonPos = transform.position;
        a_MonPos.y = a_MonPos.y + 1.5f;
        Vector3 a_CacDir = a_PlayerPos - a_MonPos;
        float a_RayUDLimit = 3.0f;

        bool isRayOk = false;
        if(Physics.Raycast(a_MonPos, a_CacDir.normalized, 
                            out RaycastHit hit, 100.0f, m_LaserMask) == true)
        {
            if(hit.collider.gameObject.tag == "Player")
            {
                isRayOk = true;
            }
        }

        m_BLTime = m_BLTime - Time.deltaTime;
        if(m_BLTime <= 0.0f)
        {
            m_BLTime = 0.0f;
        }

        if(isRayOk == true && traceDist < a_CacDir.magnitude)
        {
            if(-a_RayUDLimit <= a_CacDir.y && a_CacDir.y <= a_RayUDLimit)
            { //���̰� �Ѱ�ġ ������ ���� ��, �Ʒ� -3 ~ +3m ���̷� ���� �ɾ���

                //-- ���Ͱ� ���ΰ��� ���� �ٶ� ������ ȸ�� ó��
                Vector3 a_CacVLen = playerTr.position - transform.position;
                a_CacVLen.y = 0.0f;
                Quaternion a_TargetRot =
                            Quaternion.LookRotation(a_CacVLen.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                        a_TargetRot, Time.deltaTime * 6.0f);
                //-- ���Ͱ� ���ΰ��� ���� �ٶ� ������ ȸ�� ó��

                if(m_BLTime <= 0.0f)
                {
                    Vector3 a_StartPos = a_MonPos + a_CacDir.normalized * 1.5f;
                    GameObject a_Bullet = Instantiate(bullet, a_StartPos, Quaternion.identity);
                    a_Bullet.layer = LayerMask.NameToLayer("E_BULLET");
                    a_Bullet.tag = "E_BULLET";
                    a_Bullet.transform.forward = a_CacDir.normalized;

                    m_BLTime = 2.0f;
                }//if(m_BLTime <= 0.0f)

            }//if(-a_RayUDLimit <= a_CacDir.y && a_CacDir.y <= a_RayUDLimit)
        }//if(isRayOk == true && traceDist < a_CacDir.magnitude)

    }//void FireUpdate() //�ֱ������� �Ѿ� �߻��ϴ� �Լ�

    public void TakeDamage(int a_Value)
    {
        if (hp <= 0.0f)  //�̷��� �ϸ� ��� ó���� �ѹ��� �� ����
            return;

        //���� ȿ�� �Լ� ȣ��
        CreateBloodEffect(transform.position);

        hp -= a_Value;
        if(hp <= 0)
        {
            hp = 0;
            MonsterDie();
        }

        animator.SetTrigger("IsHit");
    }

}//public class MonsterCtrl : MonoBehaviour
