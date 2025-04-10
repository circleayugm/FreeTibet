/*******************************************************************************************************
 * 
 *	ObjectManager.cs 
 *		オブジェクト動作マネージャ
 *		生成・消去・リスト管理など
 *		当たり判定の独自実装部分もここ
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 *		20221211	3日前くらいにWSc101用に再構成
 *		20221213	止むに止まれず当たり判定を独自実装
 *		20221215	新しいキャラ実装にあたって再構成
 *		20240715	3Dobjを扱うために大幅書き換え
 * 
********************************************************************************************************/
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public sealed class ObjectManager : MonoBehaviour {

	[SerializeField]
	public Transform OBJ_ROOT;
	[Space]
	[SerializeField]
	public Sprite[] SPR_MYSHIP; // 暫定；キャラクタ画像置き場・将来的にはキャラクタ設定時にリストに登録するようにする.
	[SerializeField]
	public Sprite[] SPR_ENEMY;
	[SerializeField]
	public Sprite[] SPR_CRUSH;
	[SerializeField]
	public Sprite[] SPR_MYSHOT;
	[Space]
	[SerializeField]
	public GameObject OBJ_FIELD;
	[SerializeField]
	public GameObject OBJ_MYSHOT;
	[SerializeField]
	public GameObject OBJ_MYSHIP;
	[SerializeField]
	public GameObject OBJ_ENEMY01;
	[Space]
	[SerializeField]
	public ObjectCtrl PREFAB_OBJECT;    // キャラクタオブジェクトのprefab.
	[Space]
	//[SerializeField]
	//public CameraCtrl CAMERA_ROOT;

	public static readonly int LIMIT = 50;             // キャラクタ表示総数.

	public enum MODE    // 処理モード.
	{
		NOUSE = 0,          // 現在未使用.
		INIT,               // 初期化(出現直後).
		NOHIT,              // 当たり無視.
		HIT,                // 当たり有効・通常時はこちら.
		DEAD,               // 死亡アニメ.
		FINISH,             // 最終処理.
	}
	public enum TYPE    // 処理タイプ(当たり判定有無を大雑把に仕分け).
	{
		NOUSE = 0,          // 未使用.
		MYSHIP = 1,			// 自機.
		//MYSHOT = 2,			// 自機ショット
		//FLOOR = 3,		    // 床・すべての物体は床に乗る
		ENEMY01 = 4,		// 敵
		//ITEM,				// アイテム
		//WALL,				// 壁
		NOHIT_EFFECT,		// 爆風
	}

	public readonly Vector3 OBJECT_DISPLAY_LIMIT = new Vector3((272 * 3) / 2, (480 * 3) / 2, 0);
	public readonly Vector3 OBJECT_BULLET_LIMIT = new Vector3(272 / 2, 480 / 2, 0);

	public List<ObjectCtrl> objectStock = new List<ObjectCtrl>();
	public List<ObjectCtrl> objectUsed = new List<ObjectCtrl>();

	public int objectStockMax = LIMIT;
	public int objectUsedMax = 0;

	public bool SW_BOOT = false;        // 動作準備が整った時true

	float[] ang256 = new float[256];

	public Transform now_camera = null;

	public Vector3 pos_myship;

	void Start()
	{
		Debug.Log("★★★★★ObjectManager:Start");
		//if (ModeManager.now_mode != ModeManager.MODE.GAME_PLAY)
		{
			//return;
		}
		objectStock.Clear();
		objectStockMax = 0;
		objectUsed.Clear();
		objectUsedMax = 0;
		for (int i = 0; i < ang256.Length; i++)
		{
			ang256[i] = (90-(360.0f / 256.0f) * i);
		}
		Debug.Log("オブジェクト自薦生成部分");
		for (int i = 0; i < LIMIT; i++)
		{
			ObjectCtrl obj = new ObjectCtrl();
			Debug.Log("　→Generateに投げる直前：i=" + i);
			obj = Generate(TYPE.NOUSE, new Vector3(0, -10000, 0), 0, 0);
			obj.OBJcnt = i;
			objectStock.Add(obj);
			objectStockMax++;
			Debug.Log("　→最低限の初期化完了：i=" + i);
		}


		SW_BOOT = true;
	}
#if false
	private void Start()
	{
		CameraCtrl cameraCtrl = GameObject.Find("Camera").GetComponent<CameraCtrl>();
		CAMERA_ROOT = cameraCtrl;
	}
#endif





	/// <summary>
	///		初期設定；オブジェクトの前準備
	/// </summary>
	/// <param name="type">オブジェクトタイプ</param>
	/// <param name="pos">座標</param>
	/// <param name="angle">角度</param>
	/// <param name="speed">速度</param>
	/// <returns>前準備と設置が終わったオブジェクト</returns>
	public ObjectCtrl Generate(TYPE type, Vector3 pos, int angle, int speed)
	{
		ObjectCtrl obj = new ObjectCtrl();
		obj = ObjectCtrl.Instantiate(PREFAB_OBJECT, pos, Quaternion.identity);

		//InitializeNowFloor(obj.Now_floor,this.transform.eulerAngles);
		//obj.Now_floor.playerOnFloor = 0;
		obj.transform.SetParent(OBJ_ROOT);
		obj.obj_type = TYPE.NOUSE;
		obj.obj_mode = MODE.NOUSE;
		obj.gameObject.SetActive(false);
		obj.MainHit.enabled = false;
		obj.DisplayOff();
		return obj;
	}



	/// <summary>
	///		敵の出現初期化
	/// </summary>
	/// <param name="type"></param>
	/// <param name="group_id"></param>
	/// <param name="local_mode"></param>
	/// <param name="pos"></param>
	/// <param name="EulerAngle"></param>
	/// <param name="angle"></param>
	/// <param name="speed"></param>
	/// <returns></returns>
	public ObjectCtrl Set(TYPE type, int group_id,int local_mode,Vector3 pos, Vector3 EulerAngle,int angle, int speed)
	{
		if (objectStock.Count == 0)
		{
			return null;
		}
		ObjectCtrl obj;
		obj = objectStock[0];
		objectStockMax--;
		objectStock.RemoveAt(0);    // ここまで「ストックから取り出し」処理.

		//Debug.Log($"Set: Before Update Now_floor.FloorNormal={obj.Now_floor.FloorNormal}");



		//obj.MainModel = OBJ_ENEMY01;
		obj.local_mode = local_mode;            // オブジェクト設定.
		obj.local_type = 0;
		obj.group_id = group_id;
		obj.angle = angle;
		obj.speed = speed;
		obj.LIFE = 1;
		obj.obj_mode = MODE.INIT;
		obj.obj_type = type;
		obj.MainPic.sortingOrder = 0;
		obj.target = new Vector3(0, 0, 0);

		//InitializeNowFloor(obj.Now_floor,EulerAngle);
		//obj.Now_floor.playerOnFloor = 0;

		//Debug.Log("★ObjectManager.Set:phase 1:obj.transform.eulerAngles=" + obj.transform.eulerAngles);

		obj.transform.localRotation = Quaternion.identity;
		//obj.Now_floor.Position = pos;
		//obj.Now_floor.EulerAngles = EulerAngle;  // ★ フロアの EulerAngles を適用
		//obj.Now_floor.FloorNormal = (Quaternion.Euler(EulerAngle) * Vector3.up).normalized;
		//obj.Now_floor.SlopeDirection = (Quaternion.Euler(EulerAngle) * Vector3.forward).normalized;
		//obj.transform.localEulerAngles = EulerAngle;

		//Debug.Log($"Set FloorNormal={obj.Now_floor.FloorNormal}, EulerAngles={obj.Now_floor.EulerAngles}");

		//Debug.Log("★ObjectManager.Set:phase 2:obj.transform.eulerAngles=" + obj.transform.eulerAngles);

		obj.transform.position = pos;
		//Debug.Log("ObjectManager.Set:type="+obj.obj_type+" / Generate=" + this.transform.localPosition + " / pos=" + pos);

		obj.transform.localScale = new Vector3(1, 1, 1);
		obj.interval = 0;
		obj.enabled = true;
		obj.gameObject.SetActive(true);
		for (int i=0;i<obj.flags.Length;i++)
		{
			obj.flags[i] = 0;
			obj.positions[i] = new Vector3(0, 0, 0);
		}
		obj.count = 0;

		// フロアを更新する処理
		//Debug.Log($"Set: After Update Now_floor.FloorNormal={obj.Now_floor.FloorNormal}");

		objectUsed.Add(obj);        // 使用中リストに入れる処理.
		objectUsedMax++;
		return obj;
	}


	/// <summary>
	///		オブジェクト返却
	/// </summary>
	/// <param name="obj">返却するオブジェクト</param>
	/// <returns>使っていないオブジェクトの個数</returns>
	public int Return(ObjectCtrl obj)
	{
		obj.DisplayOff();
		objectUsed.Remove(obj);
		objectUsedMax--;
		obj.MainPic.enabled = false;
		obj.MainPic.sprite = null;
		//obj.MainModel.SetActive(false);
		//obj.MainModel = null;
		//obj.MainHit.enabled = false;
		//obj.MainPos.localPosition = new Vector3(-1000, 0, 0);
		obj.transform.SetParent(OBJ_ROOT);
		obj.obj_mode = MODE.NOUSE;
		obj.obj_type = TYPE.NOUSE;
		obj.enabled = false;
		objectStock.Add(obj);
		objectStockMax++;
		return objectStock.Count;
	}



	/// <summary>
	///		オブジェクト全部返却
	/// </summary>
	public void ResetAll()
	{
		int objcnt = objectUsed.Count;
		while (objectUsed.Count > 0)
		{
			ObjectCtrl obj;
			obj = objectUsed[0];
			objectUsed.RemoveAt(0);
			obj.obj_mode = MODE.NOUSE;
			obj.obj_type = TYPE.NOUSE;
			obj.DisplayOff();
			obj.enabled = false;
			obj.transform.SetParent(OBJ_ROOT);
			objectStock.Add(obj);
			objectStockMax = objectStock.Count;
			objectUsedMax = 0;
		}
	}



	/// <summary>
	///		使えるオブジェクトの個数を得る
	/// </summary>
	/// <returns>使えるオブジェクトの個数</returns>
	public int GetRestObject()
	{
		return objectStockMax;
	}

	/// <summary>
	///		出現中キャラクターの位置初期化
	/// </summary>
	public void ResetCharacter()
	{
		for (int i = 0; i <objectUsed.Count;i++)
		{
			objectUsed[i].count = -1;
		}
	}



	// ObjectManagerクラス内
	public void InitializeNowFloor(FloorInteraction floor, Vector3 eulerAngles)
	{
		floor.Position = Vector3.zero;
		floor.EulerAngles = eulerAngles;

		Quaternion rotation = Quaternion.Euler(eulerAngles);
		floor.FloorNormal = (rotation * Vector3.up).normalized;
		floor.SlopeDirection = (rotation * Vector3.forward).normalized;

		floor.floorTransform.rotation = rotation; // ★ 実際のTransformにも適用

		//FileOutput.Log($"Initialized Floor: Normal={floor.FloorNormal}, Euler={floor.EulerAngles}, Rotation={floor.floorTransform.rotation.eulerAngles}");
	}




	// 自機・敵機をrootに戻す(フロア全塗りクリアの時にフロアに乗ったままオブジェクト返済するのを防ぐ)
	public void MoveAllObjectsToRootFloor()
	{
		foreach (ObjectCtrl obj in objectUsed)
		{
			if (obj.obj_type == ObjectManager.TYPE.MYSHIP || obj.obj_type == ObjectManager.TYPE.ENEMY01)
			{
				obj.transform.SetParent(OBJ_ROOT.transform);
			}
		}
	}





	//-------------------------------------------------------------------
	/// <summary>
	///		敵弾に位置を基準とした移動量を設定
	/// </summary>
	/// <param name="mine">"基準座標(自分側)</param>
	/// <param name="pos">基準座標(相手側)</param>
	/// <param name="spd">移動速度</param>
	/// <param name="rad_offset">追加角度(扇状弾などに使う)</param>
	/// <param name="rotation">自分の絵を回転させる？(true=回転させる・false=回転させない)</param>
	/// <returns>移動量(Vector3・Z軸は常に0)</returns>
	//-------------------------------------------------------------------
	public Vector3 SetVector(Vector3 mine,Vector3 pos, float spd, float rad_offset, bool rotation)
	{
		Vector3 vec = Vector3.zero;
		if (spd == 0.0f)    //必ず少しは動くようにする.
		{
			spd = 1f;
		}
		//spd = spd / 100.0f;
		float rad = Mathf.Atan2(
								pos.y - mine.y,
								pos.x - mine.x
								);              //座標から角度を求める.
		rad += rad_offset;                      //角度にオフセット値追加.
		vec.x = spd * Mathf.Cos(rad);           //角度からベクトル求める.
		vec.y = spd * Mathf.Sin(rad);
		if (rotation == true)
		{
			this.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 90.0f + RadToRotation(rad)));
		}
		return vec;
	}
	//-------------------------------------------------------------------
	/// <summary>
	/// 角度と速度からxy移動量を算出する
	/// 戻り値はVector3
	/// </summary>
	/// <param name="ang">角度</param>
	/// <param name="spd">速度</param>
	/// <returns>実際の移動量(z軸は使わないので常に0)</returns>
	//-------------------------------------------------------------------
	public Vector3 AngleToVector3(float ang, float spd)
	{
		Vector3 vec = new Vector3(0, 0, 0);
		float rot = AngleToRotation(ang);
		float rad = ((rot) * Mathf.PI) / 180.0f;
		vec.x = spd * Mathf.Cos(rad);           //角度からベクトル求める.
		vec.y = spd * Mathf.Sin(rad);
		//vec = vec / 3;  // 一律で速度1/3に調整
		return vec;
	}


	//-------------------------------------------------------------------
	/// <summary>
	/// 自分の位置と相手の位置からラジアン角を取る
	/// </summary>
	/// <param name="mine">自分の位置</param>
	/// <param name="target">相手の位置</param>
	/// <returns>ラジアン角</returns>
	//-------------------------------------------------------------------
	public float GetRad(Vector3 mine,Vector3 target)
	{
		return Mathf.Atan2(target.y - mine.y,
							target.x - mine.x
						);
	}
	//-------------------------------------------------------------------
	/// <summary>
	/// ラジアン角からUnity回転角を取得(下が0、時計回りに359まで)
	/// </summary>
	/// <param name="rad">ラジアン角</param>
	/// <returns>360度方向系</returns>
	//-------------------------------------------------------------------
	public float RadToRotation(float rad)
	{
		return (float)(((rad * 180.0f) / Mathf.PI) % 360.0f);
	}
	//-------------------------------------------------------------------
	/// <summary>
	/// 256度方向系からUnity回転角を得る
	/// </summary>
	/// <param name="ang">256度方向系</param>
	/// <returns>Unity回転角</returns>
	//-------------------------------------------------------------------
	public float AngleToRotation(float ang)
	{
		return (float)(360.0f - (360.0f * ((ang % 256) / 256.0f)));
	}
	//-------------------------------------------------------------------
	/// <summary>
	/// Unity回転角から256度方向系を得る
	/// </summary>
	/// <param name="rot">Unity回転角</param>
	/// <returns>256度方向系</returns>
	//-------------------------------------------------------------------
	public int RotationToAngle(float rot)
	{
		float r = rot;
		if (r<=180.0f)
		{
			r = 360.0f - r;
		}
		else
		{
			r = 180.0f - r;
		}
		return (int)(((256.0f * (360.0f - ((r % 360.0f) / 360.0f)))) % 256);
	}



	/// <summary>
	/// カメラ座標の調整
	/// </summary>
	/// <param name="mine">主人公の座標</param>
	public void SetCamera(Vector3 mine)
	{
		//CAMERA_ROOT.transform.localPosition = new Vector3(0, 0, -10) + (mine / 1.5f);
	}
}
