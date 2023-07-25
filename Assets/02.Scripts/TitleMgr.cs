using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleMgr : MonoBehaviour
{
    [Header("------ LoginPanel ------")]
    public GameObject m_LoginPanelObj;
    public Button m_LoginBtn = null;
    // Start is called before the first frame update
    void Start()
    {
        if(m_LoginBtn != null)
        {
            m_LoginBtn.onClick.AddListener(LoginBtn);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LoginBtn()
    {
        SceneManager.LoadScene("scLobby");
    }
}
