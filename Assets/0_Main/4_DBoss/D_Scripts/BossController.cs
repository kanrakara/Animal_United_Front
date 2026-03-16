using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{

    public float bossHp = 20f;     // ボスの体力
    bool inDamage = false;      // ダメージ管理フラグ
    public GameObject bossVisual;      // 子オブジェクト（絵の部分）

    //GameObject explosionPrefab;   // 爆発エフェクトのプレハブ
    private bool isDead = false;    // 死亡フラグ

    // 移動関連
    public Vector3 initialPosition;     // ボスの初期位置
    Coroutine assaultCol;       //急襲コルーチン情報の参照用
    Coroutine repositionCol;        // 逃亡コルーチン情報の参照用
    bool moveOn;        // 移動フラグ
    Vector3 targetPosition;     // 目的地
    bool isOffScreen;       // 画面外フラグ
    public float landHeight = 2.5f;       // 降りてきた時の高さ
    public float targetHeight;      // 上下移動の距離
    public float bossMoveSpeed = 3.0f;  // 移動スピード
    public float bossDamper = 0.9f;     // 移動力の減衰（摩擦力）
    Vector3 velocity = Vector3.zero;    // 計算用

    // 攻撃関連
    public float detectionRange = 5.0f;     // 遠近切り替え距離
    public float waitTime = 2.0f;           // 攻撃後の隙の時間
    Vector3 targetPoint;                    // 各攻撃の発生場所

    // ボスのショット関連
    public GameObject bossShotPrefab;       // 弾のプレハブ
    public float bossShotSpeed = 5.0f;      // 弾速
    public float spreadAngle = 15f;         // 弾の広がる角度

    // ボスの近接攻撃関連
    public GameObject bossSlashPrefab;      // 対象スラッシュプレハブ

    // プレイヤー取得用
    GameObject player;

    void Start()
    {
        // playerを取得しておく
        player = GameObject.FindGameObjectWithTag("Player");
        // 指定した初期位置に移動
        transform.position = initialPosition;
        // 画面外フラグをオンに
        isOffScreen = true;

    }


    void Update()
    {
        // 体力が残っている場合
        if (bossHp > 0)
        {

            // 行動の条件分岐
            // 画面外フラグがON∧急襲コルーチンがnull
            if (isOffScreen && assaultCol == null)
            {
                // 急襲コルーチンの開始
                assaultCol = StartCoroutine(Assault());
            }
            // 画面外フラグがOFF∧逃亡コルーチンがnull
            else if (!isOffScreen && repositionCol == null)
            {
                // 逃亡コルーチンの開始
                repositionCol = StartCoroutine(Reposition());
            }


            // 移動 （移動フラグがONで常に実行）
            if (moveOn)
            {
                // 力として、スピード値を乗算した目標へのベクトルを作成
                Vector3 force = (targetPosition - transform.position) * bossMoveSpeed;
                // 時間を乗算し、速度を出す。慣性のように、前のフレームの速度を引き継ぐ
                velocity += force * Time.deltaTime;
                // 摩擦のように、速度を常に減衰させる
                velocity *= bossDamper;

                // 速度に時間を乗算し、このフレームでの移動先を指定
                transform.position += velocity * Time.deltaTime;

                // 目標に十分近づいたら近づいたら目標地点で止める、差の絶対値を比較する
                if (Mathf.Abs(transform.position.y - targetPosition.y) < 0.01f)
                {
                    transform.position = targetPosition;
                    velocity = Vector3.zero;
                    moveOn = false; // 到着したら移動フラグをオフにする
                }
            }
        }

        // ダメージ中であれば点滅
        if (inDamage)
        {
            float val = Mathf.Sin(Time.time * 50);
            // 0より大きければ表示(true)、小さければ非表示(false)
            bossVisual.SetActive(val > 0);
        }
    }

    // 急襲コルーチン
    IEnumerator Assault()
    {
        // playerがいなくなる可能性があるので、nullチェックを入れておく
        if (player != null)
        {
            // 移動先を指定し、移動フラグをオンにする
            targetPosition = new Vector3(transform.position.x, landHeight, 0f);
            moveOn = true;

            // 移動の待ち時間
            yield return new WaitForSeconds(2.0f);

            // 攻撃関連（近ければ近接、遠ければショット）
            float distance = Vector3.Distance(transform.position, player.transform.position);

            if (distance < detectionRange) { BossSlash(); }
            else { BossShot(); }

            // 攻撃後の隙
            yield return new WaitForSeconds(waitTime);

            // 画面外フラグOFF、コルーチンを空に
            isOffScreen = false;
            assaultCol = null;
        }
    }

    // 逃亡コルーチン
    IEnumerator Reposition()
    {
        // 移動先を指定し、移動フラグをOnにする
        targetPosition = new Vector3(transform.position.x, targetHeight, 0f);
        moveOn = true;

        // 移動の待ち時間
        yield return new WaitForSeconds(2.0f);
        // 画面外フラグON、コルーチンを空に
        isOffScreen = true;
        repositionCol = null;
        // X軸をランダムに移動(画面の幅に合わせて調整)
        float randomX = Random.Range(-8.0f, 8.0f);
        transform.position = new Vector3(randomX, transform.position.y, 0f);
    }



    void BossShot()
    {
        // playerがいなくなる可能性があるので、nullチェックを入れておく
        if (player == null) { return; }

        // Playerと自身のX座標の差、Y座標の差
        float dx = player.transform.position.x - transform.position.x;
        float dy = player.transform.position.y - transform.position.y;

        // 正規化（大きさを1にする）
        Vector3 direction = new Vector3(dx, dy, 0).normalized;

        // 発射場所をプレイヤー側にする
        targetPoint = transform.position + (direction * 2);

        // 3つの方向を配列で定義（下、中央、上）
        float[] angles = { -spreadAngle, 0f, spreadAngle };

        foreach (float angle in angles)
        {
            // 方向を変化させる
            Vector3 spreadDirection = Quaternion.Euler(0, 0, angle) * direction;

            // (x,y)とArcTanを使って、角度（Z軸の回転）を出す
            float rad = Mathf.Atan2(spreadDirection.y, spreadDirection.x);
            // RadianをDegreeに変換
            float rotationZ = rad * Mathf.Rad2Deg;
            // 回転の型に入れる。画像の角度によってはここで調整
            Quaternion rotation = Quaternion.Euler(0, 0, rotationZ);

            // 弾を生成
            GameObject bullet = Instantiate(bossShotPrefab, targetPoint, rotation);

            // 弾に速度を与える
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            rb.AddForce(spreadDirection * bossShotSpeed, ForceMode.Impulse);
        }
    }

    void BossSlash()
    {
        // playerがいなくなる可能性があるので、nullチェックを入れておく
        if (player == null) { return; }

        // Playerと自身のX座標の差、Y座標の差
        float dx = player.transform.position.x - transform.position.x;
        float dy = player.transform.position.y - transform.position.y;

        // 正規化（大きさを1にする）
        Vector3 direction = new Vector3(dx, dy, 0).normalized;

        // 発射場所をプレイヤー側にする
        targetPoint = transform.position + (direction * 2);

        // プレイヤーの方向を向く角度を計算、shotの2行を1行にまとめた
        float rotationZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        // 回転の型に入れる。画像の角度によってはここで調整
        Quaternion rotation = Quaternion.Euler(0, 0, rotationZ);

        // Slash を生成
        GameObject slash = Instantiate(bossSlashPrefab, targetPoint, rotation);
    }



    // ダメージ関連
    void OnTriggerEnter(Collider other)
    {
        // ダメージ中じゃない∧ぶつかった相手がPlayerAttackだったら
        if (!inDamage && other.gameObject.tag == "PlayerAttack")
        {
            // ぶつかった相手のゲームオブジェクトが持っているスクリプトを取得
            // ！ダメージ量の記載場所が変更になるかも！
            SlashHitbox slashHit = other.gameObject.GetComponent<SlashHitbox>();

            bossHp -= slashHit.damage;    // 体力を減少
            inDamage = true;        // ダメージ中ON
            Invoke("DamageEnd", 0.25f);     // 0.25秒後にダメージフラグ解除

            // 体力がなくなったら死亡
            if (bossHp <= 0 && !isDead)
            {
                isDead = true;      // 二度と呼ばれないようにする
                StartCoroutine(DeathRoutine());     // 演出の開始
            }
        }
    }

    void DamageEnd()
    {
        inDamage = false;
        bossVisual.SetActive(true);     // 最後は必ず表示する
    }

    IEnumerator DeathRoutine()
    {
        // 移動や攻撃のコルーチンを止める
        // StopAllCoroutines() は、そのメソッドを呼び出したスクリプト（インスタンス）から開始されたコルーチンだけを停止させる
        StopAllCoroutines(); 
        moveOn = false;

        // その場でガタガタ震える演出
        Vector3 pos = transform.position;
        for (int i = 0; i < 20; i++)
        {
            // Random.insideUnitSphere は、Unityで 半径1の球体の中の、どこかランダムな地点を1つ取得する機能
            transform.position = pos + Random.insideUnitSphere * 0.2f;
            yield return new WaitForSeconds(0.05f);
        }
        transform.position = pos;

        // 爆発エフェクトの生成
        //if (explosionPrefab != null)
        //{
        //    Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        //}

        // ボスの見た目を消す
        if (bossVisual != null) bossVisual.SetActive(false);

        // 少し待ち時間を入れてから、オブジェクトを完全に削除
        yield return new WaitForSeconds(1.0f);
        Destroy(gameObject);

        Debug.Log("ボスを撃破！");
    }
}
