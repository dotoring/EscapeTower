using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//클래스에 System.Serializable 이라는 어트리뷰트(Attribute)를 명시해야
//Inspector 뷰에 노출됨
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

    //이동 속도 변수
    public float moveSpeed = 10.0f;

    //회전 속도 변수
    public float rotSpeed = 100.0f;

    //인스펙터뷰에 표시할 애니메이션 클래스 변수
    public Anim anim;

    //아래에 있는 3D 모델의 Animation 컴포넌트에 접근하기 위한 변수
    public Animation _animation;

    //Player의 생명 변수
    public int hp = 100;
    //Player의 생명 초깃값
    private int initHp = 100;
    //Player의 Health bar 이미지
    public Image imgHpbar;

    CharacterController m_ChrCtrl;  //현재 캐릭터가 가지고 있는 캐릭터 컨트롤러 참조 변수

    public GameObject bloodEffect;  //혈흔 효과 프리팹
    FireCtrl m_FireCtrl = null;

    //--- 쉴드 스킬
    float m_SdDuration = 20.0f;
    float m_SdOnTime   = 0.0f;
    public GameObject ShieldObj = null;
    //--- 쉴드 스킬

    // Start is called before the first frame update
    void Start()
    {
        QualitySettings.vSyncCount = 0;  
        //모니터 주사율(플레임율)이 다른 컴퓨터일 경우 캐릭터 조작시 빠르게 움직일 수 있다.
        Application.targetFrameRate = 60;
        //실행 프레임 속도 60프레임으로 고정 시키기.. 코드

        moveSpeed = 7.0f;       //이동속도 초기화
        //생명 초깃값 설정
        //initHp = hp;

        //자신의 하위에 있는 Animation  컴포넌트를 찾아와 변수에 할당
        _animation = GetComponentInChildren<Animation>();

        //Animation 컴포넌트의 애니메이션 클립을 지정하고 실행
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

        //전후좌우 이동 방향 벡터 계산
        Vector3 moveDir = (Vector3.forward * v) + (Vector3.right * h);

        if (1.0f < moveDir.magnitude)
            moveDir.Normalize();

        ////Translate(이동 방향 * Time.deltaTime * 변위값 * 속도, 기준좌표)
        //transform.Translate(moveDir * Time.deltaTime * moveSpeed, Space.Self);

        if(m_ChrCtrl != null)
        {
            // 벡터를 로컬 좌표계 기준에서 월드 좌표계 기준으로 변환한다.
            moveDir = transform.TransformDirection(moveDir);

            // 캐릭터에 중력이 적용된는 이동함수
            m_ChrCtrl.SimpleMove(moveDir * moveSpeed);
        }

        //Translate(이동 방향 * 속도 * 변위값 * Time.deltaTime, 기준좌표)
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
            //Vector3.up 축을 기준으로 rotSpeed 만큼의 속도로 회전
            transform.Rotate(Vector3.up * Time.deltaTime * rotSpeed * Input.GetAxis("Mouse X") * a_AddRotSpeed);
        }

        //키보드 입력값을 기준으로 동작할 애니메이션 수행
        if(v >= 0.1f)
        {
            //전진 애니메이션
            _animation.CrossFade(anim.runForward.name, 0.3f);
        }
        else if(v <= -0.1f)
        {
            //후진 애니메이션
            _animation.CrossFade(anim.runBackward.name, 0.3f);
        }
        else if(h >= 0.1f)
        {
            //오른쪽 이동 애니메이션
            _animation.CrossFade(anim.runRight.name, 0.3f);
        }
        else if(h <= -0.1f)
        {
            //왼쪽 이동 애니메이션
            _animation.CrossFade(anim.runLeft.name, 0.3f);
        }
        else
        {
            //정지시 idle 애니메이션
            _animation.CrossFade(anim.idle.name, 0.3f);
        }

        SkillUpdate();

    }//void Update()

    //충돌한 Collider의 IsTrigger 옵션이 체크됐을 때 발생
    void OnTriggerEnter(Collider coll)
    {
        //충돌한 Collider가 몬스터의 PUNCH이면 Player의 HP 차감
        if(coll.gameObject.tag == "PUNCH")
        {
            if (0.0f < m_SdOnTime)  //쉴드 발동 중이면...
                return;

            if (hp <= 0.0f)     //이미 사망한 상태면...
                return;

            hp -= 10;
            //Debug.Log("Player HP = " + hp.ToString());

            //Image UI 항목의 fillAmount 속성을 조절해 생명 게이지 값 조절
            if (imgHpbar != null)
                imgHpbar.fillAmount = (float)hp / (float)initHp;

            //Player의 생명이 0이하이면 사망 처리
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
            //혈흔 효과 생성
            GameObject blood1 =
                        (GameObject)Instantiate(bloodEffect,
                                    coll.transform.position, Quaternion.identity);
            blood1.GetComponent<ParticleSystem>().Play();
            Destroy(blood1, 3.0f);
            //혈흔 효과 생성

            Destroy(coll.gameObject); //E_BULLET 삭제

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

    //Player의 사망 처리 루틴
    void PlayerDie()
    {
        Debug.Log("Player Die !!");

        //MONSTER는 Tag를 가진 모든 게임오브젝트를 찾아옴
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("MONSTER");

        //모든 몬스터의 OnPlayerDie 함수를 순차적으로 호출
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

        if (a_SkType == SkillType.Skill_0) //30% 힐링 아이템 스킬
        {
            hp += (int)(initHp * 0.3f);

            //머리위 텍스트 띄우기
            GameMgr.Inst.SpawnHealText((int)(initHp * 0.3f), transform.position, Color.white);

            if (initHp < hp)
                hp = initHp;

            if (imgHpbar != null)
                imgHpbar.fillAmount = hp / (float)initHp;

        }//if(a_SkType == SkillType.Skill_0) //30% 힐링 아이템 스킬
        else if (a_SkType == SkillType.Skill_1) //수류탄
        {
            if (m_FireCtrl != null)
                m_FireCtrl.FireGrenade();
        }
        else if(a_SkType == SkillType.Skill_2)  //보호막
        {
            if (0.0f < m_SdOnTime)
                return;

            m_SdOnTime = m_SdDuration;

            GameMgr.Inst.SkillTimeMethod(m_SdOnTime, m_SdDuration);
        }

    }//public void UseSkill_Item(SkillType a_SkType)

    void SkillUpdate()
    {
        //--- 쉴드 상태 업데이트
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
        //--- 쉴드 상태 업데이트
    }
}
