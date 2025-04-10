using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

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




public class FloorInteraction:MonoBehaviour
{
	public Vector3 Position;
	public Vector3 EulerAngles;
	public Vector3 FloorNormal;
	public Vector3 SlopeDirection;
	public float CurrentAngle;

	public int playerOnFloor;										// 追加・変更
	public Transform floorTransform;                            // 追加2
	public GameObject floorObject;								// 追加3

	public FloorInteraction(Transform floor)
	{
		Position = new Vector3(0, 0, 0);
		EulerAngles = new Vector3(0, 0, 0);
		FloorNormal = Quaternion.Euler(EulerAngles) * Vector3.up;
		SlopeDirection = Quaternion.Euler(EulerAngles) * Vector3.forward;
		CurrentAngle = Vector3.Angle(Vector3.up, FloorNormal);
		playerOnFloor = 0;
		floorTransform = floor;
	}

	public FloorInteraction(Vector3 position, Vector3 rotation, Vector3 floorNormal, Vector3 slopeDirection,Transform floor)
	{
		Position = position;
		EulerAngles = rotation;
		FloorNormal = floorNormal;
		SlopeDirection = slopeDirection;
		CurrentAngle = Vector3.Angle(Vector3.up, floorNormal);
		playerOnFloor = 0;
		floorTransform = floor;
	}

	public FloorInteraction(Vector3 position, Vector3 rotation,Transform floor)
	{
		Position = position;
		EulerAngles = rotation;
		FloorNormal = Quaternion.Euler(rotation) * Vector3.up;
		SlopeDirection = Quaternion.Euler(rotation) * Vector3.forward;
		CurrentAngle = Vector3.Angle(Vector3.up, FloorNormal);
		playerOnFloor = 0;
		floorTransform = floor;
	}

	public FloorInteraction(Vector3 position, Vector3 rotation,float currentAngle,Transform floor)
	{
		Position = position;
		EulerAngles = rotation;
		FloorNormal = Quaternion.Euler(rotation) * Vector3.up;
		SlopeDirection = Quaternion.Euler(rotation) * Vector3.forward;
		CurrentAngle = currentAngle;
		playerOnFloor = 0;
		floorTransform = floor;
	}

}


#if false
public class ParamContainer
{
	public int[] Flags = new int[4];      // 状態やフラグ
	public Vector3[] Positions = new Vector3[4]; // ベクトルデータ

	public ParamContainer()
	{
		for(int i = 0; i < Flags.Length;i++)
		{
			Flags[i] = 0;
		}
		for(int i = 0;i<Positions.Length;i++)
		{
			Positions[i] = Vector3.zero;
		}
	}
}
#endif



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

	public int LIFE = 0;        // 耐久力.
	public bool NOHIT = false;  // 当たり判定の有無.
	[Space]
	public Vector3 target = new Vector3(0, 0, 0);
	public int speed = 0;                       // 移動速度.
	public int angle = 0;                       // 移動角度(360度を256段階で指定).
	public int oldangle = 0;                    // 1int前の角度
	public int group_id = 0;					// グループID(数字が違う相手と当たり判定を取るためのもの)
	public int local_type = 0;                  // キャラクタタイプ(同じキャラクタだけど動きが違うなどの振り分け).
	public int local_mode = 0;                  // 動作モード(キャラクタによって意味が違う).
	public int power = 0;                       // 相手に与えるダメージ量.
	public int count = 0;                       // 動作カウンタ.
	public int[] flags = new int[4];            // パラメータ4個
	public Vector3[] positions = new Vector3[4]; // テンポラリ座標4個
	public Vector3 vect = Vector3.zero;         // 移動量
	public int interval = 0;                    // 0で自爆しない・任意の数値を入れるとカウント到達時に自爆

	//public ParamContainer param_flags = new ParamContainer();       // 旧param/parampos

	public Transform floorTransform;
	public FloorInteraction currentFloor;

	public bool isMovingToNewFloor = false;

	public float deg_forward = 0;   // 前後移動中に傾きがあった(0=傾きがない・それ以外=次のフロアとの角度差)
	public float deg_right = 0;		// 左右移動中に傾きがあった

	public int ship_energy_charge = 0;



	[Space]

	readonly Color COLOR_NORMAL = new Color(1.0f, 1.0f, 1.0f, 1.0f);
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

	public Vector3 myinp;

	public ObjectManager.MODE obj_mode = ObjectManager.MODE.NOUSE;  // キャラクタの管理状態.
	public ObjectManager.TYPE obj_type = ObjectManager.TYPE.NOUSE;  // キャラクタの分類(当たり判定時に必要).

	MainSceneCtrl MAIN;
	ObjectManager MANAGE;
	//CameraCtrl CAMERA;
	//FuelCtrl ENERGY;

	public FloorInteraction Now_floor { get => currentFloor; set => currentFloor = value; }
	//public FloorInteraction Now_floor1 { get => currentFloor; set => currentFloor = value; }

	void Awake()
	{
		//floorTransform = this.transform;
		//currentFloor = new FloorInteraction(new Vector3(0, 1, 0), new Vector3(0, 0, 0), floorTransform);
		//Now_floor = currentFloor;

		MAIN = GameObject.Find("root_game").GetComponent<MainSceneCtrl>();
		MANAGE = GameObject.Find("root_game").GetComponent<ObjectManager>();
		//CAMERA = GameObject.Find("Cameras").GetComponent<CameraCtrl>();
		//ENERGY = GameObject.Find("root_fuel").GetComponent<FuelCtrl>();
		//MainHit.enabled = false;
	}

	private bool AdjustRotationToNewFloor()
	{
		if (currentFloor == null || Now_floor == null) return false;  // 念のため
		FileOutput.Log("AdjustRotationToNewFloor:角度のあるフロアタイルに乗った:OBJcnt="+OBJcnt);

		float currentAngleX = transform.localEulerAngles.x;
		float targetAngleX = 0; // Now_floor.EulerAngles.x;
		float angleDiffX = Mathf.DeltaAngle(currentAngleX, targetAngleX); // / 2;	// 自前の角度差と目標の角度差を半分にする

		float currentAngleZ = transform.localEulerAngles.z;
		float targetAngleZ = 0; // Now_floor.EulerAngles.z;
		float angleDiffZ = Mathf.DeltaAngle(currentAngleZ, targetAngleZ); // / 2;   // 自前の角度差と目標の角度差を半分にする

		FileOutput.Log("現在の自機の角度と目標の角度の差を求める：angleDiffZ=" + angleDiffZ);

		bool ret = false;

		// X軸回転 or Z軸回転のどちらかを判定
		if (Mathf.Abs(angleDiffX) > 0)  // X軸回転(前後傾き)
		{
			FileOutput.Log("X軸(左右)の回転：this.transform.localEulerAngles.x=" + this.transform.localEulerAngles.x);

			if (Mathf.Abs(angleDiffX) > 5)
			{
				transform.localEulerAngles = new Vector3(
					currentAngleX + Mathf.Sign(angleDiffX) * 5,  // 5°ずつ補正
					transform.localEulerAngles.y,
					transform.localEulerAngles.z
				);
				FileOutput.Log("回転後の角度=" + this.transform.localEulerAngles);
			}
			else
			{
				Now_floor.EulerAngles.x = 0;
				transform.localEulerAngles = new Vector3(
					targetAngleX,  // ぴったり合わせる
					transform.localEulerAngles.y,
					transform.localEulerAngles.z
				);
				FileOutput.Log("ピッタリ！回転後の角度=" + this.transform.localEulerAngles);
				ret = true;
			}
		}
		else if (Mathf.Abs(angleDiffZ) > 0)  // Z軸回転(左右傾き)
		{
			FileOutput.Log("Z軸(前後)の回転：this.transform.localEulerAngles.z=" + this.transform.localEulerAngles.z);
			if (Mathf.Abs(angleDiffZ) > 5)
			{
				transform.localEulerAngles = new Vector3(
					transform.localEulerAngles.x,
					transform.localEulerAngles.y,
					currentAngleZ + Mathf.Sign(angleDiffZ) * 5  // 5°ずつ補正
				);
				//FileOutput.Log("回転後の角度=" + this.transform.localEulerAngles);
			}
			else
			{
				Now_floor.EulerAngles.z = 0;
				transform.localEulerAngles = new Vector3(
					transform.localEulerAngles.x,
					transform.localEulerAngles.y,
					targetAngleZ  // ぴったり合わせる
				);
				FileOutput.Log("ピッタリ！回転後の角度=" + this.transform.localEulerAngles);
				ret = true;
			}
		}
		FileOutput.Log("★★★最終的な角度:OBJcnt=" + OBJcnt + " / this.transform.localEulerAngles=" + this.transform.localEulerAngles);
		FileOutput.Log("角度一致チェック=" + ret);
		return ret;	// ピッタリ合った場合true
	}

	private Vector3 AlignMovementToFloor(Vector3 movement, Vector3 floorNormal)
	{
		// 移動ベクトルをフロアの法線に投影し、フロア上での水平成分を取得
		Vector3 projectedMovement = Vector3.ProjectOnPlane(movement, floorNormal);
		return projectedMovement;
	}


	const float moveSpeedMyShip = 0.125f;
	const float moveSpeedEnemy = 0.05f;
	void MoveByInput()
	{
		if (Now_floor != null && Now_floor.floorTransform != null)
		{
			// ◆◆◆◆◆このへんのコードはChatGPTが書きました（床のローカル空間に基づいた移動処理）
			Vector3 localInput = new Vector3(vect.x, 0, vect.z).normalized;
			Vector3 worldMove = Now_floor.floorTransform.TransformDirection(localInput) * moveSpeedMyShip;
			transform.position += worldMove;
		}
		else
		{
			// Fallback（床が未定義の場合の処理）
			Vector3 fallbackMove = new Vector3(vect.x, 0, vect.z).normalized * moveSpeedEnemy * speed;
			transform.position += fallbackMove;
		}
	}





// Use this for initialization
void Start()
	{
		for (int i = 0; i < flags.Length; i++)
		{
			flags[i] = 0;
			positions[i] = new Vector3(0, 0, 0);
			//Application.targetFrameRate = 60;
		}
		Now_floor = gameObject.AddComponent<FloorInteraction>();
		if (currentFloor != null)
		{
			//AlignToFloor(currentFloor.FloorNormal);
		}
	}






	// Update is called once per frame
	void Update()
	{
		//Debug.Log($"Update: isMovingToNewFloor = {isMovingToNewFloor}");
		if (!isMovingToNewFloor)
		{
			//MoveAlongFloor();
		}



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

				switch (count)  // 自機の状態に応じて移動・攻撃を行う
				{
					case 0: // 初期化
						{
							obj_mode = ObjectManager.MODE.HIT;
							obj_type = ObjectManager.TYPE.MYSHIP;
							NOHIT = false;
							MainHit.enabled = true;
							MainHit.size = new Vector3(0.9f, 5f, 0.9f);
							//this.transform.localScale = new Vector3(1, 1, 1);
							MainPic.sprite = MANAGE.SPR_MYSHIP[0];
							MainPic.enabled = true;
							angle = 0;
							flags[0] = 0;   // 連射トリガー
							flags[1] = 0;   // 移動角度
							flags[2] = 0;   // 実際に向いている角度
							flags[3] = 0;   // フロア側の角度
							local_mode = 1;
							power = 1;
							LIFE = 1;
							isMovingToNewFloor = false;
						}
						break;
					default: // 通常
#if false

						if (MAIN.cnt_game_over >= 0)
						{
							//if ((MAIN.cnt_stage - 1) >= Stage.STAGES.Length)
							{
								// 全ステージクリア演出
							}
							//else
							{
								//if ((count >> 2) % 4 == 0)
								{
									//MANAGE.Set(ObjectManager.TYPE.NOHIT_EFFECT, 0, 1, this.transform.position, this.transform.eulerAngles, Random.Range(0, 256), Random.Range(2, 8));
								}
							}
						}
						else
						{
							vect = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
							bool fire_1 = Input.GetButton("Fire1");				// ショット
							bool camera_next = Input.GetButtonDown("Fire2");	// カメラ切り替え
							bool respown = Input.GetButtonDown("Fire3");		// スタート地点に戻る

							if (
										(MAIN.cnt_stage_clear >= 0)
									|| (MAIN.cnt_game_over >= 0)
								)
							{
								camera_next = false;
								fire_1 = false;
								respown = false;
								vect = new Vector3(0, 0, 0);
								this.transform.SetParent(MANAGE.OBJ_ROOT.transform);
								if (MAIN.cnt_game_over >= 0)
								{
									int bomloop = Random.Range(2, 4);
									for (int i = 0; i < bomloop; i++)
									{
										MANAGE.Set(ObjectManager.TYPE.NOHIT_EFFECT, 0, 0, this.transform.position, this.transform.eulerAngles, Random.Range(0, 256), Random.Range(1, 8));
									}
								}
							}
							else if (MAIN.cnt_stage_start == 1)
							{
								this.transform.localPosition = new Vector3(0, 0, -8);
							}
							if (camera_next == true)
							{
								//CAMERA.Next();
							}

							if (fire_1 == true)
							{
								if (MAIN.cnt_game_over < 0)
								{
									if (flags[0] == 0)  // 連射抑制スイッチ(nフレーム待って射出)
									{
										MANAGE.Set(ObjectManager.TYPE.MYSHOT, 0, 0, this.transform.position, new Vector3(0, 0, 0), flags[3], (int)(positions[3].x * 10));
										SoundManager.Instance.PlaySE((int)SoundHeader.SE.SHIP_SHOT);
									}
								}
								if (++flags[0] >= 4)
								{
									flags[0] = 0;
								}
							}
							else
							{
								flags[0] = 0;
							}
							//DebugStation.SetText("\ncount=" + count + " / this.transform.position=" + this.transform.position,true);





							switch (local_mode)
							{
								case 1:
									{
										positions[2] = new Vector3(0, 0, 0);    // コントローラ軸：最終的にvectにコピーされる
										
										if (vect.x > 0.5f)
										{
											positions[2].x = 1.0f;
										}
										else if (vect.x < -0.5f)
										{
											positions[2].x = -1.0f;
										}
										else
										{
											positions[2].x = 0;
										}

										if (vect.z > 0.5f)
										{
											positions[2].z = 1.0f;
										}
										else if (vect.z < -0.5f)
										{
											positions[2].z = -1.0f;
										}
										else
										{
											positions[2].z = 0;
										}
									}
									break;
								case 2:
									{
										if (AdjustRotationToNewFloor() == true)
										{
											local_mode = 1;
											//flags[1] = 0;
										}
									}
									break;
							}
							//if (flags[1] == 0)
							{
								//local_mode = 1;
							}
						}
						break;
#endif
						break;
				}
				positions[0] = this.transform.localPosition;
#if false
				if (
							(MAIN.cnt_game_over<0)		// ゲームオーバー時
						&&	(MAIN.cnt_stage_clear<0)	// ステージクリア時のどちらでもない場合のみ移動
					)
				{
					vect = positions[2];
				}
				else
				{
					vect = new Vector3(0, 0, 0);
				}
				this.transform.localPosition += (vect * 0.125f);
				//FileOutput.Log("count="+count+" / MYSHIP:");
				//FileOutput.Log("this.transform.localPosition=" + this.transform.localPosition+ "vect=" + vect);
				//FileOutput.Log("this.transform.localEulerAngles=" + this.transform.localEulerAngles+" / local_mode="+local_mode);
				//DebugStation.SetText("MYSHIP:this.transform.localPosition=" + this.transform.localPosition + " / vect=" + vect, true);
#endif

				break;






#if false

			case ObjectManager.TYPE.MYSHOT:
				//Debug.Log("ここには来ている");

				switch (count)  // 自機の状態に応じて移動・攻撃を行う
				{
					case 0: // 初期化
						{
							//Debug.Log("ここは実行されてる");

							obj_mode = ObjectManager.MODE.HIT;
							obj_type = ObjectManager.TYPE.MYSHOT;
							//MainHit.enabled = true;
							//MainHit.size = new Vector2(32, 32);
							//Debug.Log("Generate:pos=" + pos + " / this.transform.position=" + this.transform.position);
							this.transform.localScale = new Vector3(4, 4, 4);
							NOHIT = false;
							MainHit.enabled = true;
							MainHit.size = new Vector3(0.8f, 0.8f, 1.8f);
							//MainModel = Instantiate(MANAGE.OBJ_MYSHOT, this.transform.position, Quaternion.identity,this.ModelRoll);  // 3Dモデルは物体を生成コピー(instantiate)して使う
							//MainModel.transform.SetParent(this.ModelRoll, false);   // 子オブジェクトに引っ付ける

							MainPic.enabled = true;
							MainPic.color = COLOR_NORMAL;
							this.transform.position += new Vector3(0, 0.5f, 0);
							this.transform.localEulerAngles = new Vector3(0, 0, (float)(speed % 360));  // スピード部分に元角度を格納
																										//MainPic.sortingOrder = 0;
							MainPic.sprite = MANAGE.SPR_MYSHOT[0];
							pos = this.transform.position;
							vect = new Vector3(0.0f, 0.0f, 1.0f);
							//speed = 10;

							flags[0] = 0;
							flags[1] = 0;
							flags[2] = 0;
							flags[3] = 0;
							local_mode = 1;
							power = 1;
							LIFE = 1;
							this.gameObject.SetActive(true);
							this.enabled = true;
						}
						break;
					default: // 通常
						{
							if (count == 1)
							{
								//Debug.Log("MYSHOT:Start="+this.transform.position+" / pos="+pos);
							}
							//DebugStation.SetText("count="+count+" / angle="+angle, true);

							positions[3] = this.transform.eulerAngles;
							positions[3].x += speed;
							this.transform.position += vect;
							this.transform.eulerAngles = positions[3];   // 回転方向が少し変わる
							if (this.transform.position.z >= 50.0f)
							{
								//Destroy(MainModel);
								MANAGE.Return(this);
							}
						}
						break;
				}
				break;


			/*
			フロアタイル
				すべてを塗ることでステージクリア


			*/
			case ObjectManager.TYPE.FLOOR:
				if (count == 0)
				{
					obj_mode = ObjectManager.MODE.HIT;
					obj_type = ObjectManager.TYPE.FLOOR;

					this.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
					// ★ まず回転を適用
					Now_floor.EulerAngles = this.transform.eulerAngles;
					//Debug.Log($"Step 1:count=: {count} :Applied Rotation = {transform.eulerAngles} :Now_floor.EulerAngles= {Now_floor.EulerAngles}");

					// ★ Instantiate() 後の変化をチェック
					MainModel = Instantiate(MANAGE.OBJ_FIELD, transform.localPosition, Quaternion.identity);
					//Debug.Log($"Step 2:count=: {count} :After Instantiate, Rotation = {MainModel.transform.eulerAngles}");

					// ★ SetParent() 後の変化をチェック
					MainModel.transform.SetParent(ModelRoll,true);	// prefabの最下層にMainModelを置き直す
					MainModel.transform.localRotation = Quaternion.identity;  // ★ 親の影響を打ち消す
					//WIREFRAME_CTRL = MainModel.gameObject.GetComponent<GeometryWireFrameCtrl>();
					//MainModel.transform.localScale = new Vector3(100f, 100f, 1f);
					MainModel.transform.localEulerAngles = new Vector3(270, 0, 0);
					//Debug.Log($"Step 3:count=: {count} :After SetParent, Rotation = {MainModel.transform.eulerAngles}");


					// ★ Now_floor へ設定
					Now_floor.Position = this.transform.position;

					//Debug.Log($"Before Updating Now_floor: {Now_floor.EulerAngles}");
					Now_floor.EulerAngles = this.transform.eulerAngles;
					//Debug.Log($"After Updating Now_floor: {Now_floor.EulerAngles}");

					//Debug.Log($"Step 4-A:count=: {count} :Before Updating Now_floor: {Now_floor.EulerAngles}");
					Now_floor.EulerAngles = this.transform.eulerAngles;
					//Debug.Log($"Step 4-B:count=: {count} :After Updating Now_floor: {Now_floor.EulerAngles}");
#if true
					Now_floor.Position = this.transform.position;
					Now_floor.EulerAngles = this.transform.eulerAngles;
					Now_floor.FloorNormal = (Quaternion.Euler(Now_floor.EulerAngles) * Vector3.up).normalized;
					Now_floor.SlopeDirection = (Quaternion.Euler(Now_floor.EulerAngles) * Vector3.forward).normalized;
					Now_floor.CurrentAngle = Vector3.Angle(Vector3.up, Now_floor.FloorNormal);
					Now_floor.playerOnFloor = 0;
#endif
					//Debug.Log($"Step 5:count=: {count} :After Initialized Now_floor: {Now_floor.EulerAngles}");
					this.transform.localScale = new Vector3(1f, 1f, 1f);
					
					this.transform.eulerAngles = Now_floor.EulerAngles;
					//Debug.Log($"Step 6:count=: {count} :After Setting this.transform.eulerAngles: {this.transform.eulerAngles}");

					MainModel.gameObject.SetActive(true);
					//WIREFRAME_CTRL.gameObject.SetActive(true);
					//Debug.Log("child=[" + this.transform.Find("board1_fbx4").name + "]");
					//Debug.Log("WIREFRAME_CTRL=[" + WIREFRAME_CTRL + "]");
					NOHIT = false;
					MainHit.enabled = false;  // 出現直後は当たりを見ない
					MainHit.size = new Vector3(1.8f, 0.1f, 1.8f);

					flags[0] = 0;   // フロアの状態
					flags[1] = 0;   // フロア消滅カウント用
					flags[2] = 0;	// フロアに何かが乗っているチェック用
					flags[3] = 0;   // フロアの拡大カウンタ
					Now_floor.playerOnFloor = 0;	// フロアに何が乗っているかチェック用

					this.transform.localScale = new Vector3(2.99f, 2.99f, 2.99f);

					MainHit.enabled = true;   // 当たり判定発生

					local_mode = 1; // 動作モード設定(local_mode:0の場合は出現デモを作る)
					power = 1;
					LIFE = 1;
				}
				if (MAIN.cnt_paint_floor == 0)
				{
					local_mode = 2;   // 塗りつぶし終わったらフロアが崩れ落ちる
				}


				if (MAIN.cnt_kill_enemy == 0)
				{
					if (MAIN.cnt_stage_clear >= 235)
					{
						MANAGE.Return(this);
					}
				}

				flags[2] = Now_floor.playerOnFloor % 2; // 1の時、自機が乗っている
				if (flags[2] == 1)	// 自機がフロアに乗っている
				{
					//DebugStation.SetText("Floor[" + OBJcnt + "]:count=" + count + " / flags[2]=" + flags[2] + " / Now_Floor.playerOnFloor=" + Now_floor.playerOnFloor, true);
					if (Now_floor.playerOnFloor >= 2)	// 同一フロアに敵と自機が同時に存在する場合
					{
						SoundManager.Instance.PlaySE((int)SoundHeader.SE.ENEMY_FLOOR_SHIP);
					}
					else
					{
						// 自機だけ乗っている場合は特に何もしない
					}
				}

				//Debug.Log($"Now_floor is {(Now_floor == null ? "NULL" : Now_floor.EulerAngles.ToString())}");
				//Debug.Log($"Update() called, count={count}");
				//Debug.Log($"WIREFRAME_CTRL is {(WIREFRAME_CTRL == null ? "NULL" : "OK")}");

				switch (local_mode)
				{
					case 0:     // 出現～配置
						if (flags[3] >= 0)
						{
							flags[0] = 0;
							this.transform.localScale = new Vector3(2.99f, 2.99f, 2.99f);
							local_mode = 1;
							MainHit.enabled = true;   // 当たり判定発生
						}
						else
						{
							flags[3]++;
							this.transform.localScale += positions[3];
						}
						break;
					case 1:     // 配置後の状態遷移
						switch (flags[0])
						{
							case 0:     // 触れていないフロア(色がデフォルト)
								//WIREFRAME_CTRL.ChangeColor(new Color(1, 1, 1, 1));
								//WIREFRAME_CTRL.Start();
								break;
							case 1:     // 自機が乗っているフロア
								//WIREFRAME_CTRL.ChangeColor(new Color(1, ((float)(count << 4) % 256.0f) / 256.0f, 0, 1));
								//WIREFRAME_CTRL.Start();
								break;
							case 2:     // 自機が乗った後のフロア(色が付く)
								//WIREFRAME_CTRL.ChangeColor(new Color(0, 0.75f, 1, 1));
								//WIREFRAME_CTRL.Start();
								break;
						}
						break;
					case 2:     // フロアを全部塗りつぶしてクリア(崩れるイメージ)
						{
							if (MAIN.cnt_stage_clear == 0)
							{
								flags[0] = 0;   // 総合カウンタ
								flags[1] = 120 + Random.Range(0, 110);  // フロアが消える最大フレーム数(乱数)
								positions[0] = new Vector3(Random.Range(-0.005f, 0.005f), Random.Range(-0.005f, 0.005f), Random.Range(-0.005f, -0.1f));
								positions[1] = new Vector3(0, Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
							}
						}
						this.transform.localPosition += positions[0];
						this.transform.localEulerAngles += positions[1];
						flags[0]++;
						if (flags[0] >= flags[1])
						{
							MANAGE.Return(this);
						}
						break;
				}
				break;
#endif



			/*
			 *	敵1
			 *		出現後中央に向けて左右移動
			 *		フロアにヒットしなかった場合中央に向けて前後移動
			 *		敵同士でぶつかった時に中央に向けて前後移動
			 *		ある程度前後移動したら直前と逆に移動
			 * 
			 * 
			 * 
			 * 
			 * 
			 * 
			 * 
			 * 
			 * 
			 */
			case ObjectManager.TYPE.ENEMY01:
				if (count == 0)
				{
					obj_mode = ObjectManager.MODE.HIT;
					obj_type = ObjectManager.TYPE.ENEMY01;
					//MainPic.sprite = MANAGE.SPR_WALL[2];
					//MainHit.enabled = true;
					//MainHit.size = new Vector2(32, 32);
#if false

					MainModel = Instantiate(MANAGE.OBJ_ENEMY01);  // 3Dモデルは物体を生成コピー(instantiate)して使う
																  //while (this.ModelRoll.transform.childCount == 0)

					MainModel.transform.SetParent(this.ModelRoll, false);   // 子オブジェクトに引っ付ける
																			//MainModel.AddComponent<GeometryWireFrameCtrl>();        // 3Dモデルをワイヤーフレーム表示する

					MainModel.transform.localPosition = new Vector3(0, 0, 0);
					MainModel.transform.localScale = new Vector3(2, 2, 2);
					MainModel.transform.localEulerAngles = new Vector3(0, 180, 0);
					MainModel.gameObject.SetActive(true);

					//Debug.Log("child=[" + this.transform.Find("board1_fbx4").name + "]");
					//Debug.Log("WIREFRAME_CTRL=[" + WIREFRAME_CTRL + "]");
#endif
					NOHIT = false;
					MainHit.enabled = true;
					MainHit.size = new Vector3(0.9f, 5f, 0.9f);
					//MainPic.color = COLOR_NORMAL;
					//MainPic.sortingOrder = 1;
					Now_floor.playerOnFloor = 0;
					flags[0] = 0;
					flags[1] = 0;
					flags[2] = -1;
					flags[3] = +1;
					positions[0] = new Vector3(0, 0, 0);
					positions[1] = new Vector3(0, 0, 0);
					positions[2] = new Vector3(0, 0, 0);
					positions[3] = new Vector3(0, 0, 0);
					local_mode = 1; // 動作モード設定(1=フロアに立っている・2=次のフロアに接触・0=フロアにいない)
					power = 1;
					LIFE = 1;
					this.transform.localPosition += new Vector3(0, 0.5f, 0);    // 表示上フロアの上に乗っているように見えたい
					SoundManager.Instance.PlaySE((int)SoundHeader.SE.ENEMY_BORN);
					AdjustRotationToNewFloor();

					FileOutput.Log("Enemy Start: Now_floor = " + Now_floor);
					FileOutput.Log("Enemy floor Normal: " + Now_floor.FloorNormal);
				}

				vect = MANAGE.AngleToVector3(angle, speed); // 基本的に左右にしか動かさせない
				vect.z = vect.y;
				vect.y = 0.0f;

				switch (flags[0])
				{
					case 0:     // フロアを横移動中
						{
							switch (local_mode)
							{
								case 0:
									break;
								case 1:
									//positions[0] = this.transform.localEulerAngles; // 現在の角度を保存
									break;
								case 2:
									break;

							}
							vect = AlignMovementToFloor(vect, Now_floor.FloorNormal);   // 移動方向をフロアに合わせる
							this.transform.position += (vect * 0.05f); // スピードが制御したい場合ここをいじる■■■
						}
						break;
					case 1:  // フロアから外れた
						{
							flags[1]++;
							if (flags[1] >= 3)
							{
								//local_mode = 2;
								flags[0]++;
								flags[1] = 0;
							}
							else
							{
								vect = AlignMovementToFloor(vect, Now_floor.FloorNormal);   // 移動方向をフロアに合わせる
								this.transform.position += (vect * 0.05f); // スピードが制御したい場合ここをいじる■■■
							}
						}
						break;
					case 2:  // フロア外を縦移動中(nフレーム後に横移動反転→移動→フロアに乗るハズ)
						{
							Vector3 airmove = this.transform.position;
							airmove.z += 0.05f * flags[2];             // スピードが制御したい場合ここをいじる■■■
							flags[1]++;
							if (flags[1] >= 3)
							{
								positions[0] = -positions[0]; // 移動ベクトル反転
								vect = positions[0];         // 反転起動をコピーしておく
								flags[1] = 0;
								flags[0] = 3;
							}
							else
							{
								vect = AlignMovementToFloor(vect, Now_floor.FloorNormal);   // 移動方向をフロアに合わせる
								this.transform.position += (vect * 0.05f);
							}
						}
						break;
					case 3:     // フロア外を横移動中(2の段階で横移動が反転しているので、そのまま進めばフロアに乗るハズ)
						{
							flags[1]++;
							if (flags[1] >= 3 * 2)  //反転してもフロアに乗らない場合
							{
								flags[1] = 0;
								if (this.transform.position.z < 0.0f)
								{
									positions[0].z += 0.1f;
								}
								else
								{
									positions[0].z += -0.1f;
								}
								vect = positions[0];
								vect = AlignMovementToFloor(vect, Now_floor.FloorNormal);   // 移動方向をフロアに合わせる
								this.transform.position += vect;    // 外宇宙に出るか中央に固まるかするハズ
							}
						}
						break;
				}
				//if (parampos[3].z > 0)
				{
					//DebugStation.SetText("ENEMY01:parampos[3]=" + parampos[3], true);
				}
				//positions[3].z = 0;  // フロア接地カウンタ(0の場合接地していない)




				switch (local_mode)
				{
					case 1:
#if false
						{

							positions[2] = new Vector3(0, 0, 0);    // コントローラ軸：最終的にvectにコピーされる

							if (vect.x > 0.5f)
							{
								positions[2].x = 1.0f;
							}
							else if (vect.x < -0.5f)
							{
								positions[2].x = -1.0f;
							}
							else
							{
								positions[2].x = 0;
							}

							if (vect.z > 0.5f)
							{
								positions[2].z = 1.0f;
							}
							else if (vect.z < -0.5f)
							{
								positions[2].z = -1.0f;
							}
							else
							{
								positions[2].z = 0;
							}
						}
#endif
						{
							positions[0] = this.transform.localPosition;

						}
						break;
					case 2:
						{
							if (AdjustRotationToNewFloor() == true)
							{
								local_mode = 1;
								//flags[1] = 0;
							}
						}
						break;
				}
				//if (flags[1] == 0)
				{
					//local_mode = 1;
				}
				FileOutput.Log("|" + OBJcnt + "|" + count + "|Now_Floor.FloorNormal=" + Now_floor.FloorNormal + " / Now_Floor.transform.position=" + Now_floor.transform.position + " / this.transform.position=" + this.transform.position);
#if false

				switch (local_mode)
				{
					case 1:
						{
#if true
							if (positions[3].y > 0)
							{
								Vector3 v = new Vector3(-0.25f, 0, 0);
								this.transform.localPosition += v;
							}
#endif
							this.transform.localPosition += vect * 0.05f;   // 1の場合は通常ベクトルで動作
						}
						break;
					case 2:                                             // フロア乗り移り角度変更チェック
						{
						



#if true   // 自機で試して問題がなければ移動サブルーチンと差し替える                                  

							positions[2] = this.transform.localEulerAngles;
							//DebugStation.SetText(("ロール前:count=" + count + " / param[1]="+param[1]+" / param[2]=" + param[2] + " / param[3]=" + param[3] + " / input=" + parampos[3] + "\n"), false);
							if (positions[3].x == 1)
							{
								flags[1] -= 3;
								flags[2] += 3;
								if (flags[1] <= 0)
								{
									flags[1] = 0;
									flags[2] = 0;
									local_mode = 1;
								}
							}
							else if (positions[3].x == -1)
							{
								flags[1] -= 3;
								flags[2] -= 3;
								if (flags[1] <= 0)
								{
									flags[1] = 0;
									flags[2] = 0;
									local_mode = 1;
								}
							}
							else
							{
								flags[1] = 0;
								local_mode = 1;
							}
						
#endif
						}
						break;
				}
#endif
#if false              
				//parampos[2].z = (param[1] * parampos[3].z);                                                            
				positions[2].z = (float)((flags[1] * positions[3].z) / 15) * flags[1];
				this.transform.localEulerAngles = positions[2];
				if (flags[1] == 0)
				{
					local_mode = 1;
					//parampos[3].y = 0;
				}


				if (positions[3].y > 0)
				{
					if (positions[3].y == 4)
					{
						vect = -vect;
					}
					positions[3].y++;
				}
#endif


				//if (MAIN.cnt_paint_floor == 0)
				if (false)
				{
					//MAIN.score = MAIN.cnt_stage * 50;
					// ここに爆発発生処理入れる
					MANAGE.Return(this);
				}

				if (
						(Mathf.Abs(this.transform.position.x) >= 50.0f) // ワールド座標で範囲外に出たら「倒した(スコア0)」
					|| (Mathf.Abs(this.transform.position.y) >= 50.0f)
					|| (Mathf.Abs(this.transform.position.z) >= 50.0f)
					)
				{ 
					//MAIN.cnt_kill_enemy--;
					MANAGE.Return(this);
				}
				//DebugStation.SetText("\n\nthis.transform.position=" + this.transform.position + "\nthis.transform.localEulerAngles=" + this.transform.localEulerAngles, false);
				//DebugStation.SetText("\nvect=" + vect + " / parampos[3]" + positions[3], false);
				//DebugStation.SetText("\nparam[0]=" + flags[0] + " / local_mode=" + local_mode, false);

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
				if (count == 0)
				{
					obj_mode = ObjectManager.MODE.NOHIT;
					MainHit.enabled = false;
					//MainHit.enabled = false;
					MainPic.enabled = true;
					LIFE = 1;
					vect = MANAGE.AngleToVector3(angle, speed * 0.05f);
					if (local_type == 1)
					{
						this.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
						flags[3] = 60;
						vect = vect * Random.Range(2.0f, 5.0f);
					}
					else
					{
						this.transform.localScale = new Vector3(8.3f, 8.3f, 8.3f);
						flags[3] = 32;
					}
					positions[0] = new Vector3(Random.Range(-2.0f, 2.0f), Random.Range(-2.0f, 2.0f), Random.Range(-2.0f, 2.0f));
					flags[0] = Random.Range(0, 4);
					MainPic.sprite = MANAGE.SPR_CRUSH[flags[0]];
					MainPic.sortingOrder = 5;
				}
				else if (count >= flags[3])
				{
					MANAGE.Return(this);
				}
				else
				{
					flags[0] = (flags[0] % 8);
					flags[0]++;
					this.transform.localPosition += vect;
					this.transform.localScale = this.transform.localScale * 1.05f;
					//vect -= new Vector3(0.02f, 0.02f, 0.02f);
					if (local_type == 0)
					{
						MainPic.sprite = MANAGE.SPR_CRUSH[count >> 2];
						//this.transform.LookAt(MANAGE.CAMERA_ROOT.cam_pointer.transform);
					}
					else
					{
						MainPic.sprite = MANAGE.SPR_CRUSH[8 + ((count >> 2) % 4)];
						this.transform.localEulerAngles += positions[0];
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


/*
	/// <summary>
	///		自機の向きをフロア正面(基本的にZ軸奥)に固定
	/// </summary>
	/// <param name="floorNormal"></param>
	void AlignToFloor(Vector3 floorNormal)
	{
		Debug.Log($"AlignToFloor: floorNormal={floorNormal}");

		if (floorNormal == Vector3.up)
		{
			Debug.LogWarning("Floor is flat (0,0,0), skipping alignment.");
			return;
		}

		Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, floorNormal);
		Quaternion forwardRotation = Quaternion.LookRotation(Vector3.forward, floorNormal);

		Debug.Log($"targetRotation.eulerAngles={targetRotation.eulerAngles}");
		Debug.Log($"forwardRotation.eulerAngles={forwardRotation.eulerAngles}");

		transform.rotation = forwardRotation * targetRotation;

		Debug.Log($"Final rotation.eulerAngles={transform.rotation.eulerAngles}");
	}
*/




	/*
	
	/// <summary>
	///  フロアをまたいだ移動・スムーズな補間
	/// </summary>
	/// <param name="newFloor">移動先フロア情報</param>
	/// <returns>値が出るまで別スレッドで計算を続ける(雑な理解)</returns>
	IEnumerator SmoothMoveToNewFloor(FloorInteraction newFloor)
	{
		Debug.Log($"SmoothMoveToNewFloor: Start - Target FloorNormal={newFloor.FloorNormal}");


		Vector3 startPos = transform.position;
		Quaternion startRot = transform.rotation;

			Debug.Log($"SmoothMoveToNewFloor: Before Now_floor.FloorNormal={Now_floor.FloorNormal}");
		
		isMovingToNewFloor = true;

		// 現在のフロア上でのローカル座標を計算
		Vector3 localOffset = currentFloor.floorTransform.InverseTransformPoint(transform.position);

		// 新しいフロア上でのワールド座標に変換
		Vector3 targetPos = newFloor.floorTransform.TransformPoint(localOffset);

		Quaternion targetRot = Quaternion.LookRotation(Vector3.forward, newFloor.FloorNormal);

		float duration = 0.25f;
		float elapsed = 0f;

		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
			transform.rotation = Quaternion.Slerp(startRot, targetRot, elapsed / duration);
			yield return null;
		}

		transform.position = targetPos;
		transform.rotation = targetRot;

		currentFloor = newFloor; // フロアの更新は補正完了後に行う

		isMovingToNewFloor = false;

		Debug.Log($"SmoothMoveToNewFloor: After Now_floor.FloorNormal={Now_floor.FloorNormal}");

	}
*/

/*
	/// <summary>
	///		フロアをまたいだ移動
	/// </summary>
	/// <param name="inputDirection"></param>
	/// <param name="floor"></param>
	/// <param name="transform"></param>
	void MoveAlongFloor(Vector3 input, FloorInteraction floor, Transform transform)
	{
		Vector3 forward = floor.SlopeDirection;
		Vector3 right = Vector3.Cross(floor.FloorNormal, forward).normalized;

		Vector3 moveDir = forward * input.z + right * input.x;
		transform.position += moveDir * speed * Time.deltaTime;
	}
*/










	/// <summary>
	/// 当たり判定部・スプライト同士が衝突した時に走る
	/// </summary>
	/// <param name="collider">衝突したスプライト当たり情報</param>
	void OnTriggerEnter(Collider collider)
	{




	
		if (obj_mode == ObjectManager.MODE.NOHIT)
		{
			return;
		}
		ObjectCtrl other = collider.gameObject.GetComponent<ObjectCtrl>();
		if (other == null)
		{
			//DebugStation.SetText(("【衝突】count="+count+" / OnTriggerEnter:this=" + this.name + " / other=" + other), true);
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
#if false
		switch (other.obj_type)
		{

			case ObjectManager.TYPE.FLOOR:
				{
					if (obj_type == ObjectManager.TYPE.MYSHIP)
					{
						if (other.Now_floor.playerOnFloor == 0)
						{
							other.Now_floor.playerOnFloor = 1;		// 自機が乗っている場合は1を足す・1だった場合は自機のみなので敵機判定は行わない
						}

						if (other.flags[0] == 0)
						{
							SoundManager.Instance.PlaySE((int)SoundHeader.SE.FLOOR_PAINT);
							MAIN.cnt_paint_floor--;
						}

						//FileOutput.Log("★★★フロアタイルに乗っかる前：this.transform.localEulerAngles=" + this.transform.localEulerAngles);
						this.transform.SetParent(other.transform);
						Now_floor.EulerAngles = other.transform.eulerAngles;
						//FileOutput.Log("★★★フロアタイルに乗っかった：this.transform.localEulerAngles=" + this.transform.localEulerAngles);

						if (other.transform.eulerAngles!=this.transform.eulerAngles)	// 自分とフロアタイルの角度が一致しない場合
						{
							other.flags[0] = 1;
							local_mode = 2;

							//Now_floor.EulerAngles = other.transform.localEulerAngles - this.transform.localEulerAngles;	// 差分を取る
							if (Now_floor.EulerAngles.x >= 180)
							{
								Now_floor.EulerAngles.x -= 360;
							}
							else if (Now_floor.EulerAngles.z >= 180)
							{
								Now_floor.EulerAngles.z -= 360;
							}
							FileOutput.Log("Enemy Floor Hit! " + other.gameObject.name);
							//FileOutput.Log("OnTriggerEnter:新しいフロアタイルに乗った");
							//FileOutput.Log("OnTriggerEnter:my=" + this.transform.localEulerAngles + " / FLOOR=" + other.transform.localEulerAngles);
							//FileOutput.Log("Now_floor.EulerAngles=" + Now_floor.EulerAngles);
						}
					}
					else if (obj_type == ObjectManager.TYPE.ENEMY01)
					{
						if (other.Now_floor.playerOnFloor == 0)
						{
							other.Now_floor.playerOnFloor += 2;		// 敵がフロアに乗った場合は偶数を足す・奇数の場合には自機と敵機が同一フロアに乗っている
						}

						FileOutput.Log("|"+OBJcnt+"|"+count+"|敵機ユニークナンバー(OBJcnt)=" + this.OBJcnt);
						FileOutput.Log("|" + OBJcnt + "|" + count + "|★★★フロアタイルに乗っかる前：this.transform.localEulerAngles=" + this.transform.localEulerAngles);
						this.transform.SetParent(other.transform);
						Now_floor.EulerAngles = other.transform.eulerAngles;
						FileOutput.Log("|" + OBJcnt + "|" + count + "|★★★フロアタイルに乗っかった：this.transform.localEulerAngles=" + this.transform.localEulerAngles);

						if (other.transform.eulerAngles != this.transform.eulerAngles)  // 自分とフロアタイルの角度が一致しない場合
						{
							other.flags[0] = 1;
							local_mode = 2;

							//Now_floor.EulerAngles = other.transform.localEulerAngles - this.transform.localEulerAngles;	// 差分を取る
							if (Now_floor.EulerAngles.x >= 180)
							{
								Now_floor.EulerAngles.x -= 360;
							}
							else if (Now_floor.EulerAngles.z >= 180)
							{
								Now_floor.EulerAngles.z -= 360;
							}

							FileOutput.Log("|" + OBJcnt + "|" + count + "|OnTriggerEnter:新しいフロアタイルに乗った");
							FileOutput.Log("|" + OBJcnt + "|" + count + "|OnTriggerEnter:my=" + this.transform.localEulerAngles + " / FLOOR=" + other.transform.localEulerAngles);
							FileOutput.Log("|" + OBJcnt + "|" + count + "|Now_floor.EulerAngles=" + Now_floor.EulerAngles);
						}
					}

#if false

					{
						local_mode = 2;             // 自機：角度を合わせるまで動けない(5フレーム程度？)
#if true
						//other.param[0] = 1;
						flags[3] = (int)(other.transform.localEulerAngles.z);   // 合わせるべき角度
						flags[2] = (int)(this.transform.eulerAngles.z);         // 現在の自機の速度
						if (flags[3] < flags[2])    //角度ぶち抜け対策
						{
							if (flags[3] == 0)
							{
								flags[3] = 360;
							}
						}
						if (flags[2] < flags[3])
						{
							if (flags[2] == 0)
							{
								flags[2] = 360;
							}
						}
						flags[1] = Mathf.Abs(flags[2] - flags[3]);              // 移動量の差分
																				//string s = "\nNEW PANEL:count=" + count + " / angle=" + other.transform.localEulerAngles + "\nparam[1]="+param[1]+"param[2]=" + param[2] + " / param[3]=" + param[3] + "\n";
																				//DebugStation.SetText(s, true);
																				//other.local_mode = 2;       // フロア：自機が乗っている状態
																				//this.transform.localEulerAngles = other.transform.eulerAngles;
						//this.transform.SetParent(other.transform);
						//this.transform.localEulerAngles = other.transform.localEulerAngles;

						positions[3].y = 0;
						Vector3 v = this.transform.localPosition;
						v.y = 0;
						this.transform.localPosition = v;
#endif

						if (other.transform.localEulerAngles.z != this.transform.localEulerAngles.z)
						{
							Vector3 oy = other.transform.localEulerAngles;  //角度合ってなければ無理矢理合わせる
							oy.z = this.transform.localEulerAngles.y;
							this.transform.localEulerAngles = oy;
						}
						//this.transform.SetLocalPositionAndRotation(this.transform.localPosition, other.transform.localRotation);
					}
#endif
				}
				break;
			case ObjectManager.TYPE.MYSHOT:
				{
					if (obj_type == ObjectManager.TYPE.ENEMY01) // 自機弾に敵が当たった
					{
						//MAIN.cnt_kill_enemy--;                      // 敵機数減らす
						//MAIN.score += 30;
						SoundManager.Instance.PlaySE((int)SoundHeader.SE.ENEMY_SHOT_HIT);
						int rep = Random.Range(2, 8);
						for (int i = 0; i < rep; i++)
						{
							MANAGE.Set(ObjectManager.TYPE.NOHIT_EFFECT, 0, 0, this.transform.position, new Vector3(0, 0, 0), Random.Range(0, 256), Random.Range(1, 4));
						}
						rep = Random.Range(5, 12);
						for (int i = 0; i < rep; i++)
						{
							MANAGE.Set(ObjectManager.TYPE.NOHIT_EFFECT, 1, 0, this.transform.position, new Vector3(0, 0, 0), Random.Range(0, 256), Random.Range(4, 8));
						}
						
						// ここにスコア加算と残り敵数減算を書く
						//MAIN.score += 300;
						//MAIN.cnt_kill_enemy--;
			
						MANAGE.Return(this);
					}
				}
				break;
			case ObjectManager.TYPE.MYSHIP: // ゲームオーバー条件
				{
					if (obj_type == ObjectManager.TYPE.ENEMY01) // 敵に体当たりしてしまうと終了
					{
						//ENERGY.sw_fuel_empty = true;
						//ENERGY.EnemyHit();
						SoundManager.Instance.PlaySE((int)SoundHeader.SE.ENEMY_SHIP_HIT);
						{
							if (MAIN.cnt_game_over < 0) // 二重にゲームオーバーになるのを防ぐ
							{
								MAIN.cnt_game_over = 0;
							}
						}
					}
				}
				break;
			case ObjectManager.TYPE.ENEMY01:
				{
					if (obj_type == ObjectManager.TYPE.ENEMY01)
					{
						if (other.group_id != group_id)
						{
							Vector3 movedown = new Vector3(0, 0, -0.25f);
							this.transform.position += movedown;
							other.transform.position += movedown;
							other.vect = -other.vect;
							vect = -vect;
						}
					}
				}
				break;

		}
#endif
	}

#if false

	private void OnTriggerStay(Collider collider2)	// 床に乗っているかどうかをチェックする(0が帰ったら何にも乗っていない)
	{
		ObjectCtrl other=collider2.gameObject.GetComponent<ObjectCtrl>();
		if (other != null)
		{
			if (other.obj_type == ObjectManager.TYPE.ENEMY01)
			{
				if (obj_type == ObjectManager.TYPE.FLOOR)
				{
					flags[0] = 1;                   // フロアに乗ってる通知
					Now_floor.playerOnFloor += 2;	// フロアに「敵機も」乗っている状態
													// 自機が乗っている場合は奇数になるハズ
				}
				//other.positions[3].y = 0;
			}
			else if (other.obj_type == ObjectManager.TYPE.MYSHIP)
			{
				if (obj_type == ObjectManager.TYPE.FLOOR)
				{
					Now_floor.playerOnFloor = 1;    // フロアに自機「だけ」が乗っている状態
					flags[0] = 1;					// フロアに乗ってる通知
					//other.positions[3] = Now_floor.EulerAngles;
				}
			}
		}
	}


	/// <summary>
	///		接触から離れた場合・最終的にはフロアを全部塗ればクリア
	/// </summary>
	/// <param name="collider"></param>
	void OnTriggerExit(Collider collider2)
	{
		ObjectCtrl other = collider2.gameObject.GetComponent<ObjectCtrl>();
		switch (other.obj_type)
		{
			case ObjectManager.TYPE.FLOOR:
				{
					other.Now_floor.playerOnFloor = 0;	// フロアから離れた・一旦フロア側をすべて初期化

					if (obj_type == ObjectManager.TYPE.MYSHIP)
					{
						other.flags[0] = 2;		// フロア塗った後
					}
					if (obj_type == ObjectManager.TYPE.ENEMY01)
					{
						//positions[3].y = 1;    // フロアから足抜けした
					}
				}
				break;
			default:
				break;
		}
	}
#endif



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


