using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealTextCtrl : MonoBehaviour
{
    Text m_RefText = null;
    float m_HealVal = 0.0f;
    Vector3 m_WorldPos = Vector3.zero;
    Animator m_RefAnim = null;

    //// Start is called before the first frame update
    //void Start()
    //{
        
    //}

    //// Update is called once per frame
    //void Update()
    //{
        
    //}

    public void InitState(int cont, Vector3 a_WSpawnPos, Transform a_Canvas, Color a_Color)
    {
        Vector3 a_StCacPos = new Vector3(a_WSpawnPos.x, a_WSpawnPos.y + 2.21f, a_WSpawnPos.z);
        transform.SetParent(a_Canvas, false);
        m_WorldPos = a_StCacPos;
        m_HealVal = cont;

        //--- 초기 위치 잡아주기 //--- World 좌표를 UGUI 좌표로 환산해 주는 코드
        RectTransform a_CanvasRect = a_Canvas.GetComponent<RectTransform>();
        Vector2 a_ScreenPos = Camera.main.WorldToViewportPoint(a_StCacPos);
        Vector2 a_WdScPos = Vector2.zero;
        a_WdScPos.x = (a_ScreenPos.x * a_CanvasRect.sizeDelta.x) -
                                            (a_CanvasRect.sizeDelta.x * 0.5f);
        a_WdScPos.y = (a_ScreenPos.y * a_CanvasRect.sizeDelta.y) -
                                            (a_CanvasRect.sizeDelta.y * 0.5f);
        //a_CanvasRect.sizeDelta UI 기준의 화면의 크기 1280 * 720
        this.GetComponent<RectTransform>().anchoredPosition = a_WdScPos;
        //--- 초기 위치 잡아주기 //--- World 좌표를 UGUI 좌표로 환산해 주는 코드

        m_RefText = this.gameObject.GetComponentInChildren<Text>();
        if(m_RefText != null)
        {
            if (m_HealVal <= 0)
                m_RefText.text = "-" + m_HealVal.ToString() + " Dmg";
            else //if(0 < m_HealVal)
                m_RefText.text = "+" + m_HealVal.ToString() + " Heal";

            m_RefText.color = a_Color;
        }

        m_RefAnim = GetComponentInChildren<Animator>();
        if(m_RefAnim != null)
        {
            AnimatorStateInfo a_AnimInfo = m_RefAnim.GetCurrentAnimatorStateInfo(0);
            float a_LifeTime = a_AnimInfo.length;   //애니메이션 플레이 시간
            Destroy(gameObject, a_LifeTime);
        }
    }//public void InitState(int cont, Vector3 a_WSpawnPos, Transform a_Canvas, Color a_Color)
}
