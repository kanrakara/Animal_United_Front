using UnityEngine;
using System.Collections;

public class Enemy4Controller : MonoBehaviour
{
    public GameObject shieldPre;
    public GameObject shot;
    GameObject shield;

    public float waitTime = 3.0f; //コルーチンの待機時間
    bool shieldMode; //盾モードのフラグ
    bool shotMode; //ショットモードのフラグ


    void Start()
    {
        shield = Instantiate(shieldPre, transform);
        shield.transform.localPosition = new Vector3(-1.0f, 0, 0);　//盾の生成

        StartCoroutine(ModeChange()); //モード切替コルーチンを発動させる
    }

    void Update()
    {
        if (shieldMode)
        {
            shield.SetActive(true);
        }
        if (shotMode)
        {
            shield.SetActive(false);
        }
    }

    IEnumerator ModeChange() //モードの切り替えコルーチン
    {
        while (true) //waitTime経過毎にフラグをOnOff
        {
            shieldMode = true;
            shotMode = false;

            yield return new WaitForSeconds(waitTime);

            shieldMode = false;
            shotMode = true;

            StartCoroutine(ShotCol());　//ショットコルーチンを発動

            yield return new WaitForSeconds(waitTime);
        }

    }

    void Shot() //弾を撃つメソッド
    {
        Debug.Log("弾を打つ");　//後で作る
        return;
    }

    IEnumerator ShotCol() //弾を1秒ごとに撃つコルーチン
    {
        while (shotMode)　//shotMode中は繰り返す
        {
            Shot();　//Shotメソッド発動
            yield return new WaitForSeconds(1.0f);　//1秒待つ
        }
    }



}
