using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
//using ObjectManager;
using static UnityEngine.GraphicsBuffer;

/*
 * 
 *	ObjectCtrl.cs
 *		オブジェクトの固有動作の管理
 * 
 * 
 * 
 * 
 * 
 *		20221211	WSc101用に再構成
 *		20240715	3Dobjを扱うために大幅書き換え
 *		
 */






public class ObjectCtrl : MonoBehaviour
{
	[Space]
	[SerializeField]
	public int OBJcnt = 0;
	[Space]
#if false

	[SerializeField]
	public GameObject MainModel;    // メイン3Dモデル
	[SerializeField]
	public BoxCollider MainHit;   // 3Dボックスヒット
	//[SerializeField]
	//GeometryWireFrameCtrl WIREFRAME_CTRL;   // 色変化コントローラー
	[SerializeField]
	public Transform ModelRoll;     // ロール(横回転)
	[SerializeField]
	public Transform ModelPitch;    // ピッチ(上下首振り)
	[SerializeField]
	public Transform ModelYaw;      // ヨー(左右首振り)
#endif

	[SerializeField]
	public SpriteRenderer MainPic;      // メイン画像.(プライオリティ前面)
	[SerializeField]
	public Transform MainPos;           // 座標・回転関連.
	[SerializeField]
	public BoxCollider2D MainHit;   // 当たり判定.
									//[SerializeField]
									//public SpriteRenderer EmotePic;     // エモートアイコン
	[Space]

	public ObjectManager.MovementMode currentMode = ObjectManager.MovementMode.PreciseStep; // 移動サブルーチン3種類から選べるようにする

	float[] cnt_frogger_jump = new float[] { 0, 5, 15, 3, 2, 0 };   // フロッガーモード1回の移動でジャンプする距離


	public int LIFE = 0;        // 耐久力.
	public bool NOHIT = false;  // 当たり判定の有無.
	[Space]
	public Vector3 scale = new Vector3(0.15f, 0.15f, 1);   // キャラクタの大きさ(スケール)・初期値は自機に合わせてある.
	public int speed = 0;                       // 移動速度.
	public int angle = 0;                       // 移動角度(360度を256段階で指定).
	public int oldangle = 0;                    // 1int前の角度
	public int group_id = 0;                    // グループID(数字が違う相手と当たり判定を取るためのもの)
	public int local_type = 0;                  // キャラクタタイプ(同じキャラクタだけど動きが違うなどの振り分け).
	public int local_mode = 0;                  // 動作モード(キャラクタによって意味が違う).
	public int power = 0;                       // 相手に与えるダメージ量.
	public int count = 0;                       // 動作カウンタ.
	public int[] flags = new int[4];            // パラメータ4個
	public Vector3[] positions = new Vector3[4]; // テンポラリ座標4個
	public Vector3 vect = Vector3.zero;         // 移動量
	public int interval = 0;                    // 0で自爆しない・任意の数値を入れるとカウント到達時に自爆

	public int ship_energy_charge = 0;

	private int fadeStep = 0;  // フェード段階カウント
	private bool isGoalSequence = false;        // trueならゴール演出中

	[Space]

	readonly Color COLOR_NORMAL = new Color(1.0f, 1.0f, 1.0f, 1.0f);
#if false
	readonly Color COLOR_DAMAGE = new Color(1.0f, 0.0f, 0.0f, 1.0f);
	readonly Color COLOR_ERASE = new Color(0.0f, 0.0f, 0.0f, 0.0f);

	static readonly float[] MOVE_LIMIT_X = new float[] { 227f, 575f, 735f };    // 壁判定・追い出し先
	static readonly float[] MOVE_LIMIT_Y = new float[] { 39f, 127f, 227f };
	static readonly float[] VECT_LIMIT_X = new float[] { 16f, -16f, 16f };  // 壁判定・チェック用ベクトル
	static readonly float[] VECT_LIMIT_Y = new float[] { 16f, 16f, 16f };

	const float SHIP_MOVE_SPEED = 0.10f;
	const float WATER_MOVE_SPEED = 0.54f;

	const float WATER_PERCENTAGE = 0.001f;

	const float OFFSCREEN_MIN_X = -6.00f;
	const float OFFSCREEN_MAX_X = 6.00f;
	const float OFFSCREEN_MIN_Y = -4.00f;
	const float OFFSCREEN_MAX_Y = 4.00f;

	const float HITSIZE_MYSHIP = 0.16f;
	const float HITSIZE_MYSHOT = 0.64f;
	const float HITSIZE_ENEMY = 0.32f;
#endif
	public Vector3 myinp;

	public ObjectManager.MODE obj_mode = ObjectManager.MODE.NOUSE;  // キャラクタの管理状態.
	public ObjectManager.TYPE obj_type = ObjectManager.TYPE.NOUSE;  // キャラクタの分類(当たり判定時に必要).

	MainSceneCtrl MAIN;
	ObjectManager MANAGE;



	void Awake()
	{
		MAIN = GameObject.Find("root_game").GetComponent<MainSceneCtrl>();
		MANAGE = GameObject.Find("root_game").GetComponent<ObjectManager>();
	}






	// Use this for initialization
	void Start()
	{
		for (int i = 0; i < flags.Length; i++)
		{
			flags[i] = 0;
			positions[i] = new Vector3(0, 0, 0);
		}
	}

	private static int cnt_fadeout = 0;
#if false

	bool FadeAndEnd()
	{
		if (cnt_fadeout >= 60)
		{
			cnt_fadeout = 0;
			this.transform.localPosition = new Vector3(0, -220, 0);
			MainPic.color = new Color(1, 1, 1, 1);
		}
		else
		{
			MainPic.color = new Color(1, 1, 1, ((float)cnt_fadeout / 60.0f) - 1.0f);
			if (cnt_fadeout == 0)
			{
				SoundManager.Instance.StopSE();
				SoundManager.Instance.PlaySE(2);
				MainPic.color = new Color(1, 1, 1, 1);
			}
		}
		cnt_fadeout++;
		return (cnt_fadeout >= 60);
	}
#endif

	// Update is called once per frame
	void Update()
	{
		if (obj_mode == ObjectManager.MODE.NOUSE)
		{
			return;
		}

		// ★ゲームオーバー演出中のEarly Returnは、count++を殺さないように
		// 各オブジェクトの挙動（通常移動）の直前で弾くように修正します。

		if (ModeManager.mode == ModeManager.MODE.DEMO)
		{
			return;
		}

		switch (obj_mode)
		{
			case ObjectManager.MODE.NOUSE:
				return;
			case ObjectManager.MODE.INIT:
				count = 0;
				LIFE = 1;
				obj_mode = ObjectManager.MODE.NOHIT;
				break;
			case ObjectManager.MODE.HIT:
				MainHit.enabled = true;
				break;
			case ObjectManager.MODE.NOHIT:
				MainHit.enabled = false;
				break;
			case ObjectManager.MODE.FINISH:
				MANAGE.Return(this);
				return; // 返却したならこれ以上処理しない
		}

		// ★キレイに整理した運命の switch 文
		switch (obj_type)
		{
			// ==========================================
			// 自機（MYSHIP）の処理
			// ==========================================
			case ObjectManager.TYPE.MYSHIP:
				{
					switch (count)
					{
						case 0: // --- 初期化 ---
							obj_mode = ObjectManager.MODE.HIT;
							NOHIT = false;
							MainHit.offset = new Vector2(0, -350);
							MainHit.size = new Vector3(300, 30, 1);
							MainHit.enabled = true;
							MainPic.sprite = MANAGE.SPR_MYSHIP[0];
							MainPic.enabled = true;
							MainPic.color = new Color(1, 1, 1, 1);
							angle = 0;
							flags[0] = 0;
							flags[1] = 0;
							flags[2] = 0;
							flags[3] = 0;
							local_mode = 0;
							power = 1;
							LIFE = 1;
							this.transform.localPosition = new Vector3(0, -220, 0);
							this.transform.localScale = new Vector3(0.15f, 0.15f, 1);
							break;

						default: // --- 通常更新 ---
							{
								if (MAIN.cnt_game_over >= 0)    // ゲームオーバーカウンターが0以上
								{
									if (MAIN.cnt_game_over % 4 == 0)    // 0,1,2,3,0,1... 4フレームごと
									{
										MainPic.enabled = !MainPic.enabled; // 表示反転
									}
									DebugStation.SetText($"\n<color=red>GAME OVER:　MAIN.cnt_game_over={MAIN.cnt_game_over}</color>", false);
									return; // ゲームオーバーフラグの立っている時は移動させない
								}

								// ゴール到達チェック
								if (!isGoalSequence && this.transform.localPosition.y >= 100)
								{
									isGoalSequence = true;
									fadeStep = 0;
									flags[3] = 1;   // ゴールフラグ(これが1の間はゴール演出中)
									SoundManager.Instance.StopSE();
									SoundManager.Instance.PlaySE(2);
									this.transform.localPosition = new Vector3(0, 100, 0);
								}

								// ゴール演出進行中
								if (isGoalSequence)
								{
									if (fadeStep <= 60)
									{
										float alpha = 1.0f - (fadeStep / 60f);
										MainPic.color = new Color(1, 1, 1, alpha);
										fadeStep++;
										return;    // 演出中はこれ以上の処理をしない（入力も受け付けない）
									}
									else
									{
										MAIN.cnt_score++;
										isGoalSequence = false;
										fadeStep = 0;
										MainPic.color = new Color(1, 1, 1, 1);
										this.transform.localPosition = new Vector3(0, -220, 0);
										flags[3] = 0;   // ゴールフラグリセット
									}
									flags[0] = 0;
									flags[1] = 0;

									return; // 演出中はこれ以上の処理をしない（入力も受け付けない）
									break;
								}
							}
							// 入力とモード切り替え
							if (Input.GetKeyDown(KeyCode.Alpha1)) currentMode = ObjectManager.MovementMode.PreciseStep;
							if (Input.GetKeyDown(KeyCode.Alpha2)) currentMode = ObjectManager.MovementMode.FroggerSnap;
							if (Input.GetKeyDown(KeyCode.Alpha3)) currentMode = ObjectManager.MovementMode.FreeRun;

							float input = Input.GetAxisRaw("Vertical");
							int sign = (input > 0.4f) ? 1 : (input < -0.4f) ? -1 : 0;   // 入力から進行方向を作成
							if (Mathf.Abs(sign) == 0) positions[0].x = 0;	// 入力がないときはsign=0にして、FroggerSnapモードの移動完了後の入力待ち状態を作るためのフラグもリセット
                            if (positions[0].x >= 0.1f) sign = 0;			// FroggerSnapモードで移動完了後、次の入力があるまで動けないようにするフラグをチェックして、必要なら入力を無効化

                            // 移動ロジック
                            switch (currentMode)
							{
								case ObjectManager.MovementMode.PreciseStep:	// 仕切りアリ・押しっぱなしで移動し続ける
									{
										if (flags[1] == 0 && sign != 0)
										{
											if (this.transform.localPosition.y <= -220f && sign < 0) break;

											float? nextY = MANAGE.VerticalStopPoint.GetNextTargetY(transform.localPosition.y, sign);
											if (nextY != null)
											{
												positions[1] = new Vector3(0, nextY.Value, 0);
												flags[0] = sign;
												flags[1] = 1;
											}
										}
										if (flags[1] == 1)
										{
											float speed = 8.0f * flags[0];
											Vector3 nextPos = transform.localPosition + new Vector3(0, speed, 0);

											if ((flags[0] > 0 && nextPos.y >= positions[1].y) ||
												(flags[0] < 0 && nextPos.y <= positions[1].y))
											{
												this.transform.localPosition = new Vector3(0, positions[1].y, 0);
												flags[1] = 0;
												SoundManager.Instance.PlaySE(0);
											}
											else
											{
												this.transform.localPosition = nextPos;
											}
										}
									}
									break;

								case ObjectManager.MovementMode.FroggerSnap:    // 仕切りアリ・押したら一度離さないと再移動できない(フロッガー的な挙動)
                                    {
										if (flags[1] == 0 && sign != 0) // 止まっている時に、かつ上下どちらかの入力があるとき
                                        {
											if (this.transform.localPosition.y <= -220f && sign < 0) break;	// 真下移動を制限

											float? nextY = MANAGE.VerticalStopPoint.GetNextTargetY(transform.localPosition.y, sign);
											if (nextY != null)	// VerticalStopPointから移動先を拾ってきて、存在した場合に移動
											{
												positions[1] = new Vector3(0, nextY.Value, 0);
												flags[0] = sign;
												flags[1] = 1;
											}
										}
										if (flags[1] == 1)
										{
											float now_speed = 8.0f * flags[0];
											Vector3 nextPos = transform.localPosition + new Vector3(0, now_speed, 0);

											if ((flags[0] > 0 && nextPos.y >= positions[1].y) ||
												(flags[0] < 0 && nextPos.y <= positions[1].y))
											{
												this.transform.localPosition = new Vector3(0, positions[1].y, 0);
												flags[1] = 0;
												positions[0].x = 1;	// 移動できなくする(押し直ししないと動けなくする)
												SoundManager.Instance.PlaySE(0);
											}
											else
											{
												this.transform.localPosition = nextPos;
											}
                                        }
									}
									break;

								case ObjectManager.MovementMode.FreeRun:	// 仕切りナシ・どこでも止まれる
									{
										if (sign != 0)
										{
											this.transform.localPosition += new Vector3(0, 5.0f * sign, 0);
										}
									}
									break;

							}

							// 自機専用のリミッタガード
							float finalY = this.transform.localPosition.y;
							if (finalY <= -220f)
							{
								this.transform.localPosition = new Vector3(0, -220f, 0);
								flags[0] = 0;
								flags[1] = 0;
							}
							break;
					}

					// 自機だけの座標報告
					MANAGE.pos_myship = this.transform.localPosition;
					DebugStation.SetText($"\n\n<color=LIMEGREEN>自機確定: pos={this.transform.localPosition} / Mode={currentMode}</color>", false);
					DebugStation.SetText($"\n\nFroggerSnap:flags[0]={flags[0]} / flags[1]={flags[1]} / positions[0].x={positions[0].x}", false);
				} 
                break; // ★自機ケースの完全な終了

            // ==========================================
            // 敵（TANK）の処理
            // ==========================================
            case ObjectManager.TYPE.TANK:
                {
                    // ゲームオーバー演出中の Early Return 代わり
                    if (MAIN.cnt_game_over >= 0 && MAIN.cnt_game_over < 40)
                    {
                        break;
                    }

                    if (count == 0)
                    {
                        obj_mode = ObjectManager.MODE.HIT;
                        MainPic.sprite = MANAGE.SPR_ENEMY[0];
                        MainPic.enabled = true;
                        MainHit.size = new Vector2(500, 200);
                        MainHit.offset = new Vector2(0, -100);
                        MainHit.enabled = true;
                        this.transform.localScale = new Vector3(0.15f, 0.15f, 1);
                        NOHIT = false;
                        positions[0] = new Vector3(speed * 0.25f, 0, 0);

                        switch (local_mode)
                        {
                            case 1:
                                positions[0].x = positions[0].x * -1;
                                MainPic.flipX = false;
                                this.transform.localPosition = new Vector3(449, -180, 0);
                                positions[0].y = -180.0f;
                                MainPic.sortingOrder = -1;
                                break;
                            case 2:
                                positions[0].x = positions[0].x * 1;
                                MainPic.flipX = true;
                                MainPic.sortingOrder = -3;
                                this.transform.localPosition = new Vector3(-449, -130, 0);
                                positions[0].y = -130.0f;
                                break;
                            case 3:
                                positions[0].x = positions[0].x * -1;
                                MainPic.flipX = false;
                                this.transform.localPosition = new Vector3(449, -80, 0);
                                positions[0].y = -80.0f;
                                MainPic.sortingOrder = -5;
                                break;
                            case 4:
                                positions[0].x = positions[0].x * 1;
                                MainPic.flipX = true;
                                this.transform.localPosition = new Vector3(-449, -30, 0);
                                positions[0].y = -30.0f;
                                MainPic.sortingOrder = -7;
                                break;
                        }
                        local_mode = 1;
                        power = 1;
                        LIFE = 1;
                    }
                    else
                    {
                        positions[3] = this.transform.localPosition;
                        positions[3].y = positions[0].y;
                        positions[3].x += positions[0].x;
                        this.transform.localPosition = positions[3];

                        if (Mathf.Abs(this.transform.localPosition.x) > 500)
                        {
                            MANAGE.Return(this);
                        }
                    }
                }
                break; // ★戦車ケースの完全な終了

            // ==========================================
            // 爆風エフェクト（NOHIT_EFFECT）の処理
            // ==========================================
            case ObjectManager.TYPE.NOHIT_EFFECT:
                {
                    if (count == 0)
                    {
                        obj_mode = ObjectManager.MODE.NOHIT;
                        MainHit.enabled = false;
                        MainPic.enabled = true;
                        LIFE = 1;
                        this.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                        flags[3] = 32;
                        MainPic.sprite = MANAGE.SPR_CRUSH[flags[0]];
                        MainPic.sortingOrder = 5;
                    }
                    else if (count >= flags[3])
                    {
                        MANAGE.Return(this);
                    }
                    else
                    {
                        this.transform.localScale += new Vector3(0.05f, 0.05f, 0.05f);
                        MainPic.color = new Color(1, 1, 1, (float)flags[3] / (float)(flags[3] - count));
                    }
                }
                break; // ★エフェクトケースの完全な終了
        }

        if (LIFE <= 0)
        {
            Dead();
        }


        // 全オブジェクト共通で、安全にカウントを進める
        count++;
    }
#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
        // 1. 既存の文字情報を表示
        UnityEditor.Handles.Label(this.transform.position, $"T={obj_type}\nM={obj_mode}\nC={count}");

        // 2. コライダーの補助線を描画
        if (MainHit != null && MainHit.enabled)
        {
            // Gizmosの色をレトロゲームのデバッグ画面風の「黄緑色」にする
            Gizmos.color = Color.green;

            // BoxCollider2D の設定値を取得
            Vector3 center = new Vector3(MainHit.offset.x, MainHit.offset.y, 0);
            Vector3 size = new Vector3(MainHit.size.x, MainHit.size.y, 1);

            // オブジェクトの Scale や Position を加味した行列を Gizmos に適用
            Gizmos.matrix = this.transform.localToWorldMatrix;

            // ワイヤーフレーム（線だけ）の立方体を描画
            Gizmos.DrawWireCube(center, size);

			
        }
    }
#endif
















#if false  // 旧いUpdateコード。一旦封印。                       
	// Update is called once per frame
	void Update()
	{
		Vector3 pos = Vector3.zero;

		if (obj_mode == ObjectManager.MODE.NOUSE)
		{
			return;
		}
#if false
		if (ModeManager.now_mode == ModeManager.MODE.GAME_PAUSE)
		{
			return;
		}
#endif
		if (
				(MAIN.cnt_game_over >= 0)   // 当たった瞬間を強調
			&& (MAIN.cnt_game_over < 40)
			)
		{
			return;
		}
		else if (ModeManager.mode == ModeManager.MODE.DEMO)
		{
			return;
		}
		switch (obj_mode)
		{
			case ObjectManager.MODE.NOUSE:
				return;
			case ObjectManager.MODE.INIT:
				//MainPic.enabled = true;
				count = 0;
				LIFE = 1;
				obj_mode = ObjectManager.MODE.NOHIT;
				break;
			case ObjectManager.MODE.HIT:
				MainHit.enabled = true;
				//MainHit.enabled = true;
				break;
			case ObjectManager.MODE.NOHIT:
				MainHit.enabled = false;
				//MainHit.enabled = false;
				break;
			case ObjectManager.MODE.FINISH:
				MANAGE.Return(this);
				break;
		}

		switch (obj_type)
		{
            // 自機
            case ObjectManager.TYPE.MYSHIP:
				{
					switch (count)  // 自機の状態（0=初期化、1以上=通常）
					{
						case 0: // --- 初期化 ---
							{
								obj_mode = ObjectManager.MODE.HIT;
								obj_type = ObjectManager.TYPE.MYSHIP;
								NOHIT = false;
								MainHit.offset = new Vector2(0, -350);
								MainHit.size = new Vector3(300, 50, 1);
								MainHit.enabled = true;
								MainPic.sprite = MANAGE.SPR_MYSHIP[0];  // ほんものの自機スプライトをセット
								MainPic.enabled = true;
								MainPic.color = new Color(1, 1, 1, 1);
								angle = 0;
								flags[0] = 0;   // 方向 (-1, 0, 1)
								flags[1] = 0;   // 0:停止中, 1:移動中
								flags[2] = 0;   // 足音用カウンター
								flags[3] = 0;   // 停止中カウンター
								local_mode = 0;
								power = 1;
								LIFE = 1;

								// 最初からスタート位置に配置
								this.transform.localPosition = new Vector3(0, -220, 0);
								this.transform.localScale = new Vector3(0.15f, 0.15f, 1);
							}
							break;

						default: // --- 通常（1フレームごとの更新） ---
							{
								// 1. ゲームオーバー演出中
								if (MAIN.cnt_game_over >= 0)
								{
									if (Time.frameCount % 4 == 0)
									{
										MainPic.enabled = !MainPic.enabled;
									}
									break; // これ以降の移動・入力処理をスキップ
								}

								// 2. ゴール到達チェック
								if (!isGoalSequence && this.transform.localPosition.y >= 100)
								{
									isGoalSequence = true;
									fadeStep = 0;
									SoundManager.Instance.StopSE();
									SoundManager.Instance.PlaySE(2); // ゴールSE
									this.transform.localPosition = new Vector3(0, 100, 0);
									this.transform.localScale = new Vector3(0.15f, 0.15f, 1);

								}

								// 3. ゴール演出進行中
								if (isGoalSequence)
								{
									if (fadeStep <= 60)
									{
										float alpha = 1.0f - (fadeStep / 60f);
										MainPic.color = new Color(1, 1, 1, alpha);
										fadeStep++;
									}
									else
									{
										// 演出完了: リセット
										MAIN.cnt_score++;
										isGoalSequence = false;
										fadeStep = 0;
										MainPic.color = new Color(1, 1, 1, 1);
										this.transform.localPosition = new Vector3(0, -220, 0);
										this.transform.localScale = new Vector3(0.15f, 0.15f, 1);
									}
									flags[0] = 0; // 演出中は動かないようにフラグも安全にリセット
									flags[1] = 0;
									break; // 演出中は入力を受け付けない
								}

								// 4. 入力と移動モードの切り替え
								if (Input.GetKeyDown(KeyCode.Alpha1)) currentMode = ObjectManager.MovementMode.PreciseStep;
								if (Input.GetKeyDown(KeyCode.Alpha2)) currentMode = ObjectManager.MovementMode.FroggerSnap;
								if (Input.GetKeyDown(KeyCode.Alpha3)) currentMode = ObjectManager.MovementMode.FreeRun;

								float input = Input.GetAxisRaw("Vertical");
								int sign = (input > 0.4f) ? 1 : (input < -0.4f) ? -1 : 0;
								DebugStation.SetText($"\n\n<color=ORANGE>ObjectCtrl.Update() input={input} sign={sign}</color>", false);

								// 5. 各モードの移動ロジック
								switch (currentMode)
								{
									case ObjectManager.MovementMode.PreciseStep:
										// 目的地をセット（止まっている時のみ）
										if (flags[1] == 0 && sign != 0)
										{
											// 下端にいるのに下を押した場合は無視する
											if (this.transform.localPosition.y <= -220f && sign < 0) return;

											float? nextY = MANAGE.VerticalStopPoint.GetNextTargetY(transform.localPosition.y, sign);
											if (nextY != null)
											{
												positions[1] = new Vector3(0, nextY.Value, 0); // 目的地
												flags[0] = sign; // 移動方向
												flags[1] = 1;    // 移動フラグON
											}
										}

										// 移動実行
										if (flags[1] == 1)
										{
											float speed = 8.0f * flags[0];
											Vector3 nextPos = transform.localPosition + new Vector3(0, speed, 0);

											// 追い越しチェック
											if ((flags[0] > 0 && nextPos.y >= positions[1].y) ||
												(flags[0] < 0 && nextPos.y <= positions[1].y))
											{
												this.transform.localPosition = new Vector3(0, positions[1].y, 0);
												flags[1] = 0; // 停止
												SoundManager.Instance.PlaySE(0); // 足音
											}
											else
											{
												this.transform.localPosition = nextPos;
											}
										}
										break;

									case ObjectManager.MovementMode.FreeRun:
										// GetAxisRaw のおかげで、指を離せば確実に 0 になる！
										if (sign != 0)
										{
											// 押した方向にシンプルに進む（例：速度 5.0f）
											this.transform.localPosition += new Vector3(0, 5.0f * sign, 0);

											// 足音の処理...
										}
										break;

									case ObjectManager.MovementMode.FroggerSnap:
										// 必要な場合はここにフロッガーのロジックを
										break;
								}

								// --- [移動ロジックのswitch文] が終わった直後、かつ [報告] の手前に置く ---

								float finalY = this.transform.localPosition.y;

								// 下限ガード（-220より下に行こうとしたら強制固定）
								if (finalY <= -220f)
								{
									this.transform.localPosition = new Vector3(0, -220f, 0);
									// 下端にいるときは、移動フラグや方向フラグも念のため安全にリセット
									flags[0] = 0;
									flags[1] = 0;
								}

								// 上限ガード（ゴールに触れたら、演出中でなければゴールシーケンスへ）
								if (finalY >= 100f && !isGoalSequence)
								{
									isGoalSequence = true;
									fadeStep = 0;
									SoundManager.Instance.StopSE();
									SoundManager.Instance.PlaySE(2); // ゴールSE
									this.transform.localPosition = new Vector3(0, 100f, 0); // ピタッと補正
								}
							}
							break;
					}

					// 座標をマネージャーに報告
					MANAGE.pos_myship = this.transform.localPosition;
					DebugStation.SetText($"\n\n<color=LIMEGREEN>ObjectCtrl.Update().case {obj_type}: count={count} / currentMode={currentMode} / pos={this.transform.localPosition} / flags[0]={flags[0]} / flags[1]={flags[1]}</color>", false);
				}
				break;










            /*
			 *	敵1
			 * 
			 *		決められた道路の上を、決められた速度で走る
			 *		左右及びスピード段階で動きが変わる
			 *		逆に言えば、それ以外はすべて同一
			 * 
			 * 
			 * 
			 * 
			 * 
			 * 
			 * 
			 */
            case ObjectManager.TYPE.TANK:
				{
					if (count == 0)
					{
						obj_mode = ObjectManager.MODE.HIT;
						obj_type = ObjectManager.TYPE.TANK;
						MainPic.sprite = MANAGE.SPR_ENEMY[0];
						MainPic.enabled = true;
						MainHit.size = new Vector2(500, 200);
						MainHit.offset = new Vector2(0, -100);
						MainHit.enabled = true;
						this.transform.localScale = new Vector3(0.15f, 0.15f, 1);
						NOHIT = false;
						positions[0] = new Vector3(speed * 0.75f, 0, 0);
						//Debug.Log("戦車；ID=" + this.OBJcnt + " / local_mode=" + local_mode);
						switch (local_mode) // 戦車の移動レーン
						{
							case 1:
								positions[0].x = positions[0].x * -1;
								MainPic.flipX = false;
								this.transform.localPosition = new Vector3(449, -180, 0);
								positions[0].y = -180.0f;
								MainPic.sortingOrder = -1;
								break;
							case 2:
								positions[0].x = positions[0].x * 1;
								MainPic.flipX = true;
								MainPic.sortingOrder = -3;
								this.transform.localPosition = new Vector3(-449, -130, 0);
								positions[0].y = -130.0f;
								break;
							case 3:
								positions[0].x = positions[0].x * -1;
								MainPic.flipX = false;
								this.transform.localPosition = new Vector3(449, -80, 0);
								positions[0].y = -80.0f;
								MainPic.sortingOrder = -5;
								break;
							case 4:
								positions[0].x = positions[0].x * 1;
								MainPic.flipX = true;
								this.transform.localPosition = new Vector3(-449, -30, 0);
								positions[0].y = -30.0f;
								MainPic.sortingOrder = -7;
								break;
						}
						//Debug.Log("座標確定：ID=" + OBJcnt + " / local_mode=" + local_mode + " / スタート座標=" + this.transform.localPosition);
						flags[0] = 0;
						flags[1] = 0;
						flags[2] = 0;
						flags[3] = 0;
						positions[1] = new Vector3(0, 0, 0);
						positions[2] = new Vector3(0, 0, 0);
						positions[3] = new Vector3(0, 0, 0);
						local_mode = 1; // 
						power = 1;
						LIFE = 1;

					}
					else
					{
						positions[3] = this.transform.localPosition;    // 戦車移動を安定化させたい(具体的にはちょっと下にブレるのを止めたい)
						positions[3].y = positions[0].y;
						positions[3].x += positions[0].x;
						this.transform.localPosition = positions[3];
						//Debug.Log("走行中：ID=" + OBJcnt + " / positions[0].x=" + positions[0].x + " / positions[0].y="+positions[0].y+" / positions[3]=" + positions[3]);
						if (Mathf.Abs(this.transform.localPosition.x) > 500)
						{
							//Debug.Log("消滅；ID=" + OBJcnt + " / local_mode=" + local_mode + " / 座標=" + this.transform.localPosition);
							MANAGE.Return(this);
						}
					}


				}
				break;	
		
		
				
		
	





			/******************************************************
			 * 
			 * 
			 * 
			 * 
			 * ここからエフェクトなど
			 * 
			 * 
			 *
			 ******************************************************
			 */
			case ObjectManager.TYPE.NOHIT_EFFECT:
				{
					if (count == 0)
					{
						obj_mode = ObjectManager.MODE.NOHIT;
						MainHit.enabled = false;
						MainPic.enabled = true;
						LIFE = 1;
						this.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
						flags[3] = 32;
						MainPic.sprite = MANAGE.SPR_CRUSH[flags[0]];
						MainPic.sortingOrder = 5;
					}
					else if (count >= flags[3])
					{
						MANAGE.Return(this);
					}
					else
					{
						this.transform.localScale += new Vector3(0.05f, 0.05f, 0.05f);
						MainPic.color = new Color(1, 1, 1, (float)flags[3] / (float)(flags[3] - count));
					}
				}
					break;
		}

		// 自前衝突判定を使う場合
		//MANAGE.CheckHit(this);

		if (LIFE <= 0)  // 死亡確認
		{
			Dead();
		}

		count++;
	}



#endif








    /// <summary>
    /// 当たり判定部・スプライト同士が衝突した時に走る
    /// </summary>
    /// <param name="collider">衝突したスプライト当たり情報</param>
    void OnTriggerEnter2D(Collider2D collider)
	{
		if (obj_mode == ObjectManager.MODE.NOHIT)
		{
			return;
		}
		ObjectCtrl other = collider.gameObject.GetComponent<ObjectCtrl>();
		//Debug.Log(("【衝突】count=" + count + " / OnTriggerEnter:this=" + this.OBJcnt + " / other=" + other.OBJcnt));
		if (other == null)
		{
			return;
		}
		if (other.obj_mode == ObjectManager.MODE.NOHIT)
		{
			return;
		}
		if (NOHIT == true)
		{
			return;
		}
		if (other.NOHIT == true)
		{
			return;
		}
		if (other.obj_type == this.obj_type)    // 戦車vs戦車しかあり得ないので弾く
		{
			//Debug.Log("TANK対TANK発生！");
			return;
		}
		if (
				(this.obj_type == ObjectManager.TYPE.TANK)
			&& (other.obj_type == ObjectManager.TYPE.MYSHIP)
			)
		{
			//Debug.Log("戦車対人！this.obj_type="+this.obj_type+" / other.obj_type="+other.obj_type);
			// ここから先は「戦車vs人」なので、存分に演出を描く
			if (MAIN.cnt_game_over == -1)
			{
				MAIN.cnt_game_over = 0; //MAINのゲームオーバーカウントスタート
				MANAGE.Set((int)ObjectManager.TYPE.NOHIT_EFFECT, 0, 0, this.transform.localPosition, new Vector3(0, 0, 0), 0, 0);
			}
		}
	}


	/// <summary>
	/// ダメージ与える
	/// </summary>
	/// <param name="damage">ダメージ量</param>
	public void Damage(int damage)
	{
		LIFE -= damage;
		if (LIFE <= 0)
		{
			Dead();	// リプレイある時はダメージ関数で死亡処理を行わない
		}
	}

	/// <summary>
	///		死んだ時の処理全般
	/// </summary>
	public void Dead()
	{
		obj_mode = ObjectManager.MODE.NOHIT;
		switch (obj_type)
		{
			default:
				break;
		}
		//MainPic.color = COLOR_NORMAL;
		count = 0;
		MANAGE.Return(this);
	}


	public void DisplayOff()
	{
		//MainModel.gameObject.SetActive(false);
		MainHit.gameObject.SetActive(false);
		MainPic.enabled = false;
		//MainHit.enabled = false;
		MainPic.color = COLOR_NORMAL;
		MainPic.sprite = null;
	}
}


