using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StoreMgr : MonoBehaviour
{
    public Button m_BackBtn = null;
    public Text m_UserInfoText = null;

    void Awake()
    {
        GlobalValue.LoadGameData();
    }
    // Start is called before the first frame update
    void Start()
    {
        if(m_BackBtn != null)
        {
            m_BackBtn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("scLobby");
            });
        }

        if(m_UserInfoText != null)
        {
            m_UserInfoText.text = "����(" + GlobalValue.g_NickName + ") : �������(" +
                    GlobalValue.g_UserGold + ")";
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
