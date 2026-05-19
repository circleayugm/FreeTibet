//#define DEBUGSW		// デバッグ時有効.

/*******************************************************************************************************
 * 
 *	TouchPanelCtrl.cs 
 *		タッチパネル制御用スクリプト
 *		オブジェクトに貼り付けて利用
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

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public sealed class TouchPanelCtrl : MonoBehaviour {

	// インスペクタ指定.
	[SerializeField]
	RectTransform uGUIroot;     // ドットサイズが指定されたuGUIのパネル.
	[SerializeField]
	SpriteRenderer cursor;      // 指の位置に表示するカーソル
	[SerializeField]
	SpriteMask cursor_mask;
	[SerializeField]
	BoxCollider2D cursor_prefab;
	[Space(20)]
	[SerializeField]
	SpriteRenderer cursor2;     // 指2本目の位置に表示するカーソル
	[SerializeField]
	SpriteMask cursor2_mask;
	[SerializeField]
	BoxCollider2D cursor2_prefab;
	[Space(20)]
	[SerializeField]
	Sprite[] SPRITE_TOUCHPANEL; // タッチアニメーション
	[SerializeField]
	ObjectManager manage;
	[SerializeField]
	MainSceneCtrl main;

	// 公開変数.
	public Vector3 pos0;         // タッチ座標(中央が0,0・Zは無視).
	public Vector3 pos1;
	public bool on0 = false;     // タッチされている？(true=されている・false=されていない).
	public bool on1 = false;
	public int count = 0;       // 状態が変わった瞬間を0としたカウンタ・押したor離した瞬間0.
	public int count2 = 0;		// 2本目の指が押したor離した瞬間0
	public Vector3 move;        // 移動量(Zは無視).
	public Vector3 move2;
	public float movemulti = 1.0f;	// 移動倍率(0.5f~1.5fくらいで調整できるようにする)

	// 内部変数.
	Vector3 pos_old0;            // 1フレーム前のタッチ座標.
	Vector3 pos_old1;
	bool on_old0 = false;        // 1フレーム前のタッチ判定.
	bool on_old1 = false;
	int fid0 = -1;              // 指ID
	int fid1 = -1;
	int fid_old0 = -1;          // 1フレーム前の指ID・指IDが一致しなかった場合移動量をゼロにする
	int fid_old1 = -1;
	Vector3 pos_nowpos0;		// 放物線計算時の実際の座標
	Vector3 pos_nowpos1;
	float multi;                // 座標取得拡大率.
#if DEBUGSW
	Text msg_debugout = null;   // デバッグ時文字出力.
#endif
	ScreenCtrl scr;

	void Awake()
	{
		Vector3 reso = new Vector3(((float)uGUIroot.rect.width / (float)Screen.width), ((float)uGUIroot.rect.height / (float)Screen.height), 0);    // 解像度に依存しない倍率.
		if (reso.x < reso.y)                // 解像度比が大きな側をタッチパネルサイズ基準に指定(表示画面の外をタッチされても一応座標は返る).
		{
			multi = reso.y;
		}
		else
		{
			multi = reso.x;
		}
	}



	// Use this for initialization
	void Start()
	{
#if DEBUGSW
		//msg_debugout = GameObject.Find("msg_debugout").GetComponent<Text>();
#endif
		scr = GameObject.Find("root_game").GetComponent<ScreenCtrl>();
		pos0 = new Vector3(0, 0, 0);
		pos_old0 = new Vector3(0, 0, 0);
		fid0 = -1;
		fid1 = -1;
		on_old0 = false;
		on_old1 = false;
		on0 = false;
		on1 = false;
		count = 0;
		count2 = 0;
	}

	// Update is called once per frame
	void Update()
	{
#if false
		if (ModeManager.mode==ModeManager.MODE.GAME_PAUSE)
		{
			return;
		}
#endif
		Vector3 p = new Vector3(0,0,0);
		Vector3 p2 = new Vector3(0, 0, 0);
//		on0 = false;
//		on1 = false;
#if UNITY_EDITOR
		p = Input.mousePosition;    // タッチ位置取得.
		on0 = Input.GetMouseButton(0);
#elif UNITY_STANDALONE_WIN
		p = Input.mousePosition;    // タッチ位置取得.
		on0 = Input.GetMouseButton(0);
#elif UNITY_IPHONE || UNITY_ANDROID
#if true
		int myTouchCount = Input.touchCount;
		if (myTouchCount==0)
		{
			on_old0=false;
			on_old1=false;
			on0=false;
			on1=false;
		}
		//Touch myTouch = Input.GetTouch(0);

        Touch[] myTouches = Input.touches;
        for(int i = 0; i < myTouchCount; i++)
        {
			switch(i)
			{
				case 0:
					fid0=myTouches[0].fingerId;
					if (fid0==fid_old1){
						switch(myTouches[0].phase)
						{
							case TouchPhase.Began:
								fid_old1=fid0;
								p2.x = myTouches[0].position.x;
								p2.y = myTouches[0].position.y;
								p2.z = 0.0f;
								on1=true;
								break;
							case TouchPhase.Moved:
							case TouchPhase.Stationary:
								if (fid0==fid_old1)
								{
									p2.x = myTouches[0].position.x;
									p2.y = myTouches[0].position.y;
									p2.z = 0.0f;
									on1=true;
								}
								else
								{
									p2=new Vector3(0,0,0);
									on1=false;
								}
								break;
							case TouchPhase.Canceled:
							case TouchPhase.Ended:
								p2=new Vector3(0,0,0);
								on1=false;
								fid1=-1;
								break;
						}
					}
					else
					{
						switch(myTouches[0].phase)
						{
							case TouchPhase.Began:
								fid_old0=fid0;
								p.x = myTouches[0].position.x;
								p.y = myTouches[0].position.y;
								p.z = 0.0f;
								on0=true;
								break;
							case TouchPhase.Moved:
							case TouchPhase.Stationary:
								if (fid0==fid_old0)
								{
									p.x = myTouches[0].position.x;
									p.y = myTouches[0].position.y;
									p.z = 0.0f;
									on0=true;
								}
								else
								{
									p=new Vector3(0,0,0);
									on0=false;
								}
								break;
							case TouchPhase.Canceled:
							case TouchPhase.Ended:
								p=new Vector3(0,0,0);
								on0=false;
								fid0=-1;
								break;
						}
					}
					break;
				case 1:
					fid1=myTouches[1].fingerId;
					switch(myTouches[1].phase)
					{
						case TouchPhase.Began:
							fid_old1=fid1;
							p2.x = myTouches[1].position.x;
							p2.y = myTouches[1].position.y;
							p2.z = 0.0f;
							on1=true;
							break;
						case TouchPhase.Moved:
						case TouchPhase.Stationary:
							if (fid1==fid_old1)
							{
								p2.x = myTouches[1].position.x;
								p2.y = myTouches[1].position.y;
								p2.z = 0.0f;
								on1=true;
							}
							else
							{
								p2=new Vector3(0,0,0);
								on1=false;
							}
							break;
						case TouchPhase.Canceled:
						case TouchPhase.Ended:
							p2=new Vector3(0,0,0);
							on1=false;
							fid1=-1;
							break;
					}
					break;
				default:
					break;
			}
        }
#elif false
		for (int i = 0; i < Input.touchCount; i++)
		{
			Touch touch = Input.GetTouch(i);
			switch(i)
			{
				case 0:
				{
					fid_old0=fid0;
					fid0=touch.fingerId;
					if (touch.phase==TouchPhase.Began)
					{
						fid_old0=fid0;
					}
					else if (									// タッチパネルに触れている時だけ処理
							(touch.phase==TouchPhase.Began)
						||	(touch.phase==TouchPhase.Moved)
						||	(touch.phase==TouchPhase.Stationary)
						)
					{
						if (fid0==fid_old0)						// 指IDが一致している場合は移動
						{
							p.x = touch.position.x;
							p.y = touch.position.y;
							p.z = 0.0f;
							on0 = true;
						}
						else
						{										// 指IDが不一致なら一旦指離し処理
							//p=new Vector3(0,0,0);
							on0=false;
						}
					}
					else if (								// そもそもタッチパネルに触れていない場合の処理
								(touch.phase==TouchPhase.Canceled)
							||	(touch.phase==TouchPhase.Ended)
							)
					{
						//p=new Vector3(0,0,0);
						on0=false;
					}
				}
					break;
				case 1:
				{
					fid_old1=fid1;
					fid1=touch.fingerId;
					if (touch.phase==TouchPhase.Began)
					{
						fid_old1=fid1;
					}
					else if (									// タッチパネルに触れている時だけ処理
							(touch.phase==TouchPhase.Began)
						||	(touch.phase==TouchPhase.Moved)
						||	(touch.phase==TouchPhase.Stationary)
						)
					{
						if (fid1==fid_old1)						// 指IDが一致している場合は移動
						{
							//p.x = touch.position.x;
							//p.y = touch.position.y;
							//p.z = 0.0f;
							on1 = true;
						}
						else
						{										// 指IDが不一致なら一旦指離し処理
							//p=new Vector3(0,0,0);
							on1=false;
						}
					}
					else if (								// そもそもタッチパネルに触れていない場合の処理
								(touch.phase==TouchPhase.Canceled)
							||	(touch.phase==TouchPhase.Ended)
							)
					{
						//p=new Vector3(0,0,0);
						on1=false;
					}
				}
				break;
			default:
				break;
			}
		}
#endif

#endif
		pos_old0 = pos0;
		pos0 = new Vector3((p.x - (Screen.width / 2)) * multi, (p.y - (Screen.height / 2)) * multi, 0);  // 画面中心を(0,0,0)とした座標軸に変換.
		if (on0 == true)
		{							// タッチされていたら移動/カーソル表示
			switch(count)
			{
				case -2:
				case -1:
				case 0:
#if false

					move = pos0 - pos_old0;
					cursor_prefab.transform.localPosition = manage.pos_myship;
					cursor.sprite = SPRITE_TOUCHPANEL[count % 8];
					cursor.transform.localPosition = manage.pos_myship;
					if (scr.flip_game==true)
					{
						Vector3 vec = cursor.transform.localPosition;
						vec.x = 0 - vec.x;
						cursor.transform.localPosition = vec;
					}
					cursor_prefab.enabled = false;
					cursor_mask.enabled = false;
					cursor.enabled = false;
#endif
					break;
				case 1:
				case 2:
				case 3:
				case 4:
				case 5:
				case 6:
				case 7:
				case 8:
					move = pos0 - pos_old0;
					//cursor_prefab.transform.localPosition = manage.pos_myship + (pos0 - manage.pos_myship) * (float)(count / 8.0f);
					//cursor.sprite = SPRITE_TOUCHPANEL[count % 8];
					//if (scr.flip_game == true)
					{
						//Vector3 vec = manage.pos_myship;
						//vec.x = 0 - vec.x;
						//cursor.transform.localPosition = vec;
						//cursor.transform.localPosition = vec + (pos0 - vec) * (float)(count / 8.0f);
					}
					//else
					{
						//cursor.transform.localPosition = manage.pos_myship + (pos0 - manage.pos_myship) * (float)(count / 8.0f);
					}
					//cursor_prefab.enabled = false;
					//cursor_mask.enabled = false;
					if (count > 1)
					{
						//cursor.enabled = false;
					}
					else
					{
						//cursor.enabled = false;
					}
					break;
				default:
					move = pos0 - pos_old0;
					//cursor.transform.localPosition = pos0;
					//cursor_prefab.transform.localPosition = pos0;
					//if (scr.flip_game==true)
					{
						//Vector3 vec = cursor_prefab.transform.localPosition;
						//vec.x = 0 - vec.x;
						//cursor_prefab.transform.localPosition = vec;
					}
					//cursor.sprite = SPRITE_TOUCHPANEL[count % 8];
					//cursor_prefab.enabled = false;
					//cursor_mask.enabled = false;
					//cursor.enabled = false;
					break;
			}
		}
		else
		{							// タッチされていなかったら停止/カーソル消去
			move = new Vector3(0, 0, 0);
			//cursor.enabled = false;
			//cursor_mask.enabled = false;
		}
		//pos_old1 = pos1;		// 2本目の指
		//pos1 = new Vector3((p2.x - (Screen.width / 2)) * multi, (p2.y - (Screen.height / 2)) * multi, 0);
		//if (on1 == true)
		{
			switch (count2)
			{
				case -2:
				case -1:
				case 0:
					move2 = pos1 - pos_old1;
					//cursor2.transform.localPosition = manage.pos_myship;
					//cursor2_prefab.transform.localPosition = manage.pos_myship;
					//if (scr.flip_game == true)
					{
						//Vector3 vec = cursor2.transform.localPosition;
						//vec.x = 0 - vec.x;
						//cursor2.transform.localPosition = vec;
					}
					//cursor2.sprite = SPRITE_TOUCHPANEL[(count2 % 8) + 8];
					//cursor2_prefab.enabled = false;
					//cursor2_mask.enabled = false;
					//cursor2.enabled = false;
					break;
				case 1:
				case 2:
				case 3:
				case 4:
				case 5:
				case 6:
				case 7:
				case 8:
					move2 = pos1 - pos_old1;
					//cursor2.transform.localPosition = manage.pos_myship + (pos1 - manage.pos_myship) * (float)(count2 / 8.0f);
					//cursor2_prefab.transform.localPosition = manage.pos_myship + (pos1 - manage.pos_myship) * (float)(count2 / 8.0f);
					//if (scr.flip_game == true)
					{
						//Vector3 vec = manage.pos_myship;
						//vec.x = 0 - vec.x;
						//cursor2.transform.localPosition = vec;
						//cursor2.transform.localPosition = vec + (pos1 - vec) * (float)(count2 / 8.0f);
					}
					//else
					{
						//cursor2.transform.localPosition = manage.pos_myship + (pos1 - manage.pos_myship) * (float)(count2 / 8.0f);
					}
					//cursor2.sprite = SPRITE_TOUCHPANEL[(count2 % 8) + 8];
					//cursor2_prefab.enabled = false;
					//cursor2_mask.enabled = false;
					if (count2 > 1)
					{
						//cursor2.enabled = false;
					}
					//else
					{
						//cursor2.enabled = false;
					}
					break;
				default:
					move2 = pos1 - pos_old1;
					
					//cursor2.transform.localPosition = pos1;
					//cursor2_prefab.transform.localPosition = pos1;
					//if (scr.flip_game == true)
					{
						//Vector3 vec = cursor2_prefab.transform.localPosition;
						//vec.x = 0 - vec.x;
						//cursor2_prefab.transform.localPosition = vec;
					}
					//cursor2.sprite = SPRITE_TOUCHPANEL[(count2 % 8) + 8];    // 2色目
					//cursor2_prefab.enabled = false;
					//cursor2_mask.enabled = false;
					//cursor2.enabled = false;
					break;
			}
		}
		//else
		{
			//cursor2.enabled = false;
			//cursor2_mask.enabled = false;
		}



		if (on0 != on_old0)
		{                           // 指が離されたら停止処理に入るため移動量リセット
			count = 0;
			on_old0 = on0;
			move = new Vector3(0, 0, 0);
		}
		else
		{                           // 指が触れたままなのでカウンタ上げるだけ
			count++;
		}
		if (on1 != on_old1)
		{
			count2 = 0;
			on_old1 = on1;
			move2 = new Vector3(0, 0, 0);
		}
		else
		{
			count2++;
		}
		move = move * movemulti;
		if (count+count2==0)	// 両方の指が離れている時
		{
			//manage.ship_restart = true;
		}


#if true
		//if (msg_debugout!=null)
		{
			DebugStation.SetText("screen=(" + Screen.width + "," + Screen.height + ")\nmulti=" + multi, true);
			DebugStation.SetText("\n\npos_old0=" + pos_old0 + " / pos0=" + pos0 + " / on0=" + on0, false);
			DebugStation.SetText("\ncount=" + count, false);
			DebugStation.SetText("\npos_old1=" + pos_old1 + " / pos1=" + pos1 + " / on1=" + on1, false);
			DebugStation.SetText("\ncount2=" + count2, false);
			DebugStation.SetText("\n\nmove=" + move, false);
			DebugStation.SetText(" fid0=" + fid0, false);
			DebugStation.SetText(" fid_old0=" + fid_old0, false);
			DebugStation.SetText(" fid1=" + fid1, false);
			DebugStation.SetText(" fid_old1=" + fid_old1, false);
			DebugStation.SetText("\nmanage.pos_myship=" + manage.pos_myship, false);
		}
#endif
	}
}
