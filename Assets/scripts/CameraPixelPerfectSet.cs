/*
 * 
 *	CameraPixelPerfectSet.cs
 *		カメラ焦点を矩形サイズに固定する
 * 
 * 
 * 
 * 
 * 
 *	20221211	3日前くらいにWSc101用に再構成
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraPixelPerfectSet : MonoBehaviour {

	[SerializeField]
	Camera cam;
	[SerializeField]
	CanvasScaler[] canvas;

	int PixelsToUnit = 1;
	const float ScreenWidth = 800;		// 目指す解像度(横基準)
	const float ScreenHeight = 600;

	void Update()
	{
		cam.fieldOfView = 110;
		bool longHeightScreen = false;
        if (((float)Screen.height / (float)Screen.width) < (ScreenHeight / ScreenWidth))
        {
            // 縦長端末
            longHeightScreen = true;
        }
//#endif
        int width = (int)ScreenWidth;
		if (longHeightScreen == true)   // 縦に長い端末
		{
			for (int i = 0; i < canvas.Length; i++)
			{
				canvas[i].screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
				canvas[i].matchWidthOrHeight = 1.0f;
			}
		}
		else
		{
			for (int i = 0; i < canvas.Length; i++)
			{
				canvas[i].screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
				canvas[i].matchWidthOrHeight = 0.0f;
			}
		}
		int height = Mathf.RoundToInt(width / (float)Screen.width * (float)Screen.height);  // カメラ距離確定
		cam.orthographicSize = height / PixelsToUnit / 2;   // カメラ焦点サイズ調整
	}


}
