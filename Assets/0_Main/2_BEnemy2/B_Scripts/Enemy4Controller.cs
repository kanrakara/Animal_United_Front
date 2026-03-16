using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;

public class Enemy4Controller : MonoBehaviour
{
    public GameObject shieldPre; //盾のプレハブデータ
    GameObject shield;
    bool shieldMode; //盾モードのフラグ

    public GameObject shotPre; //ショットのプレハブデータ
    Transform gateTransfome; //ショットの発射場所
    bool shotMode; //ショットモードのフラグ

    [Header("弾の制御")]
    public float shotSpeed =2.0f; //ショットスピード
    public float shotInterval = 1.0f; //ショットの間隔

    [Header("モード切り替え時間")]
    public float waitTime = 3.0f; //コルーチンの待機時間

    void Start()
    {
        shield = Instantiate(shieldPre, transform);
        shield.transform.localPosition = new Vector3(-1.0f, 0, 0);　//盾の生成
        gateTransfome = transform.Find("Gate"); //ショットの発射場所情報を取得

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
        //Debug.Log("弾を打つ");　//後で作る
        GameObject obj = Instantiate(shotPre, gateTransfome.position, Quaternion.identity);
        Rigidbody rbody = obj.GetComponent<Rigidbody>();
        rbody.AddForce(Vector3.left *shotSpeed, ForceMode.Impulse);
    }

    IEnumerator ShotCol() //弾の発射間隔を制御するコルーチン
    {
        while (shotMode)　//shotMode中は繰り返す
        {
            Shot();　//Shotメソッド発動
            yield return new WaitForSeconds(shotInterval);　//1秒待つ
        }
    }



}
