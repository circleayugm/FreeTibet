//==================================================================================================
//
//	SoundManager.cs
//
//	サウンド発声・停止
//	GameObject SoundManagerにアタッチして使う
//	使用にはタグ「SoundManager」が必須
//	
//	元スクリプト
//	http://zyyxlabo.blogspot.com/2013/03/unitySoundManager-ver.html
//
//	20150331	U.G.M.	コメントを追記などした
//	20150408	U.G.M.	完全に起動したときに初期化ルーチンにメッセージを飛ばすようにした
//	20150728	U.G.M.	iOS対策・起動直後セーブデータが存在しない場合の対応を追加
//	20160127	U.G.M.	BGMを一時消音・再開する関数を追加
//	20160414	U.G.M.	ボイスの音量を記録するように変更(ただしボイス音量は現在別関数で使用するのみ)
//==================================================================================================

using UnityEngine;
using System;
using System.Collections;


// 音管理クラス.
public class SoundManager : MonoBehaviour
{
	protected static SoundManager instance;

	public static SoundManager Instance
	{
		get
		{
			if(instance == null)
			{
				instance = GameObject.FindObjectOfType(typeof(SoundManager)) as SoundManager;
				
				if (instance == null)
				{
					Debug.LogError("SoundManager Instance Error");
				}
			}
			
			return instance;
		}
	}
	
	// 音量.
	public SoundVolume volume = new SoundVolume();
	
	// === AudioSource ===.
	// BGM.
	private AudioSource BGMsource;
	// SE.
	private AudioSource[] SEsources = new AudioSource[16];
	// 音声.
	private AudioSource[] VoiceSources = new AudioSource[16];
	
	// === AudioClip ===.
	// BGM.
	public AudioClip[] BGM;
	// SE.
	public AudioClip[] SE;
	// 音声.
	public AudioClip[] Voice;

	void Awake ()
	{
		GameObject[] obj = GameObject.FindGameObjectsWithTag("SoundManager");
		if( obj.Length > 1 )
		{
			// 既に存在しているなら削除.
			Destroy(gameObject);
		}
		else
		{
			// 音管理はシーン遷移では破棄させない.
			DontDestroyOnLoad(gameObject);

			// 全てのAudioSourceコンポーネントを追加する.

			// BGM AudioSource.
			BGMsource = gameObject.AddComponent<AudioSource>();
			// BGMはループを有効にする.
			BGMsource.loop = true;

			// SE AudioSource.
			for (int i = 0; i < SEsources.Length; i++)
			{
				SEsources[i] = gameObject.AddComponent<AudioSource>();
			}

			// 音声 AudioSource.
			for (int i = 0; i < VoiceSources.Length; i++)
			{
				VoiceSources[i] = gameObject.AddComponent<AudioSource>();
			}

			/*	システム系のオブジェクトをまとめて「System」に置いていた頃の名残.
			GameObject init = GameObject.Find("InitMain");
			if (init != null)
			{
				init.SendMessage("StartRoutine", "SoundManager", SendMessageOptions.DontRequireReceiver);	//初期化処理完了通知.
			}
			*/

			//初期音量設定・ロードして設定する.
			bool createdata = false;	// 新たにデータ作成された？(true=作成された・false=作成されていない).
			if (PlayerPrefs.HasKey("volBGM") == true)
			{							// キー存在すれば音量セット・存在しなければ初期値を設定.
				volume.BGM = PlayerPrefs.GetFloat("volBGM");
			}
			else
			{
				volume.BGM = 0.71f;		// BGM初期値(ちょっとだけ抑えめ).
				PlayerPrefs.SetFloat("volBGM", volume.BGM);
				createdata = true;
			}

			if (PlayerPrefs.HasKey("volSE") == true)
			{							// キー存在すれば音量セット・存在しなければ初期値を設定.
				volume.SE = PlayerPrefs.GetFloat("volSE");
			}
			else
			{
				volume.SE = 0.71f;		// SE初期値(少し低め).
				PlayerPrefs.SetFloat("volSE", volume.SE);
				createdata = true;
			}

			if (PlayerPrefs.HasKey("volVoice") == true)
			{							// キー存在すれば音量セット・存在しなければ初期値を設定.
				volume.Voice = PlayerPrefs.GetFloat("volVoice");
			}
			else
			{
				volume.Voice = 0.71f;		// Voice初期値(そこそこ抑えめ).
				PlayerPrefs.SetFloat("volVoice", volume.Voice);
				createdata = true;
			}

			if (createdata == true)		// 新規登録の場合音量を記録する.
			{
				PlayerPrefs.Save();
			}
		}
	}

	void Update ()
	{
		// ミュート設定.
		BGMsource.mute = volume.Mute;
		foreach(AudioSource source in SEsources ){
			source.mute = volume.Mute;
		}
		foreach(AudioSource source in VoiceSources ){
			source.mute = volume.Mute;
		}
		
		// ボリューム設定.
		BGMsource.volume = volume.BGM;
		foreach(AudioSource source in SEsources ){
			source.volume = volume.SE;
		}
		foreach(AudioSource source in VoiceSources ){
			source.volume = volume.Voice;
		}
	}
	


	// ボリューム設定を記録
	public void SaveVolume(float newBGM,float newSE,float newVoice)
	{
		PlayerPrefs.SetFloat("volBGM", newBGM);
		PlayerPrefs.SetFloat("volSE", newSE);
		PlayerPrefs.SetFloat("volVoice", newVoice);
	}


	// ***** BGM再生 *****.
	// BGM再生.
	public void PlayBGM(int index){
		if( 0 > index || BGM.Length <= index ){
			return;
		}
		// 同じBGMの場合は何もしない.
		if( BGMsource.clip == BGM[index] ){ 
			return;
		}
		BGMsource.Stop();
		BGMsource.clip = BGM[index];
		BGMsource.Play();
	}
	
	// BGM停止.
	public void StopBGM(){
		BGMsource.Stop();
		BGMsource.clip = null;
	}

	// BGM音量下げる(ゼロにせず絞る).
	public void VolumeDownBGM(float percent)
	{
		if (percent > 1.0f)			// 0.0～1.0の間に仕切り.
		{
			percent = 1.0f;
		}
		else if (percent < 0.0f)
		{
			percent = 0.0f;
		}
		volume.BGM = PlayerPrefs.GetFloat("volBGM") * percent;
	}

	// BGM一時停止(音量ゼロ).
	public void MuteBGM()
	{
		volume.BGM = 0.0f;
	}

	// BGM再開(音量戻す).
	public void ResumeBGM()
	{
		volume.BGM = PlayerPrefs.GetFloat("volBGM");
	}
	
	

	// ***** SE再生 *****.
	// SE再生.
	public void PlaySE(int index){
		if( 0 > index || SE.Length <= index ){
			return;
		}
		
		// 再生中で無いAudioSouceで鳴らす.
		foreach(AudioSource source in SEsources){
			if( false == source.isPlaying ){
				source.clip = SE[index];
				source.Play();
				return;
			}
		}  
	}
	
	// SE停止.
	public void StopSE(){
		// 全てのSE用のAudioSouceを停止する.
		foreach(AudioSource source in SEsources){
			source.Stop();
			source.clip = null;
		}  
	}
	
	
	// ***** 音声再生 *****.
	// 音声再生.
	public void PlayVoice(int index){
		if( 0 > index || Voice.Length <= index ){
			return;
		}
		// 再生中で無いAudioSouceで鳴らす.
		foreach(AudioSource source in VoiceSources){
			if( false == source.isPlaying ){
				source.clip = Voice[index];
				source.Play();
				return;
			}
		} 
	}
	
	// 音声停止.
	public void StopVoice(){
		// 全ての音声用のAudioSouceを停止する.
		foreach(AudioSource source in VoiceSources){
			source.Stop();
			source.clip = null;
		}  
	}
}

// 音量クラス.
[Serializable]
public class SoundVolume{
	public float BGM = 1.0f;
	public float Voice = 1.0f;
	public float SE = 1.0f;
	public bool Mute = false;
	
	public void Init(){
		BGM = 1.0f;
		Voice = 1.0f;
		SE = 1.0f;
		Mute = false;
	}
}
