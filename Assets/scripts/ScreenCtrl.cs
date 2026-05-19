/*******************************************************************************************************
 * 
 *	ScreenCtrl.cs 
 *		スプライトなどメイン画面3画面を個別に反転する制御
 *		
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
********************************************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenCtrl : MonoBehaviour {
	[SerializeField]
	GameObject SCREEN_BACKGROUND;
	[SerializeField]
	GameObject SCREEN_WAVEFORM;
	[SerializeField]
	GameObject SCREEN_EQUALIZER;
	[SerializeField]
	GameObject SCREEN_GAME;
	

	public bool flip_background = false;	// 背景・反転時true(ゲーム画面と同期)
	public bool flip_waveform = false;		// 波形・反転時true
	public bool flip_equalizer = false;		// イコライザー・反転時true
	public bool flip_game = false;			// ゲーム画面・反転時true




	// Use this for initialization
	void Awake () {
		// 起動時に状態を設定からロード、画面反転設定を行うこと

		if (PlayerPrefs.HasKey("ScreenFlip") == true)
		{
			int sf = PlayerPrefs.GetInt("ScreenFlip");
			if (sf == 1)
			{
				//				GameScreenFlip();
				GameScreenScrollRight();
			}
		}

	}





	public void WaveformScreenFlip()
	{
		flip_waveform = !flip_waveform; // 反転
		SCREEN_WAVEFORM.transform.localEulerAngles = new Vector3(0, (SCREEN_WAVEFORM.transform.localEulerAngles.y + 180) % 360, 0);
		//return flip_waveform;
	}


	public void EqualizerScreenFlip()
	{
		flip_equalizer = !flip_equalizer; // 反転
		SCREEN_EQUALIZER.transform.localEulerAngles = new Vector3(0, (SCREEN_EQUALIZER.transform.localEulerAngles.y + 180) % 360, 0);
		//return flip_waveform;
	}


	public void GameScreenFlip()
	{
		flip_game = !flip_game; // 反転
		flip_background = !flip_background;
		SCREEN_GAME.transform.localEulerAngles = new Vector3(0, (SCREEN_GAME.transform.localEulerAngles.y + 180) % 360, 0);
		SCREEN_BACKGROUND.transform.localEulerAngles = new Vector3(0, (SCREEN_BACKGROUND.transform.localEulerAngles.y + 180) % 360, 0);
		//return flip_waveform;
	}
	public void GameScreenScrollLeft()
	{
		flip_game = false;
		SCREEN_GAME.transform.localEulerAngles = new Vector3(0, 0, 0);
		//SCREEN_BACKGROUND.transform.localEulerAngles = new Vector3(0, 0, 0);
		//PlayerPrefs.SetInt("ScreenFlip", 0);
	}
	public void GameScreenScrollRight()
	{
		flip_game = true;
		SCREEN_GAME.transform.localEulerAngles = new Vector3(0, 180, 0);
		//SCREEN_BACKGROUND.transform.localEulerAngles = new Vector3(0, 180, 0);
		//PlayerPrefs.SetInt("ScreenFlip", 1);
	}
}
