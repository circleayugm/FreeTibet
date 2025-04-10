//==================================================================================================
//
//	FileOutput.cs
//
//	デバッグ用ログファイル出力
//	GameObject FileOutputにアタッチして使う
//	使用にはタグ「FileOutput」が必須
//	
//	20150320		とりあえず作成
//	20150323		外部からセーブされるファイルパスを取り出せるようにした
//	20150408		完全に起動したときに初期化ルーチンにメッセージを飛ばすようにした
//	20160325		コンピュータでのデバッグ時の出力ディレクトリを変更
//==================================================================================================

using UnityEngine;
using System.Collections;
using System.IO;			//System.IO.FileInfo, System.IO.StreamReader, System.IO.StreamWriter
using System;				//Exception
using System.Text;			//Encoding

public class FileOutput : MonoBehaviour {

	//スイッチ類.
	const bool sw_enable = true;							//そもそも出力するか(true=出力する・false=出力しない).
	const bool sw_onmemory = true;							//新しいシーン移行時に自分を残すか(true=残す・false=残さない).
	const bool sw_newfile = true;							//毎回新しくファイルを作り直すか(true=作り直す・false=どんどん追記してゆく).
	const bool sw_debuglog = false;							//Debug.Logにも出力するか(true=出力する・false=出力しない).

	//定義時変更の必要な変数.
	static string ApplicationName = "TempShooter";					//アプリケーション識別名.
	static string outputFileName = "DebugLog.utf8.txt";		//ファイルネーム.
	string ApplicationNewFileMessage = "新規作成されました。";		//ファイルが作成された(作り直された)際のメッセージ.

	//変更される変数.
	static public string outputFilePath = "/";				//ファイルパス・起動時に大雑把に振り分け.
	static public string displaySaveFile = "";				//表示用ファイルパスが格納される・外部から読み出し可能.



	//-------------------------------------------------------------------
	//	void Awake()
	//		コンポーネント起動直後に呼ばれる
	//-------------------------------------------------------------------
	void Awake()
	{
		//if (CompileSW.Debug == false) return;	//デバッグモードでなければ機能しない.
		GameObject[] obj = GameObject.FindGameObjectsWithTag("FileOutput");
		if (obj.Length > 1)		// 既に存在しているなら削除.
		{
			Destroy(gameObject);
		}
		else
		{
			if (sw_onmemory == true)					//ファイルログ処理はシーン遷移では破棄させない.
			{
				DontDestroyOnLoad(gameObject);
			}

			switch (Application.platform)				//プラットフォーム別でファイル保存位置を変更する.
			{
				case RuntimePlatform.WindowsPlayer:
				case RuntimePlatform.WindowsEditor:
				case RuntimePlatform.OSXPlayer:
				case RuntimePlatform.OSXEditor:
					outputFilePath = Application.dataPath;
					outputFilePath += "/..";	// プロジェクトAssets内から3つ落としてプロジェクトルートに置くようにする・ここはプロジェクト毎に調整.
					break;
				case RuntimePlatform.Android:
					outputFilePath = Application.persistentDataPath;
//					outputFilePath = "/data/data/jp.gamersuniverse.mphomerun/files";	//Android本体に保存される場合の場所.
					break;
				case RuntimePlatform.IPhonePlayer:
					outputFilePath = Application.temporaryCachePath;
					break;
				default:
					outputFilePath = Application.dataPath;
					break;
			}
			if (sw_newfile == true)		//必要ならファイル削除して再作成.
			{
				DestroyFile();
			}
			ReadFile();					//初回読み込み.
			displaySaveFile = outputFilePath + "/" + outputFileName;	//出力ファイルの場所.

			//テスト用.
			FileOutput.Log("boot.");
			FileOutput.Log("Application.platform=" + Application.platform);
			FileOutput.Log("Screen Resolution=(" + Screen.width + "," + Screen.height + ")");
			FileOutput.Log("Application.dataPath= " + Application.dataPath);
			FileOutput.Log("Application.persistentDataPath= " + Application.persistentDataPath);
			FileOutput.Log("Application.temporaryCachePath= " + Application.temporaryCachePath);
			FileOutput.Log("このファイルの場所→" + outputFilePath + "/" + outputFileName);

			GameObject init = GameObject.Find("InitMain");
			if (init != null)
			{
				init.SendMessage("StartRoutine", "FileOutput", SendMessageOptions.DontRequireReceiver);	//初期化処理完了通知.
			}
		}
	}

	
	
	//-------------------------------------------------------------------
	//	void Start()
	//		ゲームオブジェクト配置後に呼ばれる
	//-------------------------------------------------------------------
	void Start()
	{

	}



	//-------------------------------------------------------------------
	//	void Update()
	//		毎フレーム呼ばれる
	//-------------------------------------------------------------------
	void Update()
	{

	}



	//-------------------------------------------------------------------
	//	static public void Log(string txt)
	//		テキストの内容をログに書き出す(外部呼び出し)
	//	string txt=書き出す内容
	//-------------------------------------------------------------------
	static public void Log(string txt)
	{
		//if (CompileSW.Debug == false) return;	//デバッグモードでなければ機能しない.
		string s = DateTime.Now.ToString() + " : " + txt;		//フォーマット整えて書き出し.
 		WriteFile(s);
	}



	//-------------------------------------------------------------------
	//	static void WriteFile(string txt)
	//		実際にファイルに書き出す
	//	string txt=書き出す内容
	//-------------------------------------------------------------------
	static void WriteFile(string txt)
	{
		//if (CompileSW.Debug == false) return;	//デバッグモードでなければ機能しない.
		if (sw_enable == false) return;	//動作フラグ立っていない場合動かない.
		FileInfo fi = new FileInfo(outputFilePath + "/" + outputFileName);	//ファイル作成.
		try
		{
			using (StreamWriter sw = fi.AppendText())							//追記モード.
			{
				sw.WriteLine(txt);													//ファイル出力.
				if (sw_debuglog == true)											//必要ならデバッグログにも出力する.
				{
					Debug.Log(outputFileName + " > " + txt);
				}
			}
		}
		catch (Exception)
		{
		}
	}



	//-------------------------------------------------------------------
	//	void ReadFile()
	//		ファイルから読み込む
	//	string txt=書き出す内容
	//-------------------------------------------------------------------
	void ReadFile()
	{
		//if (CompileSW.Debug == false) return;	//デバッグモードでなければ機能しない.
		if (sw_enable == false) return;	//動作フラグ立っていない場合動かない.
		FileInfo fi = new FileInfo(outputFilePath + "/" + outputFileName);	//ファイルあるかチェック.
		try
		{																		//あった場合.
			using (StreamReader sr = new StreamReader(fi.OpenRead(), Encoding.UTF8))	//UTF8として読み込み.
			{
				string s = sr.ReadToEnd();
			}
		}
		catch (Exception)
		{																		//なかった場合.
			Log(ApplicationNewFileMessage + Environment.NewLine);						//新規ファイルメッセージ使ってファイル作成.
		}
	}



	//-------------------------------------------------------------------
	//	void DestroyFile()
	//		ファイルが存在したら削除する
	//-------------------------------------------------------------------
	void DestroyFile()
	{
		//if (CompileSW.Debug == false) return;	//デバッグモードでなければ機能しない.
		if (sw_enable == false) return;	//動作フラグ立っていない場合動かない.
		FileInfo fi = new FileInfo(outputFilePath + "/" + outputFileName);	//ファイルあるかチェック.
		if (fi.Exists == true)												//ファイルあったら.
		{
			fi.Delete();														//削除して作り直し(作り直し自体はReadFileがする).
		}
	}




}
