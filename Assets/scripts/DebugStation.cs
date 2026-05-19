using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugData
{
	public string message { get; set; }
	public int count { get; set; }
	public ModeManager.MODE scene { get; set; }
	public Color color { get; set; }
	public Color outline { get; set; }

	/// <summary>
	///		DebugData構造体の新規作成・初期化
	/// </summary>
	/// <returns>初期化されたDebugData構造体</returns>
	public DebugData Generate()
	{
		DebugData data = new DebugData()
		{
			message = "",
			count = 0,
			scene = ModeManager.MODE.INITIALIZE,
			color = new Color(1, 1, 0, 1),
			outline = new Color(1, 1, 0, 0.3f)
		};
		return data;
	}
	/// <summary>
	///		DebugData構造体の設定
	/// </summary>
	/// <param name="message">デバッグ情報</param>
	/// <param name="count">カウンター</param>
	/// <param name="scene">ゲームシーン</param>
	/// <param name="color">文字色</param>
	/// <param name="outline">文字アウトライン色</param>
	/// <returns></returns>
	public DebugData Set(string message, int count, ModeManager.MODE scene, Color color, Color outline)
	{
		DebugData data = new DebugData();
		data.message = message;
		data.count = count;
		data.scene = scene;
		data.color = color;
		data.outline = outline;
		return data;
	}
}

public class DebugStation : MonoBehaviour
{
	[SerializeField]
	public Text MSG_MAIN;			// メインログ
	[SerializeField]
	public Text MSG_COUNTER;		// カウンター
	[SerializeField]
	public Text MSG_SCENE;			// 今いるシーン
	[SerializeField]
	public Outline[] MSG_OUTLINE;	// それぞれのTextのアウトライン

	private static bool sw_onmemory = true;	// インスタンスをシーンに残すスイッチ

	private static int count = 0;											// カウンター
	private static ModeManager.MODE scene = ModeManager.MODE.INITIALIZE;	// ゲームモード
	private static string debug_log = "DebugStation:Ready.";                // デバッグログ

	private static DebugData debug_data = new DebugData().Generate();

	private static int DebugStationBootCount=0;
	// Start is called before the first frame update
	void Start()
	{
		DebugStationBootCount++;
		if (DebugStationBootCount != 1)
		{
			GameObject.Destroy(this);
		}
		else
		{
			DontDestroyOnLoad(this);
		}
		MSG_COUNTER = GameObject.Find("count_DebugStation").GetComponent<Text>();
		MSG_SCENE = GameObject.Find("scene_DebugStation").GetComponent<Text>();
		MSG_MAIN = GameObject.Find("text_DebugStation").GetComponent<Text>();
		MSG_OUTLINE[0] = GameObject.Find("text_DebugStation").GetComponent<Outline>();
		MSG_OUTLINE[1] = GameObject.Find("scene_DebugStation").GetComponent<Outline>();
		MSG_OUTLINE[2] = GameObject.Find("count_DebugStation").GetComponent<Outline>();
	}

	// Update is called once per frame
	void Update()
	{
		MSG_COUNTER.text = "c:" + debug_data.count;
		MSG_MAIN.text = debug_data.message;
		MSG_SCENE.text = ModeManager.MODE_SCENE[(int)debug_data.scene];

        MSG_MAIN.color = debug_data.color;
		MSG_COUNTER.color = debug_data.color;
		MSG_SCENE.color = debug_data.color;
		MSG_OUTLINE[0].effectColor = debug_data.outline;
		MSG_OUTLINE[1].effectColor = debug_data.outline;
		MSG_OUTLINE[2].effectColor = debug_data.outline;

		count++;
	}

	/// <summary>
	///		カウンター数値更新
	/// </summary>
	/// <param name="cnt">カウンター</param>
	public static void SetCount(int cnt)
	{
		debug_data.count = cnt;
	}
	/// <summary>
	///		現在のゲームシーン更新
	/// </summary>
	/// <param name="scn">ゲームシーンのenum</param>
	public static void SetScene(ModeManager.MODE scn)
	{
		debug_data.scene = scn;
	}
	/// <summary>
	///		デバッグ情報更新
	/// </summary>
	/// <param name="msg">デバッグ情報</param>
	/// <param name="erase">trueで一旦消去・falseでつなげて表示</param>
	public static void SetText(string msg, bool erase)
	{
		if (erase == true)
		{
			debug_data.message = "";
		}
		debug_data.message += msg;
	}
	/// <summary>
	///		デバッグ情報の文字色変更
	/// </summary>
	/// <param name="c">文字色</param>
	/// <param name="o">文字アウトライン色</param>
	public static void SetTextColor(Color c, Color o)
	{
		debug_data.color = c;
		debug_data.outline = o;
	}
	/// <summary>
	///		DebugData構造体に乗せての一括処理
	/// </summary>
	/// <param name="data">デバッグ情報の構造体</param>
	/// <param name="erase">trueで一旦消去・falseでつなげて表示</param>
	public static void SetAll(DebugData data,bool erase)
	{
		if (erase == true)
		{
			debug_data.message = "";
		}
		debug_data.message += data.message;
		debug_data.count = data.count;
		debug_data.scene = data.scene;
		debug_data.color = data.color;
		debug_data.outline = data.outline;
	}
}
