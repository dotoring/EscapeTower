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

    //Text UI �׸� ������ ���� ����
    public Text txtScore;
    //���� ������ ����ϱ� ���� ����
    private int totScore = 0;
    int m_CurScore = 0;     //�̹� ������������ ���� ���� ����

    public Button BackBtn;

    [Header("------ Monster Spawn ------")]
    //���Ͱ� ������ ��ġ�� ���� �迭
    public Transform[] points;
    //���� �������� �Ҵ��� ����
    public GameObject monsterPrefab;
    //���� �̸� ������ ������ ����Ʈ �ڷ���
    public List<GameObject> monsterPool = new List<GameObject>();

    //���͸� �߻���ų �ֱ�
    public float createTime = 2.0f;
    //������ �ִ� �߻� ����
    public int maxMonster = 10;
    //���� ���� ���� ����
    public bool isGameOver = false;

    PlayerCtrl m_RefHero = null;

    //--- �Ӹ� ���� ���ؽ�Ʈ ����� ���� ����
    [Header("------- HealText -------")]
    public Transform m_Heal_Canvas = null;
    public GameObject m_HTextPrefab = null;
    //--- �Ӹ� ���� ���ؽ�Ʈ ����� ���� ����

    [Header("------- Skill Cool Timer -------")]
    public GameObject m_SkCoolPrefab = null;
    public Transform m_SkCoolRoot = null;
    public SkInvenNode[] m_SkInvenNode;     //Skill �κ��丮 ��ư ���� ����

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

    //�̱��� ������ ���� �ν��Ͻ� ���� ����
    public static GameMgr Inst = null;

    void Awake()
    {
        //GameMgr Ŭ������ �ν��Ͻ��� ����
        Inst = this;    
    }
    //�̱��� ������ ���� �ν��Ͻ� ���� ����

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1.0f;      //���� �ӵ���...
        s_GameState = GameState.GameIng;

        GlobalValue.LoadGameData();
        RefreshGameUI();

        DispScore(0);

        BackBtn.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("scLobby");
        });

        //--- Monster Spawn
        //Hierarchy ���� SpawnPoint�� ã�� ������ �ִ� ��� Transform ������Ʈ�� ã�ƿ�
        points = GameObject.Find("SpawnPoint").GetComponentsInChildren<Transform>();

        //���͸� ������ ������Ʈ Ǯ�� ����
        for(int i = 0; i < maxMonster; i++)
        {
            //���� �������� ����
            GameObject monster = (GameObject)Instantiate(monsterPrefab);
            //������ ������ �̸� ����
            monster.name = "Monster_" + i.ToString();
            //������ ���͸� ��Ȱ��ȭ
            monster.SetActive(false);
            //������ ���͸� ������Ʈ Ǯ�� �߰�
            monsterPool.Add(monster);
        }

        if(points.Length > 0)
        {
            //���� ���� �ڷ�ƾ �Լ� ȣ��
            StartCoroutine(this.CreateMonster());
        }
        //--- Monster Spawn

        //���ӿ��� ��ư ó�� �ڵ�
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
        //���콺 �߾ӹ�ư(�� Ŭ��)
        if(Input.GetMouseButtonDown(2))
        {
            UseSkill_Key(SkillType.Skill_1); //����ź ���
        }

        //--- ����Ű �̿����� ��ų ����ϱ�...
        if(Input.GetKeyDown(KeyCode.Alpha1) ||  //����Ű 1
            Input.GetKeyDown(KeyCode.Keypad1))
        {
            UseSkill_Key(SkillType.Skill_0);    //30% ���� ������ ��ų
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) ||  //����Ű 2
                Input.GetKeyDown(KeyCode.Keypad2))
        {
            UseSkill_Key(SkillType.Skill_1);    //����ź ���
        }
        else if(Input.GetKeyDown(KeyCode.Alpha3) ||   //����Ű 3 
                Input.GetKeyDown(KeyCode.Keypad3))
        {
            UseSkill_Key(SkillType.Skill_2);    //��ȣ��
        }
        //--- ����Ű �̿����� ��ų ����ϱ�...
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

    //���� ���� �� ȭ�� ǥ��
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

    //���� ���� �ڷ�ƾ �Լ�
    IEnumerator CreateMonster()
    {
        //���� ���� �ñ��� ���� ����
        while(!isGameOver)
        {
            //���� ���� �ֱ� �ð���ŭ ���� ������ �纸
            yield return new WaitForSeconds(createTime);

            //�÷��̾ ������� �� �ڷ�ƾ�� ������ ���� ��ƾ�� �������� ����
            if (GameMgr.s_GameState == GameState.GameEnd) 
                yield break; //<-- �ڷ�ƾ �Լ��� ��� ���������� �ڵ�

            //������Ʈ Ǯ�� ó������ ������ ��ȸ
            foreach(GameObject monster in monsterPool)
            {
                //��Ȱ��ȭ ���η� ��� ������ ���͸� �Ǵ�
                if(!monster.activeSelf)
                {
                    //���͸� ������ų ��ġ�� �ε������� ����
                    int idx = Random.Range(1, points.Length);
                    //������ ������ġ�� ����
                    monster.transform.position = points[idx].position;
                    //���͸� Ȱ��ȭ��
                    monster.SetActive(true);
                    //������Ʈ Ǯ���� ���� ������ �ϳ��� Ȱ��ȭ�� �� for ������ ��������
                    break;
                }
            }//foreach(GameObject monster in monsterPool)

        }//while(!isGameOver)




        ////���� ���� �ñ��� ���� ����
        //while(!isGameOver)
        //{
        //    //���� ������ ���� ���� ����
        //    int monsterCount = (int)GameObject.FindGameObjectsWithTag("MONSTER").Length;

        //    //������ �ִ� ���� �������� ���� ���� ���� ����
        //    if(monsterCount < maxMonster)
        //    {
        //        //������ ���� �ֱ� �ð���ŭ ���
        //        yield return new WaitForSeconds(createTime);

        //        //�ұ�Ģ���� ��ġ ����
        //        int idx = Random.Range(1, points.Length);
        //        //������ ���� ����
        //        Instantiate(monsterPrefab, points[idx].position, points[idx].rotation);
        //    }
        //    else
        //    {
        //        yield return null; //�� �÷����� ���� ���� ���
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
            "ȹ�� ����\n" + m_CurScore + "\n\n" + "ȹ�� ���\n" + m_CurGold;
    }
}
