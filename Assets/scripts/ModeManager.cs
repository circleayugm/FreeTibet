/*******************************************************************************************************
 * 
 *	ModeManager.cs 
 *		現在のモードを返すグローバル変数を持つ
 *		ここを切り替えてシーンチェンジに移行するように組む
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 *	20221211	3日前くらいにWSc101用に再構成
 * 
 * 
********************************************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ModeManager : MonoBehaviour
{

    // --- シングルトン化 ---
    public static ModeManager Instance;
    void Awake() { Instance = this; }

    [Header("Views")]
    [SerializeField] GameObject PANEL_INITIALIZE;
    [SerializeField] GameObject PANEL_DEMO;
	[SerializeField] GameObject PANEL_GAME;
    
	// モード
    public enum MODE
	{
		//INITIALIZE = 0,
		INITIALIZE=0,
		DEMO,
		GAME,
		//GAME_PAUSE,
		MAX
	};
	// ModeManager.cs 内
	public static readonly string[] MODE_SCENE = new string[] // ← publicが必要
    {
		"00_initialize",
		"01_demo",
		"02_game",	
		"SCENE_MAX"
	};


    public static MODE mode = MODE.INITIALIZE;  // 現在のモード

    // 表示の更新（Instance経由で本物を操作する）
    public static void RefreshView(MODE targetMode)
    {
        if (Instance == null) return;

        //Instance.PANEL_INITIALIZE.SetActive(targetMode == MODE.INITIALIZE);
        //Instance.PANEL_DEMO.SetActive(targetMode == MODE.DEMO);
        //Instance.PANEL_GAME.SetActive(targetMode == MODE.GAME);
        switch (targetMode)
        {
            case MODE.INITIALIZE:
                Instance.PANEL_INITIALIZE.SetActive(true);
                break;
            case MODE.DEMO:
                Instance.PANEL_DEMO.SetActive(true);
                break;
            case MODE.GAME:
                Instance.PANEL_GAME.SetActive(true);
                break;
        }
    }



    // モードチェンジ関数
    public static MODE ChangeMode(MODE newmode)
	{
		if (mode != newmode)
		{
			{
                mode = newmode;
				RefreshView(newmode);
                // SceneManager.LoadScene は使わない！
                Debug.Log($"モードが {newmode} に切り替わりました");
            }
		}
		return mode;
	}
}
