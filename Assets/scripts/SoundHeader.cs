/* ==========================================================================================================
 * 
 *		SoundHeader.cs
 *		Circle-AY.Info / U.G.M.
 *		
 *		音周りの定義ファイル
 *		番号で呼ぶと見通しが悪いので名前を定義する
 *		SoundManagerの再生関数と同時に使われることを想定している
 * 
 *		20150321	とりあえず作成
 *		20220810	RSc100用に更新
 *		20221213	WSc101用に更新
 *		20230620	theroom用に更新
 * 
 * ========================================================================================================== 
 */

using UnityEngine;
using System.Collections;

public class SoundHeader
{

	//曲列挙.
	public enum BGM
	{
		PLAY_GAME=0,
		Count
	}

	//SE列挙.
	public enum SE
	{
		FLOOR_PAINT=0,
		SHIP_SHOT,
		ENEMY_BORN,
		ENEMY_SHOT_HIT,
		ENEMY_FLOOR_SHIP,
		ENEMY_SHIP_HIT,
		PLAY_START,
		CLEAR_FLOOR_ALL,
		CLEAR_ENEMY_ALL,
		Count
	}
		
	//音声列挙.
	public enum VOICE
	{
		Count
	}

}
