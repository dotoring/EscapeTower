using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameState
{
    GameIng,
    GameEnd
}

public class GameMgr : MonoBehaviour
{
    public static GameState s_GameState = GameState.GameIng;

    //Text UI 항목 연결을 위한 변수
    public Text txtScore;
    //누적 점수를 기록하기 위한 변수
    private int totScore = 0;
    int m_CurScore = 0;     //이번 스테이지에서 얻은 게임 점수

    public Button BackBtn;

    [Header("------ Monster Spawn ------")]
    //몬스터가 출현할 위치를 담을 배열
    public Transform[] points;
    //몬스터 프리팹을 할당할 변수
    public GameObject monsterPrefab;
    //몬스터 미리 생성해 저장할 리스트 자료형
    public List<GameObject> monsterPool = new List<GameObject>();

    //몬스터를 발생시킬 주기
    public float createTime = 2.0f;
    //몬스터의 최대 발생 개수
    public int maxMonster = 10;
    //게임 종료 여부 변수
    public bool isGameOver = false;

    PlayerCtrl m_RefHero = null;

    //--- 머리 위에 힐텍스트 띄우기용 변수 선언
    [Header("------- HealText -------")]
    public Transform m_Heal_Canvas = null;
    public GameObject m_HTextPrefab = null;
    //--- 머리 위에 힐텍스트 띄우기용 변수 선언

    [Header("------- Skill Cool Timer -------")]
    public GameObject m_SkCoolPrefab = null;
    public Transform m_SkCoolRoot = null;
    public SkInvenNode[] m_SkInvenNode;     //Skill 인벤토리 버튼 연결 변수

    [HideInInspector] public GameObject m_CoinItem = null;
    [Header("------ Gold UI ------")]
    public Text m_UserGoldText = null;
    int m_CurGold = 0;

    [Header("------ GameOverPanel ------")]
    public GameObject ResultPanel = null;
    public Text Title_txt = null;
    public Text Result_txt = null;
    public Button Replay_Btn = null;
    public Button RstLobby_Btn = null;

    //싱글턴 패턴을 위한 인스턴스 변수 선언
    public static GameMgr Inst = null;

    void Awake()
    {
        //GameMgr 클래스를 인스턴스에 대입
        Inst = this;    
    }
    //싱글턴 패턴을 위한 인스턴스 변수 선언

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1.0f;      //원래 속도로...
        s_GameState = GameState.GameIng;

        GlobalValue.LoadGameData();
        RefreshGameUI();

        DispScore(0);

        BackBtn.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("scLobby");
        });

        //--- Monster Spawn
        //Hierarchy 뷰의 SpawnPoint를 찾아 하위에 있는 모든 Transform 컴포넌트를 찾아옴
        points = GameObject.Find("SpawnPoint").GetComponentsInChildren<Transform>();

        //몬스터를 생성해 오브젝트 풀에 저장
        for(int i = 0; i < maxMonster; i++)
        {
            //몬스터 프리팹을 생성
            GameObject monster = (GameObject)Instantiate(monsterPrefab);
            //생성한 몬스터의 이름 설정
            monster.name = "Monster_" + i.ToString();
            //생성한 몬스터를 비활성화
            monster.SetActive(false);
            //생성한 몬스터를 오브젝트 풀에 추가
            monsterPool.Add(monster);
        }

        if(points.Length > 0)
        {
            //몬스터 생성 코루틴 함수 호출
            StartCoroutine(this.CreateMonster());
        }
        //--- Monster Spawn

        //게임오버 버튼 처리 코드
        if(RstLobby_Btn != null)
        {
            RstLobby_Btn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("scLobby");
            });
        }

        if (Replay_Btn != null)
        {
            Replay_Btn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("scLevel01");
                SceneManager.LoadScene("scPlay", LoadSceneMode.Additive);
            });
        }

        m_RefHero = GameObject.FindObjectOfType<PlayerCtrl>();

        m_CoinItem = Resources.Load("CoinItem/CoinPrefab") as GameObject;
    }//void Start()

    // Update is called once per frame
    void Update()
    {
        //마우스 중앙버튼(휠 클릭)
        if(Input.GetMouseButtonDown(2))
        {
            UseSkill_Key(SkillType.Skill_1); //수류탄 사용
        }

        //--- 단축키 이용으로 스킬 사용하기...
        if(Input.GetKeyDown(KeyCode.Alpha1) ||  //단축키 1
            Input.GetKeyDown(KeyCode.Keypad1))
        {
            UseSkill_Key(SkillType.Skill_0);    //30% 힐링 아이템 스킬
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) ||  //단축키 2
                Input.GetKeyDown(KeyCode.Keypad2))
        {
            UseSkill_Key(SkillType.Skill_1);    //수류탄 사용
        }
        else if(Input.GetKeyDown(KeyCode.Alpha3) ||   //단축키 3 
                Input.GetKeyDown(KeyCode.Keypad3))
        {
            UseSkill_Key(SkillType.Skill_2);    //보호막
        }
        //--- 단축키 이용으로 스킬 사용하기...
    }

    public void UseSkill_Key(SkillType a_SkType)
    {
        if (GlobalValue.g_SkillCount[(int)a_SkType] <= 0)
        {
            return;
        }

        if (m_RefHero != null)
            m_RefHero.UseSkill_Item(a_SkType);

        if ((int)a_SkType < m_SkInvenNode.Length)
        {
            m_SkInvenNode[(int)a_SkType].m_SkCountText.text =
                        GlobalValue.g_SkillCount[(int)a_SkType].ToString();
        }

    }

    //점수 누적 및 화면 표시
    public void DispScore(int score)
    {
        //totScore += score;
        //txtScore.text = "score <color=#ff0000>" + totScore.ToString() + "</color>";

        m_CurScore += score;
        if(m_CurScore < 0)
        {
            m_CurScore = 0;
        }

        GlobalValue.g_BestScore += score;

        if(GlobalValue.g_BestScore < 0)
        {
            GlobalValue.g_BestScore = 0;
        }

        int a_MaxValue = int.MaxValue - 10;
        if(a_MaxValue < GlobalValue.g_BestScore)
        {
            GlobalValue.g_BestScore = a_MaxValue;
        }

        txtScore.text = "SCORE <color=#ff0000>" + m_CurScore.ToString() + 
            "</color> / BEST <color=#ff0000>" + GlobalValue.g_BestScore.ToString() + "</color>";

        PlayerPrefs.SetInt("BestScore", GlobalValue.g_BestScore);
    }

    public void AddGold(int value = 10)
    {
        m_CurGold += value;
        if(m_CurGold < 0)
        {
            m_CurGold = 0;
        }

        GlobalValue.g_UserGold += value;

        if(GlobalValue.g_UserGold < 0)
        {
            GlobalValue.g_UserGold = 0;
        }

        int a_MaxValue = int.MaxValue - 10;
        if(a_MaxValue < GlobalValue.g_UserGold)
        {
            GlobalValue.g_UserGold = a_MaxValue;
        }

        if(m_UserGoldText != null)
        {
            m_UserGoldText.text = "Gold <color=#ffff00>" + GlobalValue.g_UserGold + "</color>";
        }

        PlayerPrefs.SetInt("UserGold", GlobalValue.g_UserGold);
    }

    //몬스터 생성 코루틴 함수
    IEnumerator CreateMonster()
    {
        //게임 종료 시까지 무한 루프
        while(!isGameOver)
        {
            //몬스터 생성 주기 시간만큼 메인 루프에 양보
            yield return new WaitForSeconds(createTime);

            //플레이어가 사망했을 대 코루틴을 종료해 다음 루틴을 진행하지 않음
            if (GameMgr.s_GameState == GameState.GameEnd) 
                yield break; //<-- 코루틴 함수를 즉시 빠져나가는 코드

            //오브젝트 풀이 처음부터 끝까지 순회
            foreach(GameObject monster in monsterPool)
            {
                //비활성화 여부로 사용 가능한 몬스터를 판단
                if(!monster.activeSelf)
                {
                    //몬스터를 출현시킬 위치의 인덱스값을 추출
                    int idx = Random.Range(1, points.Length);
                    //몬스터의 출현위치를 설정
                    monster.transform.position = points[idx].position;
                    //몬스터를 활성화함
                    monster.SetActive(true);
                    //오브젝트 풀에서 몬스터 프리팹 하나를 활성화한 후 for 루프를 빠져나감
                    break;
                }
            }//foreach(GameObject monster in monsterPool)

        }//while(!isGameOver)




        ////게임 종료 시까지 무한 루프
        //while(!isGameOver)
        //{
        //    //현재 생성된 몬스터 개수 산출
        //    int monsterCount = (int)GameObject.FindGameObjectsWithTag("MONSTER").Length;

        //    //몬스터의 최대 생성 개수보다 적을 때만 몬스터 생성
        //    if(monsterCount < maxMonster)
        //    {
        //        //몬스터의 생성 주기 시간만큼 대기
        //        yield return new WaitForSeconds(createTime);

        //        //불규칙적인 위치 산출
        //        int idx = Random.Range(1, points.Length);
        //        //몬스터의 동적 생성
        //        Instantiate(monsterPrefab, points[idx].position, points[idx].rotation);
        //    }
        //    else
        //    {
        //        yield return null; //한 플레임이 도는 동안 대기
        //    }
        //}//while(!isGameOver)
    }//IEnumerator CreateMonster()

    public void SpawnHealText(int cont, Vector3 a_WSpawnPos, Color a_Color)
    {
        if (m_Heal_Canvas == null || m_HTextPrefab == null)
            return;

        GameObject a_HealObj = Instantiate(m_HTextPrefab) as GameObject;
        HealTextCtrl a_HealText = a_HealObj.GetComponent<HealTextCtrl>();
        if (a_HealText != null)
            a_HealText.InitState(cont, a_WSpawnPos, m_Heal_Canvas, a_Color);
    }

    public void SkillTimeMethod(float a_Time, float a_Dur)
    {
        GameObject obj = Instantiate(m_SkCoolPrefab) as GameObject;
        obj.transform.SetParent(m_SkCoolRoot, false);
        SkCool_NodeCtrl skNode = obj.GetComponent<SkCool_NodeCtrl>();
        skNode.InitState(a_Time, a_Dur);
    }

    void RefreshGameUI()
    {
        if (m_UserGoldText != null)
        {
            m_UserGoldText.text = "Gold <color=#ffff00>" + GlobalValue.g_UserGold + "</color>";
        }

        for (int i = 0; i < GlobalValue.g_SkillCount.Length; i++)
        {
            if(m_SkInvenNode.Length <= i)
            {
                continue;
            }

            m_SkInvenNode[i].m_SkType = (SkillType)i;
            m_SkInvenNode[i].m_SkCountText.text = GlobalValue.g_SkillCount[i].ToString();
        }
    }

    public void GameOverFunc()
    {
        ResultPanel.SetActive(true);

        Result_txt.text = "NickName\n" + GlobalValue.g_NickName + "\n\n" +
            "획득 점수\n" + m_CurScore + "\n\n" + "획득 골드\n" + m_CurGold;
    }
}
