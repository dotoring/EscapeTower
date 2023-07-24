using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemDragMgr : MonoBehaviour
{
    public SlotScript[] m_ProductSlots;
    public SlotScript[] m_InvenSlots;
    public Image m_MsObj = null;    //마우스를 따라 다녀야 하는 오브젝트
    int m_SaveIndex = -1;           //-1이 아니면 아이템을 픽킹상태에서 드래그 중이라는 뜻

    public Text m_BagSizeText;
    public Text m_HelpText;
    float m_HelpDuring = 1.5f;
    float m_HelpAddTimer = 0.0f;
    float m_CalcTime = 0.0f;
    Color m_Color;

    StoreMgr m_StMgr = null;

    // Start is called before the first frame update
    void Start()
    {
        m_StMgr = GameObject.FindObjectOfType<StoreMgr>();

        RefreshUI();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0) == true)
        {
            MouseBtnDown();
        }

        if(Input.GetMouseButton(0) == true)
        {
            MousePress();
        }

        if(Input.GetMouseButtonUp(0) == true)
        {
            MouseBtnUp();
        }

        //HelpText 서서히 사라지게 연출
        if(0.0f < m_HelpAddTimer)
        {
            m_HelpAddTimer -= Time.deltaTime;
            m_CalcTime = m_HelpAddTimer / (m_HelpDuring - 1.0f);
            if(1.0f < m_CalcTime)
            {
                m_CalcTime = 1.0f;
            }
            m_Color = m_HelpText.color;
            m_Color.a = m_CalcTime;
            m_HelpText.color = m_Color;

            if(m_HelpAddTimer <= 0.0f)
            {
                m_HelpText.gameObject.SetActive(false);
            }
        }
    }

    void MouseBtnDown()
    {
        m_SaveIndex = -1;
        for(int i = 0; i < m_ProductSlots.Length; i++)
        {
            if (m_ProductSlots[i].ItemImg.gameObject.activeSelf == true &&
                IsCollSlot(m_ProductSlots[i]) == true)
            {
                m_SaveIndex = i;
                Transform a_ChildImg = m_MsObj.transform.Find("MsIconImg");
                if (a_ChildImg != null)
                {
                    a_ChildImg.GetComponent<Image>().sprite =
                                    m_ProductSlots[i].ItemImg.sprite;
                }
                //m_ProductSlots[i].ItemImg.gameObject.SetActive(false);
                m_MsObj.gameObject.SetActive(true);
                break;
            }
        }
    }

    void MousePress()
    {
        if (0 <= m_SaveIndex)
        {
            m_MsObj.transform.position = Input.mousePosition;
        }
    }

    void MouseBtnUp()
    {
        if(m_SaveIndex < 0 || m_ProductSlots.Length <= m_SaveIndex) 
        { 
            return;
        }

        int a_BuyIndex = -1;
        for(int i = 0; i < m_InvenSlots.Length; i++)
        {
            if (IsCollSlot(m_InvenSlots[i]) == true)
            {
                if(m_SaveIndex == i)
                {
                    if(BuySkItem(m_SaveIndex) == true)
                    {
                        a_BuyIndex = i;
                        break;
                    }
                }
                else
                {
                    ShowMessage("해당 슬롯에는 아이템을 장착할 수 없습니다.");
                }
                
            }
        }

        if(0 <= a_BuyIndex)
        {
            Sprite a_MsIconImg = null;
            Transform a_ChildImg = m_MsObj.transform.Find("MsIconImg");
            if (a_ChildImg != null)
            {
                a_MsIconImg = a_ChildImg.GetComponent<Image>().sprite;
            }

            m_InvenSlots[a_BuyIndex].ItemImg.sprite = a_MsIconImg;
            m_InvenSlots[a_BuyIndex].ItemImg.gameObject.SetActive(true);
            m_InvenSlots[a_BuyIndex].m_CurItemIdx = m_SaveIndex;
        }
        //else
        //{
        //    m_ProductSlots[m_SaveIndex].ItemImg.gameObject.SetActive(true);
        //}

        m_SaveIndex = -1;
        m_MsObj.gameObject.SetActive(false);
    }

    void ShowMessage(string a_Msg)
    {
        if(m_HelpText == null)
        {
            return;
        }

        m_HelpText.text = a_Msg;
        //m_HelpText.color = Color.red;
        m_HelpText.gameObject.SetActive(true);
        m_HelpAddTimer = m_HelpDuring;
    }

    //마우스가 UI 슬롯 위에 있는지 판단하는 함수
    bool IsCollSlot(SlotScript a_CkSlot)
    {
        if(a_CkSlot == null)
        {
            return false;
        }

        Vector3[] v = new Vector3[4];
        a_CkSlot.GetComponent<RectTransform>().GetWorldCorners(v);
        //v[0] : 좌측하단, v[1] : 좌측상단, v[2] : 우측상단, v[3] : 우측하단
        //v[0] 좌측하단이 0,0 좌표인 스크린좌표를 마우스 좌표계 RectTransform : 즉 UGUI 좌표 기준으로
        if (v[0].x <= Input.mousePosition.x && Input.mousePosition.x <= v[2].x &&
            v[0].y <= Input.mousePosition.y && Input.mousePosition.y <= v[2].y)
        {
            return true;
        }

        return false;
    }

    bool BuySkItem(int a_SkIdx)
    {
        int a_Cost = 300;
        if(a_SkIdx == 1)
        {
            a_Cost = 500;
        }
        else if(a_SkIdx == 2)
        {
            a_Cost = 1000;
        }

        if(GlobalValue.g_UserGold < a_Cost)
        {
            ShowMessage("골드가 부족합니다.");
            return false;
        }

        int a_CurBagSize = 0;
        for(int i = 0; i < GlobalValue.g_SkillCount.Length; i++)
        {
            a_CurBagSize += GlobalValue.g_SkillCount[i];
        }

        if(10 <= a_CurBagSize)
        {
            ShowMessage("가방이 가득 찼습니다.");
            return false;
        }

        GlobalValue.g_SkillCount[a_SkIdx]++;
        GlobalValue.g_UserGold -= a_Cost;

        //변동사항 로컬에 저장
        string a_MkKey = "SkItem_" + a_SkIdx.ToString();
        PlayerPrefs.SetInt(a_MkKey, GlobalValue.g_SkillCount[a_SkIdx]);
        PlayerPrefs.SetInt("UserGold", GlobalValue.g_UserGold);

        //UI 갱신
        RefreshUI();

        return true;
    }

    void RefreshUI()
    {
        for(int i = 0; i < m_InvenSlots.Length; i++)
        {
            if(0 < GlobalValue.g_SkillCount[i])
            {
                m_InvenSlots[i].ItemCountText.text = GlobalValue.g_SkillCount[i].ToString();
                m_InvenSlots[i].ItemImg.sprite = m_ProductSlots[i].ItemImg.sprite;
                m_InvenSlots[i].ItemImg.gameObject.SetActive(true);
                m_InvenSlots[i].m_CurItemIdx = i;
            }
            else
            {
                m_InvenSlots[i].ItemCountText.text = "0";
                m_InvenSlots[i].ItemImg.gameObject.SetActive(false);
            }
        }

        if(m_StMgr != null && m_StMgr.m_UserInfoText != null)
        {
            m_StMgr.m_UserInfoText.text = "별명(" + GlobalValue.g_NickName + ") : 보유골드(" +
                GlobalValue.g_UserGold + ")";
        }

        int a_CurBagSize = 0;
        for (int i = 0; i < GlobalValue.g_SkillCount.Length; i++)
        {
            a_CurBagSize += GlobalValue.g_SkillCount[i];
        }
        m_BagSizeText.text = "가방사이즈 : " + a_CurBagSize + " / 10";
    }
}
