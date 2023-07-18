using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyMgr : MonoBehaviour
{
    public Button m_Start_Btn;
    public Button m_Store_Btn;
    public Button m_LogOut_Btn;
    public Button m_Clear_Save_Btn;

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1.0f;  //일시정지를 원래 속도로...

        if (m_Start_Btn != null)
            m_Start_Btn.onClick.AddListener(StartBtnClick);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void StartBtnClick()
    {
        SceneManager.LoadScene("scLevel01");
        SceneManager.LoadScene("scPlay", LoadSceneMode.Additive);
    }
}
