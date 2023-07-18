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

    public SideWall() //생성자 함수
    {
        m_IsColl = false;
        m_SideWalls = null;
        m_WallMaterial = null;
    }
}

public class FollowCam : MonoBehaviour
{
    //--- 캐릭터 변경 관련 변수
    public GameObject[] CharObjects;   //캐릭터 2종류 오브젝트 연결 배열 변수
    int CharType = 0;
    //--- 캐릭터 변경 관련 변수

    public Transform targetTr;      //추적할 타깃 게임오브젝트의 Transform 변수
    public float dist = 10.0f;      //카메라와의 일정 거리
    public float height = 3.0f;     //카메라의 높이 설정
    public float dampTrace = 20.0f; //부드러운 추적을 위한 변수

    Vector3 m_PlayerVec = Vector3.zero;
    float rotSpeed = 10.0f;

    //---- Side Wall 리스트 관련 변수
    Vector3 m_CacVLen = Vector3.zero;
    Vector3 m_CacDirVec = Vector3.zero;

    GameObject[] m_SideWalls = null;
    LayerMask m_WallLyMask = -1;
    List<SideWall> m_SW_List = new List<SideWall>();
    //---- Side Wall 리스트 관련 변수

    //--- 카메라 위치 계산용 변수
    float m_RotV = 0.0f;        //마우스 상하 조작값 계산용 변수
    float m_DefaltRotV = 25.2f; //높이 기준의 회전 각도
    float m_MarginRotV = 22.3f; //총구와의 마진 각도
    float m_MinLimitV = -17.9f; //위 아래 각도 제한
    float m_MaxLimitV = 52.9f;  //위 아래 각도 제한
    float m_MaxDist = 4.0f;     //마우스 줌 아웃 최대 거리 제한값
    float m_MinDist = 2.0f;     //마우스 줌 인 최대 거리 제한값
    float m_ZoomSpeed = 0.7f;   //마우스 휠 조작에 대한 줌 인 아웃 스피드 설정값

    Quaternion m_BuffRot;       //카메라 회전 계산용 변수
    Vector3 m_BuffPos;       //카메라 회전에 대한 위치 좌표 계산용 변수
    Vector3 m_BasicPos = Vector3.zero;  //위치 계산용 변수
    //--- 카메라 위치 계산용 변수

    //--- 총 조준 방향 계산용 변수
    public static Vector3 m_RifleDir = Vector3.zero;        //총 조준 방향
    Quaternion m_RFCacRot;
    Vector3 m_RFCacPos = Vector3.forward;
    //--- 총 조준 방향 계산용 변수

    //--- 카메라 컨트롤 모드 관련 변수
    public static CamCtrlMode m_CCMMode = CamCtrlMode.CCM_Default;
    public static float m_CCMDelay = 1.0f;
    bool IsShowCursor = false;
    //--- 카메라 컨트롤 모드 관련 변수

    // Start is called before the first frame update
    void Start()
    {
        dist = 3.4f;
        height = 2.8f;

        //--- Side Wall 리스트 만들기...
        m_WallLyMask = 1 << LayerMask.NameToLayer("SideWall");
        //SideWall"번 레이어만 클릭 가능하게 하는 옵션

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
        //--- Side Wall 리스트 만들기...

        //--- 카메라 위치 계산
        m_RotV = m_DefaltRotV;  //높이 기준의 회전 각도
        //--- 카메라 위치 계산

        //--- 카메라 컨트롤 모드 초기화
        int a_CCMMode = PlayerPrefs.GetInt("CamCtrlMode", 0);
        if (a_CCMMode == 0)
            m_CCMMode = CamCtrlMode.CCM_Default;
        else
            m_CCMMode = CamCtrlMode.CCM_WithMBtn;

        m_CCMDelay = 1.0f;
        //--- 카메라 컨트롤 모드 초기화

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
            Cursor.visible = true;      //커서를 보이게 처리하는 옵션
            Cursor.lockState = CursorLockMode.None;  //커서가 화면 밖으로 벗어날 수 있게 
        }
        else //if(IsShowCursor == false)
        {
            Cursor.visible = false;     //커서를 보이지 않게 처리하는 옵션
            Cursor.lockState = CursorLockMode.Locked;   //커서가 화면 밖으로 벗어날 수 없게 하는 옵션
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
            ////---- 카메라 위 아래 바라보는 각도 조절을 위한 높낮이 변경 코드
            //height -= (rotSpeed * Time.deltaTime * Input.GetAxis("Mouse Y"));

            //if (height < 0.1f)
            //    height = 0.1f;

            //if (5.7f < height)
            //    height = 5.7f;
            ////---- 카메라 위 아래 바라보는 각도 조절을 위한 높낮이 변경 코드

            //--- (구좌표계를 이용한 수직 회전 처리 코드)
            rotSpeed = a_AddRotSpeed;   //235.0f;  //카메라 위아래 회전 속도
            m_RotV -= (rotSpeed * Time.deltaTime * Input.GetAxisRaw("Mouse Y"));
            //마우스를 위아래로 움직였을 때 값
            if (m_RotV < m_MinLimitV)
                m_RotV = m_MinLimitV;
            if (m_MaxLimitV < m_RotV)
                m_RotV = m_MaxLimitV;
            //--- (구좌표계를 이용한 수직 회전 처리 코드)
        }//if(Input.GetMouseButton(0) == true || Input.GetMouseButton(1) == true)

        //--- 카메라 줌인 줌아웃
        if(Input.GetAxis("Mouse ScrollWheel") < 0 && dist < m_MaxDist)
        {
            dist += m_ZoomSpeed;
        }

        if(Input.GetAxis("Mouse ScrollWheel") > 0 && dist > m_MinDist)
        {
            dist -= m_ZoomSpeed;
        }
        //--- 카메라 줌인 줌아웃

        //--- Rifle 방향 계산
        m_RFCacRot = Quaternion.Euler(
                                Camera.main.transform.eulerAngles.x - m_MarginRotV,
                                targetTr.eulerAngles.y,
                                0.0f);
        m_RifleDir = m_RFCacRot * m_RFCacPos;
        //--- Rifle 방향 계산

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

    //Update 함수 호출 이후 한 번씩 호출되는 함수인 LateUpdate 사용
    //추적할 타깃의 이동이 종료된 이후에 카메라가 추적하기 위해 LateUpdate 사용
    void LateUpdate()
    {
        m_PlayerVec = targetTr.position;
        m_PlayerVec.y = m_PlayerVec.y + 1.2f;

        ////--- 카메라 위치 잡아 주는 절대강좌 소스
        ////카메라의 위치를 추적대상의 dist 변수만큼 위쪽으로 배치하고
        ////height 변수만큼 위로 올림
        //transform.position = Vector3.Lerp(transform.position,
        //                                 targetTr.position
        //                                 - (targetTr.forward * dist)
        //                                 + (Vector3.up * height),
        //                                 Time.deltaTime * dampTrace);
        ////--- 카메라 위치 잡아 주는 절대강좌 소스

        //--- (구좌표계를 직각좌표계로 환산해서 카메라의 위치를 잡아주는 코드)
        m_BuffRot = Quaternion.Euler(m_RotV, targetTr.eulerAngles.y, 0.0f);
        m_BasicPos.x = 0.0f;
        m_BasicPos.y = 0.0f;
        m_BasicPos.z = -dist;
        m_BuffPos = m_PlayerVec + (m_BuffRot * m_BasicPos);
        transform.position = Vector3.Lerp(transform.position, m_BuffPos,
                                                Time.deltaTime * dampTrace);
        //--- (구좌표계를 직각좌표계로 환산해서 카메라의 위치를 잡아주는 코드)

        //카메라가 타깃 게임오브젝트를 바라보게 설정
        transform.LookAt(m_PlayerVec);

        //---- Wall 카메라 충돌 처리 부분
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
         //---- Wall 카메라 충돌 처리 부분

        //if(Input.GetKeyDown(KeyCode.C))
        //{
        //    CharacterChange();
        //}//if(Input.GetKeyDown(KeyCode.C))

        //--- Rifle 방향 계산
        m_RFCacRot = Quaternion.Euler(
                                Camera.main.transform.eulerAngles.x - m_MarginRotV,
                                targetTr.eulerAngles.y,
                                0.0f);
        m_RifleDir = m_RFCacRot * m_RFCacPos;
        //--- Rifle 방향 계산

    }//void LateUpdate()

    void WallAlphaOnOff(Material mtrl, bool isOn = true)
    {
        if(isOn == true) //투명도를 켤 때 
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
        else //투명도를 끌 때
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
