using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{
    Skill_0 = 0,    //30% 힐링
    Skill_1,        //수류탄
    Skill_2,        //보호막
    SkCount
}

public class GlobalValue 
{
    public static string g_Unique_ID = "";  //유저의 고유번호

    public static string g_NickName = "";   //유저의 별명
    public static int g_BestScore = 0;      //게임점수
    public static int g_UserGold = 0;       //게임머니
    public static int g_Exp = 0;            //경험치 Experience
    public static int g_Level = 0;          //레벨

    public static int[] g_SkillCount = new int[3];  //아이템 보유수

    public static void LoadGameData()
    {
        g_NickName  = PlayerPrefs.GetString("NickName", "SBS영웅");
        g_BestScore = PlayerPrefs.GetInt("BestScore", 0);
        g_UserGold  = PlayerPrefs.GetInt("UserGold", 0);

        string a_MkKey = "";
        for(int ii = 0; ii < g_SkillCount.Length; ii++)
        {
            a_MkKey = "SkItem_" + ii.ToString();
            g_SkillCount[ii] = PlayerPrefs.GetInt(a_MkKey, 0);

            g_SkillCount[ii] = 3 - ii;  //Test 용
        }
    }//public static void LoadGameData()
}
