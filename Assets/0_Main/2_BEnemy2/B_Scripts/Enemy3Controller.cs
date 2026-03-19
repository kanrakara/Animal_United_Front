using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class Enemy3Controller : MonoBehaviour
{

    GameObject player;
    Rigidbody rbody;
    Rigidbody enemy3;

    bool attackOn = false; //攻撃中のフラグ

    [Header("敵の動き制御")]
    public float speed = 2.0f; //移動スピード
    public float seachlength = 5.0f; //索敵範囲
    public float attackSpeed = 0.2f; //突進スピード
    public float toDestroyTime = 10.0f; //オブジェクト消滅までの時間

    [Header("ステータス")]
    public float life = 10.0f; //体力
    public float enemy3attackPower = 1.0f; //攻撃力

    bool inDamage; //ダメージ管理フラグ

    [Header("ダメージ点滅するレンダラー")]
    public Renderer[] rend; //子オブジェクトの情報

    void Start()
    {
        rbody = GetComponent<Rigidbody>(); //Rigidbodyを取得
        player = GameObject.FindGameObjectWithTag("PlayerFollower"); //プレイヤー情報を取得

        rbody.linearVelocity = Vector3.left * speed;

        StartCoroutine(DestroyCol()); //消滅コルーチン発動

        rend = GetComponentsInChildren<Renderer>();
    }


    void Update()
    {
        if (CheckLength(player.transform.position))
        {
            Attack();
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

            foreach (Renderer r in rend)
            {
                r.enabled = isVisible;
            }
        }
    }


    void Attack()
    {
   

        if (attackOn) //アタックフラグ中であれば何もしない
        {
            return;
        }

        rbody.linearVelocity = Vector3.zero;

        //Debug.Log("発動");

        float dx = player.transform.position.x - transform.position.x;
        float dy = player.transform.position.y - transform.position.y;

        //Xの差（底辺）、Yの差（高さ）から逆算して角度を求める※ラジアン係数（円周率）で出てくる
        float rad = Mathf.Atan2(dy, dx);

        //角度情報から改めて、長辺を1としたときのxとyの比率をそれぞれ入手
        float x = Mathf.Cos(rad);
        float y = Mathf.Sin(rad);
        Vector3 v = new Vector3(x, y, 0) * attackSpeed;
        rbody.AddForce(v, ForceMode.Impulse);

        attackOn = true;

    }


    bool CheckLength(Vector2 targetPos) //プレイヤーとの距離をチェック
    {
        bool ret = false;　//最初はfolse
        float d = Vector2.Distance(transform.position, targetPos);
        if (seachlength >= d)
        {
            ret = true; //索敵範囲内ならtrue
        }
        return ret;
    }

    IEnumerator DestroyCol() //時間が来たら消滅するコルーチン
    {
        yield return new WaitForSeconds(toDestroyTime);
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "PlayerAttack") //ぶつかった相手がPlayerAttackなら
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

    void DamageEnd()　//ダメージ終了管理
    {
        inDamage = false;
        //ダメージ終了と同時に確実に姿を表示させる（点滅終了）
        foreach (Renderer r in rend)
        {
            r.enabled = true;
        }
    }
}
