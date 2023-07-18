using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Ŭ������ System.Serializable �̶�� ��Ʈ����Ʈ(Attribute)�� ����ؾ�
//Inspector �信 �����
[System.Serializable]
public class Anim
{
    public AnimationClip idle;
    public AnimationClip runForward;
    public AnimationClip runBackward;
    public AnimationClip runRight;
    public AnimationClip runLeft;
}

public class PlayerCtrl : MonoBehaviour
{
    private float h = 0.0f;
    private float v = 0.0f;

    //�̵� �ӵ� ����
    public float moveSpeed = 10.0f;

    //ȸ�� �ӵ� ����
    public float rotSpeed = 100.0f;

    //�ν����ͺ信 ǥ���� �ִϸ��̼� Ŭ���� ����
    public Anim anim;

    //�Ʒ��� �ִ� 3D ���� Animation ������Ʈ�� �����ϱ� ���� ����
    public Animation _animation;

    //Player�� ���� ����
    public int hp = 100;
    //Player�� ���� �ʱ갪
    private int initHp = 100;
    //Player�� Health bar �̹���
    public Image imgHpbar;

    CharacterController m_ChrCtrl;  //���� ĳ���Ͱ� ������ �ִ� ĳ���� ��Ʈ�ѷ� ���� ����

    public GameObject bloodEffect;  //���� ȿ�� ������
    FireCtrl m_FireCtrl = null;

    //--- ���� ��ų
    float m_SdDuration = 20.0f;
    float m_SdOnTime   = 0.0f;
    public GameObject ShieldObj = null;
    //--- ���� ��ų

    // Start is called before the first frame update
    void Start()
    {
        QualitySettings.vSyncCount = 0;  
        //����� �ֻ���(�÷�����)�� �ٸ� ��ǻ���� ��� ĳ���� ���۽� ������ ������ �� �ִ�.
        Application.targetFrameRate = 60;
        //���� ������ �ӵ� 60���������� ���� ��Ű��.. �ڵ�

        moveSpeed = 7.0f;       //�̵��ӵ� �ʱ�ȭ
        //���� �ʱ갪 ����
        //initHp = hp;

        //�ڽ��� ������ �ִ� Animation  ������Ʈ�� ã�ƿ� ������ �Ҵ�
        _animation = GetComponentInChildren<Animation>();

        //Animation ������Ʈ�� �ִϸ��̼� Ŭ���� �����ϰ� ����
        _animation.clip = anim.idle;
        _animation.Play();

        m_ChrCtrl = GetComponent<CharacterController>();

        m_FireCtrl = GetComponent<FireCtrl>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameMgr.s_GameState == GameState.GameEnd)
            return;

        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        //�����¿� �̵� ���� ���� ���
        Vector3 moveDir = (Vector3.forward * v) + (Vector3.right * h);

        if (1.0f < moveDir.magnitude)
            moveDir.Normalize();

        ////Translate(�̵� ���� * Time.deltaTime * ������ * �ӵ�, ������ǥ)
        //transform.Translate(moveDir * Time.deltaTime * moveSpeed, Space.Self);

        if(m_ChrCtrl != null)
        {
            // ���͸� ���� ��ǥ�� ���ؿ��� ���� ��ǥ�� �������� ��ȯ�Ѵ�.
            moveDir = transform.TransformDirection(moveDir);

            // ĳ���Ϳ� �߷��� ����ȴ� �̵��Լ�
            m_ChrCtrl.SimpleMove(moveDir * moveSpeed);
        }

        //Translate(�̵� ���� * �ӵ� * ������ * Time.deltaTime, ������ǥ)
        //transform.Translate(Vector3.forward * moveSpeed * v * Time.deltaTime, Space.Self);
        //transform.Translate(transform.forward * moveSpeed * v * Time.deltaTime, Space.World);

        bool IsMsRot = false;
        float a_AddRotSpeed = 3.0f;
        if(FollowCam.m_CCMMode == CamCtrlMode.CCM_Default)
        {
            if (FollowCam.m_CCMDelay <= 0.0f)
                IsMsRot = true;
            else
            {
                if (Input.GetMouseButton(0) == true || Input.GetMouseButton(1) == true)
                    IsMsRot = true;
            }

            a_AddRotSpeed = 2.8f;
        }
        else //if(FollowCam.m_CCMMode == CamCtrlMode.CCM_WithMBtn)
        {
            if (Input.GetMouseButton(0) == true || Input.GetMouseButton(1) == true)
                IsMsRot = true;

        }//else if(FollowCam.m_CCMMode == CamCtrlMode.CCM_WithMBtn)

        //if (Input.GetMouseButton(0) == true || Input.GetMouseButton(1) == true)
        if (IsMsRot == true)
        {
            //Vector3.up ���� �������� rotSpeed ��ŭ�� �ӵ��� ȸ��
            transform.Rotate(Vector3.up * Time.deltaTime * rotSpeed * Input.GetAxis("Mouse X") * a_AddRotSpeed);
        }

        //Ű���� �Է°��� �������� ������ �ִϸ��̼� ����
        if(v >= 0.1f)
        {
            //���� �ִϸ��̼�
            _animation.CrossFade(anim.runForward.name, 0.3f);
        }
        else if(v <= -0.1f)
        {
            //���� �ִϸ��̼�
            _animation.CrossFade(anim.runBackward.name, 0.3f);
        }
        else if(h >= 0.1f)
        {
            //������ �̵� �ִϸ��̼�
            _animation.CrossFade(anim.runRight.name, 0.3f);
        }
        else if(h <= -0.1f)
        {
            //���� �̵� �ִϸ��̼�
            _animation.CrossFade(anim.runLeft.name, 0.3f);
        }
        else
        {
            //������ idle �ִϸ��̼�
            _animation.CrossFade(anim.idle.name, 0.3f);
        }

        SkillUpdate();

    }//void Update()

    //�浹�� Collider�� IsTrigger �ɼ��� üũ���� �� �߻�
    void OnTriggerEnter(Collider coll)
    {
        //�浹�� Collider�� ������ PUNCH�̸� Player�� HP ����
        if(coll.gameObject.tag == "PUNCH")
        {
            if (0.0f < m_SdOnTime)  //���� �ߵ� ���̸�...
                return;

            if (hp <= 0.0f)     //�̹� ����� ���¸�...
                return;

            hp -= 10;
            //Debug.Log("Player HP = " + hp.ToString());

            //Image UI �׸��� fillAmount �Ӽ��� ������ ���� ������ �� ����
            if (imgHpbar != null)
                imgHpbar.fillAmount = (float)hp / (float)initHp;

            //Player�� ������ 0�����̸� ��� ó��
            if(hp <= 0)
            {
                PlayerDie();
            }
        }
    }//void OnTriggerEnter(Collider coll)

    void OnCollisionEnter(Collision coll)
    {
        if(coll.gameObject.tag == "E_BULLET")
        {
            //���� ȿ�� ����
            GameObject blood1 =
                        (GameObject)Instantiate(bloodEffect,
                                    coll.transform.position, Quaternion.identity);
            blood1.GetComponent<ParticleSystem>().Play();
            Destroy(blood1, 3.0f);
            //���� ȿ�� ����

            Destroy(coll.gameObject); //E_BULLET ����

            if (hp <= 0.0f)
                return;

            hp -= 10;

            if (imgHpbar == null)
                imgHpbar = GameObject.Find("Hp_Image").GetComponent<Image>();

            if (imgHpbar != null)
                imgHpbar.fillAmount = (float)hp / (float)initHp;

            if(hp <= 0)
            {
                PlayerDie();
            }

        }//if(coll.gameObject.tag == "E_BULLET")
    }//void OnCollisionEnter(Collision coll)

    //Player�� ��� ó�� ��ƾ
    void PlayerDie()
    {
        Debug.Log("Player Die !!");

        //MONSTER�� Tag�� ���� ��� ���ӿ�����Ʈ�� ã�ƿ�
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("MONSTER");

        //��� ������ OnPlayerDie �Լ��� ���������� ȣ��
        foreach(GameObject monster in monsters)
        {
            //monster.GetComponent<MonsterCtrl>().OnPlayerDie();

            monster.SendMessage("OnPlayerDie", SendMessageOptions.DontRequireReceiver);
        }

        _animation.Stop();

        GameMgr.s_GameState = GameState.GameEnd;

    }//void PlayerDie()

    public void UseSkill_Item(SkillType a_SkType)
    {
        if (GameMgr.s_GameState == GameState.GameEnd)
            return;

        if (a_SkType == SkillType.Skill_0) //30% ���� ������ ��ų
        {
            hp += (int)(initHp * 0.3f);

            //�Ӹ��� �ؽ�Ʈ ����
            GameMgr.Inst.SpawnHealText((int)(initHp * 0.3f), transform.position, Color.white);

            if (initHp < hp)
                hp = initHp;

            if (imgHpbar != null)
                imgHpbar.fillAmount = hp / (float)initHp;

        }//if(a_SkType == SkillType.Skill_0) //30% ���� ������ ��ų
        else if (a_SkType == SkillType.Skill_1) //����ź
        {
            if (m_FireCtrl != null)
                m_FireCtrl.FireGrenade();
        }
        else if(a_SkType == SkillType.Skill_2)  //��ȣ��
        {
            if (0.0f < m_SdOnTime)
                return;

            m_SdOnTime = m_SdDuration;

            GameMgr.Inst.SkillTimeMethod(m_SdOnTime, m_SdDuration);
        }

    }//public void UseSkill_Item(SkillType a_SkType)

    void SkillUpdate()
    {
        //--- ���� ���� ������Ʈ
        if(0.0f < m_SdOnTime)
        {
            m_SdOnTime -= Time.deltaTime;
            if (ShieldObj != null && ShieldObj.activeSelf == false)
                ShieldObj.SetActive(true);
        }
        else
        {
            if (ShieldObj != null && ShieldObj.activeSelf == true)
                ShieldObj.SetActive(false);
        }
        //--- ���� ���� ������Ʈ
    }
}
