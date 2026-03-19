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
    public float shotSpeed = 2.0f; //ショットスピード
    public float shotInterval = 1.0f; //ショットの間隔

    [Header("モード切り替え時間")]
    public float waitTime = 3.0f; //コルーチンの待機時間

    [Header("ステータス")]
    public float life = 10.0f; //体力
    public float enemy4attackPower = 1.0f; //攻撃力

    bool inDamage; //ダメージ管理フラグ

    [Header("ダメージ点滅するレンダラー")]
    public Renderer[] rend; //子オブジェクトの情報

    GameObject player; //プレイヤー情報

    void Start()
    {
        shield = Instantiate(shieldPre, transform);
        shield.transform.localPosition = new Vector3(-1.0f, 0, 0);　//盾の生成
        gateTransfome = transform.Find("Gate"); //ショットの発射場所情報を取得

        StartCoroutine(ModeChange()); //モード切替コルーチンを発動させる

        player = GameObject.FindGameObjectWithTag("PlayerFollower");　//プレイヤー情報を取得

        rend = GetComponentsInChildren<Renderer>();
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

        //もしダメージ管理フラグが立っていたら点滅処理
        if (inDamage)
        {
            //Sin関数の角度に経過時間（一定リズムの値）を与えると、等間隔で+と－の結果が得られる
            float val = Mathf.Sin(Time.time * 50);

            //等間隔で変わっているであろうvalの値をチェックして＋の時間帯は表示、－の時間帯は非表示
            //if (val > 0)
            //{
            //    rend.enabled = true;
            //}
            //else
            //{
            //    rend.enabled = false;
            //}

            bool isVisible = val > 0;

            foreach(Renderer r in rend)
            {
                r.enabled = isVisible; 
            }
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
        //Debug.Log("弾を打つ");
        GameObject obj = Instantiate(shotPre, gateTransfome.position, Quaternion.identity);
        Rigidbody rbody = obj.GetComponent<Rigidbody>();
        rbody.AddForce(Vector3.left * shotSpeed, ForceMode.Impulse);
    }

    IEnumerator ShotCol() //弾の発射間隔を制御するコルーチン
    {
        while (shotMode)　//shotMode中は繰り返す
        {
            Shot();　//Shotメソッド発動
            yield return new WaitForSeconds(shotInterval);　//1秒待つ
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "PlayerAttack") //ぶつかった相手がPlayerAttackなら
        {
            if (!shieldMode || player.transform.position.x > transform.position.x)
            {

                if (life > 0) //まだ体力が残っている場合
                {
                    //PlayerAttackのスクリプトを取得　ここは必要に応じて
                    //*****Controller **** = collision.gameObject.GetComponent<*****Controller>();
                    //Life -= *****Cnt.attackPower;

                    life -= 1; //体力を減少

                    inDamage = true; //ダメージ管理フラグを立てる
                    Invoke("DamageEnd", 0.25f); //ダメージ後0.25秒待ってからダメージ管理フラグをおろす
                }
                else //体力が残っていない場合は消滅
                {
                    Destroy(gameObject, 0.3f);
                }
            }
        }
    }

    void DamageEnd()　//ダメージ終了管理
    {
        inDamage = false;
        //ダメージ終了と同時に確実に姿を表示させる（点滅終了）
        foreach(Renderer r in rend)
        {
            r.enabled = true;
        }
    }

}
