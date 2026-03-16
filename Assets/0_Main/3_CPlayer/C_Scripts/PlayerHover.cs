using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

public class PlayerHover : MonoBehaviour
{
    //コンポーネント系
    PlayerMove playerMove;
    CharacterController controller;

    [Header("ガードオブジェクト")]
    public GameObject playerGuard;

    [Header("重力カット")]
    public float gravityLoss = 0.9f;

    [Header("初期上昇力・最大上昇力")]
    public float initialUpForce = 2.0f;
    public float maxUpForce = 30.0f;

    [Header("１秒当たりの上昇力")]
    public float upForceRate = 25.0f; // 1秒あたりの上昇力増加量

    [Header("ホバリング時間")]
    public float hoverTime = 1.5f;

    float currentHoverTime = 0f; //ホバー経過時間
    bool jumpButtonHold = false; // ジャンプボタンフラグ

    void Awake()
    {
        playerMove = GetComponent<PlayerMove>();
        controller = GetComponent<CharacterController>();
    }

    //ジャンプボタンが押されている間フラグを立てる
    void OnJump(InputValue value)
    {
        jumpButtonHold = value.isPressed;
    }

    //インタクタボタンが押されている間ガードメソッド
    void OnInteract(InputValue value)
    {
        Gourd(value.isPressed);
    }

    void Update()
    {
        // 地面にいる場合はホバリング時間をリセット
        if (controller.isGrounded)
        {
            currentHoverTime = 0f;
        }

        //ジャンプボタン　&　空中　& ホバー時間が残っている
        if (jumpButtonHold && !controller.isGrounded && currentHoverTime < hoverTime)
        {
            currentHoverTime += Time.deltaTime; //ホバー時間が経過

            // 経過時間に応じてupForceを計算
            float currentUpForce = initialUpForce + (upForceRate * currentHoverTime);
            currentUpForce = Mathf.Clamp(currentUpForce, initialUpForce, maxUpForce); // 最大値でクランプ

            // PlayerのMoveDirectionを取得
            Vector3 currentMoveDirection = playerMove.MoveDirection;

            //playerMoveの重力を変数gravityLossの分だけカットして弱める
            float effectiveGravity = playerMove.gravity * (1f - gravityLoss);

            currentMoveDirection.y -= effectiveGravity * Time.deltaTime; //弱めた重力を計算に混ぜる
            currentMoveDirection.y += currentUpForce * Time.deltaTime; // さらに上昇力も計算に混ぜる

            playerMove.SetMoveDirectionY(currentMoveDirection.y); // PlayerMoveのmoveDirectionを上書きし続ける
        }
    }

    //ガードメソッド
    public void Gourd(bool btn)
    {
        if (btn && controller.isGrounded)
        {
            playerMove.enabled = false;　//PlayerMove無効
            playerGuard.SetActive(true);　//ガードオブジェクトを出す
            //コライダーを全て無効にする
            SphereCollider[] cols = GetComponents<SphereCollider>();
            foreach (SphereCollider sc in cols)
            {
                sc.enabled = false;
            }
        }
        else
        {
            playerMove.enabled = true;//PlayerMove有効
            playerGuard.SetActive(false);//ガードオブジェクトを隠す
            //コライダーを全て復活
            SphereCollider[] cols = GetComponents<SphereCollider>();
            foreach (SphereCollider sc in cols)
            {
                sc.enabled = true;
            }
        }
    }
}