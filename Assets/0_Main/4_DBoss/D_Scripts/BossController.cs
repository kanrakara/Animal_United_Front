using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    // ボスの体力
    public int bossHp = 10;

    // 移動関連
    Coroutine assaultCol;       //急襲コルーチン情報の参照用
    Coroutine repositionCol;        // 逃亡コルーチン情報の参照用
    bool moveOn;        // 移動フラグ
    Vector3 targetPosition;     // 目的地
    bool isOffScreen;       // 画面外フラグ
    float landHeight;
    public float targetHeight;      // 上下移動の距離
    public float bossMoveSpeed = 3.0f;  // 移動スピード
    public float bossDamper = 0.9f;     // 移動力の減衰（摩擦力）
    Vector3 velocity = Vector3.zero;    // 計算用


    // ボスのショット関連
    public GameObject bossShotPrefab;       // 弾のプレハブ
    public float bossShotSpeed = 5.0f;      // 弾速
    public float spreadAngle = 15f;         // 弾の広がる角度

    // ボスの近接攻撃関連
    public GameObject bossSlashPrefab;      // 対象スラッシュプレハブ

    // 各攻撃の発生場所
    Vector3 targetPoint;

    // プレイヤー取得用
    GameObject player;

    void Start()
    {
        // playerを取得しておく
        player = GameObject.FindGameObjectWithTag("Player");
        // 画面外フラグをオンに
        isOffScreen = true;
        // 下降時のy座標を直接指定
        landHeight = 2.5f;
    }


    void Update()
    {
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



        // 移動
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

    // 急襲コルーチン
    IEnumerator Assault()
    {
        // playerがいなくなる可能性があるので、nullチェックを入れておく
        if (player != null)
        {
            // 移動先を指定し、移動フラグをオンにする
            targetPosition = new Vector3(transform.position.x, landHeight, 0f);
            moveOn = true;

            // 移動の時間待ち
            yield return new WaitForSeconds(2.0f);

            // 攻撃関連（近ければ近接、遠ければショット）
            float distance = Vector3.Distance(transform.position, player.transform.position);

            if (distance < 5.0f) { BossSlash(); }
            else { BossShot(); }

            // 攻撃後の隙
            yield return new WaitForSeconds(2.0f);

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

        // 移動の時間待ち
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
}
