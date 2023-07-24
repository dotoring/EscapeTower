using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemDragMgr : MonoBehaviour
{
    public SlotScript[] m_ProductSlots;
    public SlotScript[] m_InvenSlots;
    public Image m_MsObj = null;    //���콺�� ���� �ٳ�� �ϴ� ������Ʈ
    int m_SaveIndex = -1;           //-1�� �ƴϸ� �������� ��ŷ���¿��� �巡�� ���̶�� ��

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

        //HelpText ������ ������� ����
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
                    ShowMessage("�ش� ���Կ��� �������� ������ �� �����ϴ�.");
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

    //���콺�� UI ���� ���� �ִ��� �Ǵ��ϴ� �Լ�
    bool IsCollSlot(SlotScript a_CkSlot)
    {
        if(a_CkSlot == null)
        {
            return false;
        }

        Vector3[] v = new Vector3[4];
        a_CkSlot.GetComponent<RectTransform>().GetWorldCorners(v);
        //v[0] : �����ϴ�, v[1] : �������, v[2] : �������, v[3] : �����ϴ�
        //v[0] �����ϴ��� 0,0 ��ǥ�� ��ũ����ǥ�� ���콺 ��ǥ�� RectTransform : �� UGUI ��ǥ ��������
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
            ShowMessage("��尡 �����մϴ�.");
            return false;
        }

        int a_CurBagSize = 0;
        for(int i = 0; i < GlobalValue.g_SkillCount.Length; i++)
        {
            a_CurBagSize += GlobalValue.g_SkillCount[i];
        }

        if(10 <= a_CurBagSize)
        {
            ShowMessage("������ ���� á���ϴ�.");
            return false;
        }

        GlobalValue.g_SkillCount[a_SkIdx]++;
        GlobalValue.g_UserGold -= a_Cost;

        //�������� ���ÿ� ����
        string a_MkKey = "SkItem_" + a_SkIdx.ToString();
        PlayerPrefs.SetInt(a_MkKey, GlobalValue.g_SkillCount[a_SkIdx]);
        PlayerPrefs.SetInt("UserGold", GlobalValue.g_UserGold);

        //UI ����
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
            m_StMgr.m_UserInfoText.text = "����(" + GlobalValue.g_NickName + ") : �������(" +
                GlobalValue.g_UserGold + ")";
        }

        int a_CurBagSize = 0;
        for (int i = 0; i < GlobalValue.g_SkillCount.Length; i++)
        {
            a_CurBagSize += GlobalValue.g_SkillCount[i];
        }
        m_BagSizeText.text = "��������� : " + a_CurBagSize + " / 10";
    }
}
