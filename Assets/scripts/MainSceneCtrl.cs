using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class MainSceneCtrl : MonoBehaviour
{
    [Space]
    [SerializeField]
    GameObject ROOT_TITLE;  // タイトル画面一切合切
    [SerializeField]
    Text MSG_GAME_START;
    //[SerializeField]
    //Text MSG_GAME_OVER;
    [SerializeField]
    RectTransform ROOT_GAME_OVER;
    //[SerializeField]
    //UnityEngine.UIElements.Image SPR_GAME_OVER;
    [SerializeField]
    Text MSG_SCORE;
    [SerializeField]
    Text MSG_DEBUG;
    [SerializeField]
    ObjectCtrl MYSHIP;  // 外部参照可能：自機のオブジェクトコントローラー
    [SerializeField]
    ObjectManager MANAGE;   // オブジェクトマネージャーのハンドル
    [SerializeField]
    TouchPanelCtrl TOUCH;   // タッチパネル情報のハンドル
    //-------------------------------------------------定数
    const uint CNT_GAME_CLEAR = 1411000000;  // スコアがこれを越えたらエンディング
    const uint CNT_INTERVAL_MAXIMUM = 60;
    const uint CNT_INTERVAL_MINIMUM = 10;    // これ以下にするとゲームが破綻する



    //-------------------------------------------------変数
    public uint cnt_interval = CNT_INTERVAL_MAXIMUM;        // 戦車の出現フレーム数・待っているとどんどん減ってゆく


    public uint cnt_score = 0;
    public int count = -1;
    int cnt_tank_start = 0;         // ゲーム中常に加算・cnt_intervalを越えると戦車が1台スタート

    public int cnt_game_over = -1;  // 外部参照可能：0以上でゲームオーバー処理

    // ゲームオーバー演出が始まったとき、またはデモ画面に遷移した瞬間
    bool isWaitingForKeyRelease = true;     // キー操作
    bool isWaitingForTouchRelease = true;   // タッチ操作


    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;   // フレームレート60に固定
        MYSHIP.gameObject.SetActive(false); // 自機は最初は非表示
        //cnt_score = 0;
    }

    // Update is called once per frame
    void Update()
    {
        switch(ModeManager.mode)
        {
            /*
                        case ModeManager.MODE.INITIALIZE:
                            //while(true)
                            {
                                cnt_score++;
                                if (MANAGE.SW_BOOT == true)
                                {
                                    ModeManager.ChangeMode(ModeManager.MODE.DEMO);
                                    count = -1;
                                    //break;
                                }
                            }
                            break;
            */
            case ModeManager.MODE.INITIALIZE:
                Debug.Log("MANAGEのオブジェクト名: " + MANAGE.gameObject.name, MANAGE.gameObject);
                if (MANAGE == null)
                {
                    Debug.LogError("MANAGEがアタッチされていません！");
                    Debug.LogError($"MANAGE = {MANAGE}");
                    break;
                }

                if (MANAGE.SW_BOOT == false)
                {
                    // まだ起動していないなら起動命令を出す
                    MANAGE.Boot_ObjectManager_Generate();
                    Debug.Log("BootManagerを呼び出しました");
                }
                else
                {
                    // 準備ができたらDEMOへ
                    ModeManager.ChangeMode(ModeManager.MODE.DEMO);
                    count = -1;
                    //Debug.Log("DEMOモードへ移行します");
                }
                break;
            case ModeManager.MODE.DEMO:
                //Debug.Log($"DEMOモード: count= {count}");
                {
                    if (MANAGE.SW_BOOT == false)
                    {
                        return; // 何もせず脱出
                    }
                    if (
                            (count >= 0)
                        && (count <= 100)
                        )
                    {
                        if (count == 0)
                        {
							SoundManager.Instance.StopSE();
							SoundManager.Instance.PlaySE(4);    // デモ画面
                            ROOT_TITLE.SetActive(true);
                        }
                        ROOT_TITLE.transform.localScale = new Vector3(1, ((float)count / 100.0f), 1);
                    }
                    else
                    {
                        ROOT_TITLE.transform.localScale = new Vector3(1, 1, 1);
                    }
                    // --- デモ画面やタイトル画面での、ゲーム開始入力チェック部分 ---

                    //if (isWaitingForKeyRelease == true)
                    {
                        // 移動入力が「完全に0（離された）」になったら、フラグを解除する
                        if (Mathf.Abs(Input.GetAxisRaw("Vertical")) < 0.1f)
                        {
                            isWaitingForKeyRelease = false;
                        }
                    }
                    //if (isWaitingForTouchRelease == true)
                    { 
                        // タッチパネル離されたらフラグ解除
                        if(TOUCH.on0 == false)
                        {
                            isWaitingForTouchRelease = false;
                        }
                        // キーが離されるまでは、この下の「ゲーム開始判定」を完全に無視（スルー）する！
                    }
                    DebugStation.SetText($"\nisWaitingForKeyRelease: {isWaitingForKeyRelease}\nisWaitingForTouchRelease: {isWaitingForTouchRelease} / TOUCH.on0={TOUCH.on0}\n\n", true);
                    // --- ここから通常のゲーム開始判定 ---
                    bool sw_start = false;  // ゲーム開始のスイッチ
                    if (isWaitingForKeyRelease == false && isWaitingForTouchRelease == false)   // キー・タッチ両方とも無効
                    {
                        if (Input.GetAxisRaw("Vertical") < -0.4f)   // 下が押された(上だとすぐ突っ込んじゃうことに今更気づいた)
                        {
                            sw_start = true;
                        }
                        if (Input.GetMouseButton(0) == true)                      // タッチパネルが押された
                        {
                            sw_start = true;
                        }
                    }
                    if (sw_start==true)
                    {
                        // ここで初めて、新鮮な「カチッ」という入力としてゲームが始まる！
                        sw_start = false; // スイッチは一度立てたらリセットしておく

                        cnt_score = 0;
                        cnt_game_over = -1;
                        cnt_tank_start = 0;
                        cnt_interval = CNT_INTERVAL_MAXIMUM;
                        count = -1;
                        MANAGE.ResetAll(); // 前回の残骸を掃除
                        cnt_score = 0;
                        cnt_interval = CNT_INTERVAL_MAXIMUM;
                        ROOT_TITLE.SetActive(false);
                        ModeManager.RefreshView(ModeManager.MODE.GAME);
                        ModeManager.ChangeMode(ModeManager.MODE.GAME);
                    }
                    if (count % 15 == 0)  //（60fps想定）
                    {
                        MSG_GAME_START.enabled = !MSG_GAME_START.enabled;
                    }
                }
                break;
                case ModeManager.MODE.GAME:
                {
                    if (MANAGE.SW_BOOT == false)
                    {
                        return;
                    }
                    if (count == 0)
                    {
                        MYSHIP.local_type = (int)ObjectManager.TYPE.MYSHIP;
                        MYSHIP.local_mode = (int)ObjectManager.MODE.INIT;    // これで自機が初期化される
                        MYSHIP.count = 0;
                        MYSHIP.gameObject.SetActive(true);
                    }
					if (cnt_interval < CNT_INTERVAL_MINIMUM)
					{
						cnt_interval = CNT_INTERVAL_MINIMUM;
					}
					if (++cnt_tank_start > cnt_interval)
					{
                        int r = Random.Range(1, 5); // レーン番号
                        MANAGE.Set((int)ObjectManager.TYPE.TANK, 0, r, new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1, Random.Range(8, 14));
                        int r2 = Random.Range(1, 5);
                        if (r != r2)    // 同じレーンでは出さない
                        {
                            MANAGE.Set((int)ObjectManager.TYPE.TANK, 0, r2, new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1, Random.Range(8, 14));
                        }
                        cnt_tank_start = 0;
					}
                    if (cnt_game_over == -1)
                    {
                        switch (cnt_interval)
                        {
                            case CNT_INTERVAL_MINIMUM:
                                SoundManager.Instance.PlayBGM(3);
                                break;
                            case 14:
                                SoundManager.Instance.PlayBGM(2);
                                break;
                            case 40:
                                SoundManager.Instance.PlayBGM(1);
                                break;
                            case CNT_INTERVAL_MAXIMUM:
                                SoundManager.Instance.PlayBGM(0);
                                break;
                        }
                    }
                    else
                    {
                        SoundManager.Instance.StopBGM();    // ゲーム終了時は曲を止める
                    }

                    if (cnt_game_over >= 0)
                    {
						cnt_game_over++;
						if (cnt_game_over == 40)
                        {
                            SoundManager.Instance.StopSE();
                            SoundManager.Instance.PlaySE(3);
                            isWaitingForKeyRelease = true;  // ゲームオーバー演出が始まったら、キー入力待ちフラグをセットする（再び「キーが離されるのを待つ」状態にする）
                            isWaitingForTouchRelease = true;    // タッチ入力待ちフラグもセットする
                        }
                        if (
                                (cnt_game_over >= 40)   // 「終劇」時間を掛けて縦に拡大
                            && (cnt_game_over <= 90)
                            )
                        {
                            ROOT_GAME_OVER.localScale = new Vector3(1, (float)(cnt_game_over - 41) / 41.0f, 1);
                            ROOT_GAME_OVER.gameObject.SetActive(true);
                        }
                        else if (cnt_game_over > 90)
                        {
                            ROOT_GAME_OVER.localScale = new Vector3(1, 1, 1);
                        }
                        if (cnt_game_over >= 240)
                        {
                            cnt_game_over = -1;
                            count = -1;
                            // MANAGE.SW_BOOT = false; // ← これは消す！
                            MANAGE.ResetAll(); // オブジェクトを回収するだけにする
                            // BGM再生部分は「値が変わった瞬間」だけ呼ぶ工夫が必要です
                            // 例: if (prev_interval != cnt_interval) { switch... }
                            ROOT_GAME_OVER.localScale = new Vector3(1, 0, 1);
                            ROOT_GAME_OVER.gameObject.SetActive(false);
                            MYSHIP.gameObject.SetActive(false);         // 自機も消す
                            ModeManager.ChangeMode(ModeManager.MODE.DEMO);
                        }
                    }
				}
				break;
        }











		MSG_SCORE.text = KanjiConverter.ToFormattedKanji((long)cnt_score) + "人";
        //MSG_SCORE.text = cnt_score.ToString();  // 一時的な措置
        count++;

        //DebugStation.SetText("cnt_score: " + cnt_score.ToString() + "\ncnt_interval: " + cnt_interval.ToString() + "\ncnt_game_over: " + cnt_game_over.ToString(), false);

    }



#if false
    public void DEBUG_MESSAGE_SET(string str)
    {
        MSG_DEBUG.text = str;
    }
#endif




}
