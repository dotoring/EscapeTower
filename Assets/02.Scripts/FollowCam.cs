using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CamCtrlMode
{
    CCM_Default,
    CCM_WithMBtn
}

public class SideWall
{
    public bool m_IsColl = false;
    public GameObject m_SideWalls = null;
    public Material m_WallMaterial = null;

    public SideWall() //������ �Լ�
    {
        m_IsColl = false;
        m_SideWalls = null;
        m_WallMaterial = null;
    }
}

public class FollowCam : MonoBehaviour
{
    //--- ĳ���� ���� ���� ����
    public GameObject[] CharObjects;   //ĳ���� 2���� ������Ʈ ���� �迭 ����
    int CharType = 0;
    //--- ĳ���� ���� ���� ����

    public Transform targetTr;      //������ Ÿ�� ���ӿ�����Ʈ�� Transform ����
    public float dist = 10.0f;      //ī�޶���� ���� �Ÿ�
    public float height = 3.0f;     //ī�޶��� ���� ����
    public float dampTrace = 20.0f; //�ε巯�� ������ ���� ����

    Vector3 m_PlayerVec = Vector3.zero;
    float rotSpeed = 10.0f;

    //---- Side Wall ����Ʈ ���� ����
    Vector3 m_CacVLen = Vector3.zero;
    Vector3 m_CacDirVec = Vector3.zero;

    GameObject[] m_SideWalls = null;
    LayerMask m_WallLyMask = -1;
    List<SideWall> m_SW_List = new List<SideWall>();
    //---- Side Wall ����Ʈ ���� ����

    //--- ī�޶� ��ġ ���� ����
    float m_RotV = 0.0f;        //���콺 ���� ���۰� ���� ����
    float m_DefaltRotV = 25.2f; //���� ������ ȸ�� ����
    float m_MarginRotV = 22.3f; //�ѱ����� ���� ����
    float m_MinLimitV = -17.9f; //�� �Ʒ� ���� ����
    float m_MaxLimitV = 52.9f;  //�� �Ʒ� ���� ����
    float m_MaxDist = 4.0f;     //���콺 �� �ƿ� �ִ� �Ÿ� ���Ѱ�
    float m_MinDist = 2.0f;     //���콺 �� �� �ִ� �Ÿ� ���Ѱ�
    float m_ZoomSpeed = 0.7f;   //���콺 �� ���ۿ� ���� �� �� �ƿ� ���ǵ� ������

    Quaternion m_BuffRot;       //ī�޶� ȸ�� ���� ����
    Vector3 m_BuffPos;       //ī�޶� ȸ���� ���� ��ġ ��ǥ ���� ����
    Vector3 m_BasicPos = Vector3.zero;  //��ġ ���� ����
    //--- ī�޶� ��ġ ���� ����

    //--- �� ���� ���� ���� ����
    public static Vector3 m_RifleDir = Vector3.zero;        //�� ���� ����
    Quaternion m_RFCacRot;
    Vector3 m_RFCacPos = Vector3.forward;
    //--- �� ���� ���� ���� ����

    //--- ī�޶� ��Ʈ�� ��� ���� ����
    public static CamCtrlMode m_CCMMode = CamCtrlMode.CCM_Default;
    public static float m_CCMDelay = 1.0f;
    bool IsShowCursor = false;
    //--- ī�޶� ��Ʈ�� ��� ���� ����

    // Start is called before the first frame update
    void Start()
    {
        dist = 3.4f;
        height = 2.8f;

        //--- Side Wall ����Ʈ �����...
        m_WallLyMask = 1 << LayerMask.NameToLayer("SideWall");
        //SideWall"�� ���̾ Ŭ�� �����ϰ� �ϴ� �ɼ�

        m_SideWalls = GameObject.FindGameObjectsWithTag("SideWall");
        if (0 < m_SideWalls.Length)
        {
            SideWall a_SdWall = null;
            for (int ii = 0; ii < m_SideWalls.Length; ii++)
            {
                a_SdWall = new SideWall();
                a_SdWall.m_IsColl = false;
                a_SdWall.m_SideWalls = m_SideWalls[ii];
                a_SdWall.m_WallMaterial =
                        m_SideWalls[ii].GetComponent<Renderer>().material;
                WallAlphaOnOff(a_SdWall.m_WallMaterial, false);
                m_SW_List.Add(a_SdWall);
            }
        }//if(0 < m_SideWalls.Length)
        //--- Side Wall ����Ʈ �����...

        //--- ī�޶� ��ġ ���
        m_RotV = m_DefaltRotV;  //���� ������ ȸ�� ����
        //--- ī�޶� ��ġ ���

        //--- ī�޶� ��Ʈ�� ��� �ʱ�ȭ
        int a_CCMMode = PlayerPrefs.GetInt("CamCtrlMode", 0);
        if (a_CCMMode == 0)
            m_CCMMode = CamCtrlMode.CCM_Default;
        else
            m_CCMMode = CamCtrlMode.CCM_WithMBtn;

        m_CCMDelay = 1.0f;
        //--- ī�޶� ��Ʈ�� ��� �ʱ�ȭ

    }//void Start()

    // Update is called once per frame
    void Update()
    {
        if (GameMgr.s_GameState == GameState.GameEnd)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            IsShowCursor = !IsShowCursor;

        if (IsShowCursor == true || m_CCMMode == CamCtrlMode.CCM_WithMBtn)
        {
            Cursor.visible = true;      //Ŀ���� ���̰� ó���ϴ� �ɼ�
            Cursor.lockState = CursorLockMode.None;  //Ŀ���� ȭ�� ������ ��� �� �ְ� 
        }
        else //if(IsShowCursor == false)
        {
            Cursor.visible = false;     //Ŀ���� ������ �ʰ� ó���ϴ� �ɼ�
            Cursor.lockState = CursorLockMode.Locked;   //Ŀ���� ȭ�� ������ ��� �� ���� �ϴ� �ɼ�
        }

        bool IsMsRot = false;
        float a_AddRotSpeed = 235.0f;
        if(m_CCMMode == CamCtrlMode.CCM_Default)
        {
            if (0.0f < m_CCMDelay)
                m_CCMDelay -= Time.deltaTime;

            if (m_CCMDelay <= 0.0f)
                IsMsRot = true;
            else
            {
                if (Input.GetMouseButton(0) == true || Input.GetMouseButton(1) == true)
                    IsMsRot = true;
            }

            a_AddRotSpeed = 180.0f;
            dampTrace = 10.0f;
        }
        else //if(m_CCMMode == CamCtrlMode.CCM_WithMBtn)
        {
            if (Input.GetMouseButton(0) == true || Input.GetMouseButton(1) == true)
                IsMsRot = true;

            dampTrace = 20.0f;
        }//else if(m_CCMMode == CamCtrlMode.CCM_WithMBtn)

        //if (Input.GetMouseButton(0) == true || Input.GetMouseButton(1) == true)
        if(IsMsRot == true)
        {
            ////---- ī�޶� �� �Ʒ� �ٶ󺸴� ���� ������ ���� ������ ���� �ڵ�
            //height -= (rotSpeed * Time.deltaTime * Input.GetAxis("Mouse Y"));

            //if (height < 0.1f)
            //    height = 0.1f;

            //if (5.7f < height)
            //    height = 5.7f;
            ////---- ī�޶� �� �Ʒ� �ٶ󺸴� ���� ������ ���� ������ ���� �ڵ�

            //--- (����ǥ�踦 �̿��� ���� ȸ�� ó�� �ڵ�)
            rotSpeed = a_AddRotSpeed;   //235.0f;  //ī�޶� ���Ʒ� ȸ�� �ӵ�
            m_RotV -= (rotSpeed * Time.deltaTime * Input.GetAxisRaw("Mouse Y"));
            //���콺�� ���Ʒ��� �������� �� ��
            if (m_RotV < m_MinLimitV)
                m_RotV = m_MinLimitV;
            if (m_MaxLimitV < m_RotV)
                m_RotV = m_MaxLimitV;
            //--- (����ǥ�踦 �̿��� ���� ȸ�� ó�� �ڵ�)
        }//if(Input.GetMouseButton(0) == true || Input.GetMouseButton(1) == true)

        //--- ī�޶� ���� �ܾƿ�
        if(Input.GetAxis("Mouse ScrollWheel") < 0 && dist < m_MaxDist)
        {
            dist += m_ZoomSpeed;
        }

        if(Input.GetAxis("Mouse ScrollWheel") > 0 && dist > m_MinDist)
        {
            dist -= m_ZoomSpeed;
        }
        //--- ī�޶� ���� �ܾƿ�

        //--- Rifle ���� ���
        m_RFCacRot = Quaternion.Euler(
                                Camera.main.transform.eulerAngles.x - m_MarginRotV,
                                targetTr.eulerAngles.y,
                                0.0f);
        m_RifleDir = m_RFCacRot * m_RFCacPos;
        //--- Rifle ���� ���

        if(Input.GetKeyDown(KeyCode.C))
        {
            if(m_CCMMode == CamCtrlMode.CCM_Default)
            {
                PlayerPrefs.SetInt("CamCtrlMode", 1);
                m_CCMMode = CamCtrlMode.CCM_WithMBtn;
            }
            else //if(m_CCMMode == CamCtrlMode.CCM_WithMBtn)
            {
                PlayerPrefs.SetInt("CamCtrlMode", 0);
                m_CCMMode = CamCtrlMode.CCM_Default;
            }
        }//if(Input.GetKeyDown(KeyCode.C))

    }//void Update()

    //Update �Լ� ȣ�� ���� �� ���� ȣ��Ǵ� �Լ��� LateUpdate ���
    //������ Ÿ���� �̵��� ����� ���Ŀ� ī�޶� �����ϱ� ���� LateUpdate ���
    void LateUpdate()
    {
        m_PlayerVec = targetTr.position;
        m_PlayerVec.y = m_PlayerVec.y + 1.2f;

        ////--- ī�޶� ��ġ ��� �ִ� ���밭�� �ҽ�
        ////ī�޶��� ��ġ�� ��������� dist ������ŭ �������� ��ġ�ϰ�
        ////height ������ŭ ���� �ø�
        //transform.position = Vector3.Lerp(transform.position,
        //                                 targetTr.position
        //                                 - (targetTr.forward * dist)
        //                                 + (Vector3.up * height),
        //                                 Time.deltaTime * dampTrace);
        ////--- ī�޶� ��ġ ��� �ִ� ���밭�� �ҽ�

        //--- (����ǥ�踦 ������ǥ��� ȯ���ؼ� ī�޶��� ��ġ�� ����ִ� �ڵ�)
        m_BuffRot = Quaternion.Euler(m_RotV, targetTr.eulerAngles.y, 0.0f);
        m_BasicPos.x = 0.0f;
        m_BasicPos.y = 0.0f;
        m_BasicPos.z = -dist;
        m_BuffPos = m_PlayerVec + (m_BuffRot * m_BasicPos);
        transform.position = Vector3.Lerp(transform.position, m_BuffPos,
                                                Time.deltaTime * dampTrace);
        //--- (����ǥ�踦 ������ǥ��� ȯ���ؼ� ī�޶��� ��ġ�� ����ִ� �ڵ�)

        //ī�޶� Ÿ�� ���ӿ�����Ʈ�� �ٶ󺸰� ����
        transform.LookAt(m_PlayerVec);

        //---- Wall ī�޶� �浹 ó�� �κ�
        m_CacVLen = this.transform.position - targetTr.position;
        m_CacDirVec = m_CacVLen.normalized;
        GameObject a_FindObj = null;
        RaycastHit a_HitInfo;
        if(Physics.Raycast(targetTr.position + (-m_CacDirVec * 1.0f), 
            m_CacDirVec, out a_HitInfo, m_CacVLen.magnitude + 4.0f, m_WallLyMask.value))
        {
            a_FindObj = a_HitInfo.collider.gameObject;
        }

        for(int ii = 0; ii < m_SW_List.Count; ii++)
        {
            if (m_SW_List[ii].m_SideWalls == null)
                continue;

            if(m_SW_List[ii].m_SideWalls == a_FindObj)
            {
                if(m_SW_List[ii].m_IsColl == false)
                {
                    WallAlphaOnOff(m_SW_List[ii].m_WallMaterial, true);
                    m_SW_List[ii].m_IsColl = true;
                }
            }//if(m_SW_List[ii].m_SideWalls == a_FindObj)
            else
            {
                if(m_SW_List[ii].m_IsColl == true)
                {
                    WallAlphaOnOff(m_SW_List[ii].m_WallMaterial, false);
                    m_SW_List[ii].m_IsColl = false;
                }
            }
        }//for(int ii = 0; ii < m_SW_List.Count; ii++)
         //---- Wall ī�޶� �浹 ó�� �κ�

        //if(Input.GetKeyDown(KeyCode.C))
        //{
        //    CharacterChange();
        //}//if(Input.GetKeyDown(KeyCode.C))

        //--- Rifle ���� ���
        m_RFCacRot = Quaternion.Euler(
                                Camera.main.transform.eulerAngles.x - m_MarginRotV,
                                targetTr.eulerAngles.y,
                                0.0f);
        m_RifleDir = m_RFCacRot * m_RFCacPos;
        //--- Rifle ���� ���

    }//void LateUpdate()

    void WallAlphaOnOff(Material mtrl, bool isOn = true)
    {
        if(isOn == true) //������ �� �� 
        {
            mtrl.SetFloat("_Mode", 3); //Transparent
            mtrl.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mtrl.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mtrl.SetInt("_ZWrite", 0);
            mtrl.DisableKeyword("_ALPHATEST_ON");
            mtrl.DisableKeyword("_ALPHABLEND_ON");
            mtrl.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            mtrl.renderQueue = 3000;
            mtrl.color = new Color(1, 1, 1, 0.2f);
        }
        else //������ �� ��
        {
            mtrl.SetFloat("_Mode", 0); //Opaque
            mtrl.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mtrl.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            mtrl.SetInt("_ZWrite", 1);
            mtrl.DisableKeyword("_ALPHATEST_ON");
            mtrl.DisableKeyword("_ALPHABLEND_ON");
            mtrl.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mtrl.renderQueue = -1;
            mtrl.color = new Color(1, 1, 1, 1);
        }
    }//void WallAlphaOnOff(Material mtrl, bool isOn = true)

    void CharacterChange()
    {
        Vector3 a_Pos = CharObjects[CharType].transform.position;
        Quaternion a_Rot = CharObjects[CharType].transform.rotation;
        int a_hp = CharObjects[CharType].GetComponent<PlayerCtrl>().hp;
        CharObjects[CharType].SetActive(false);
        CharType++;
        if (CharObjects.Length <= CharType)
            CharType = 0;
        CharObjects[CharType].SetActive(true);
        CharObjects[CharType].transform.position = a_Pos;
        CharObjects[CharType].transform.rotation = a_Rot;
        CharObjects[CharType].GetComponent<PlayerCtrl>().hp = a_hp;
        targetTr = CharObjects[CharType].transform;
    }

}
