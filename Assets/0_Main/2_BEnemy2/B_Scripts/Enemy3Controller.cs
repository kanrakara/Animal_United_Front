using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class Enemy3Controller : MonoBehaviour
{

    GameObject player;
    Rigidbody rbody;
    Rigidbody enemy3;

    [Header("敵の動き制御")]
    public float speed = 2.0f; //移動スピード
    public float seachlength = 5.0f; //索敵範囲
    public float attackSpeed = 0.2f; //突進スピード
    public float toDestroyTime = 10.0f; //オブジェクト消滅までの時間


    void Start()
    {
        rbody = GetComponent<Rigidbody>(); //Rigidbodyを取得
        player = GameObject.FindGameObjectWithTag("Player"); //プレイヤー情報を取得

        rbody.linearVelocity = Vector3.left * speed;
    }


    void Update()
    {
        if (CheckLength(player.transform.position))
        {
            Attack();
        }

        StartCoroutine(DestroyCol());　//消滅コルーチン発動
    }


    void Attack()
    {
        float dx = player.transform.position.x - rbody.transform.position.x;
        float dy = player.transform.position.y - rbody.transform.position.y;

        //Xの差（底辺）、Yの差（高さ）から逆算して角度を求める※ラジアン係数（円周率）で出てくる
        float rad = Mathf.Atan2(dy, dx);

        //角度情報から改めて、長辺を1としたときのxとyの比率をそれぞれ入手
        float x = Mathf.Cos(rad);
        float y = Mathf.Sin(rad);
        Vector3 v = new Vector3(x, y, 0) * attackSpeed;
        rbody.AddForce(v, ForceMode.Impulse);
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
}
