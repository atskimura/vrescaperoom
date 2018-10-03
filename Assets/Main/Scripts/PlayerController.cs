using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    // タイトル表示
    [SerializeField]
    private GameObject title;

    // クリア表示
    [SerializeField]
    private GameObject clear;

    // ゲームスタートしているかのフラグ
    private bool isStart = false;

    // GetComponentがコストが大きいのでStart時に取得しておく
    private AudioSource clearAudio;
    private Rigidbody rb;

    // 移動時の速度
    private float speed = 2.0F;

    // PCでの移動時の回転速度
    private float rotateSpeed = 2.0F;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        clearAudio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isStart)
        {
            // トリガーでスタート
            if (Input.GetKey(KeyCode.Space) || OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger))
            {
                GameStart();
            }
        }
        else
        {
            // 戻るボタンが押されたらリセット
            if (Input.GetKeyDown(KeyCode.Return) || OVRInput.Get(OVRInput.Button.Back))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

    void FixedUpdate()
    {
        // RigidbodyのAddForce以外での移動はFixedUpdateで呼ぶ
        Move();
    }

    /**
     * 他のオブジェクトに衝突したときの処理
     */
    private void OnTriggerEnter(Collider other)
    {
        // Goalにぶつかったら
        if (other.gameObject.name == "Goal")
        {
            GameClear(other.gameObject);
        }
    }

    private void GameStart()
    {
        // タイトル画面を非表示
        title.SetActive(false);

        // レーザーポインターを有効化
        GetComponent<LaserPointerController>().enabled = true;

        isStart = true;
    }

    private void GameClear(GameObject goal)
    {
        // 眼前にクリア表示
        Vector3 forward = getCameraFoward();
        clear.transform.position = transform.position + forward * 2.0f;
        // クリア表示を見ている方向に回転させる
        clear.transform.rotation = Quaternion.LookRotation(forward);
        clear.SetActive(true);
        // クリア時の効果音を再生
        clearAudio.Play();
        // ゴールを残しておくと通るたびに何度も上記が動いてしまうので削除する
        Destroy(goal);
    }

    private void Move()
    {
        // スタートしてなかったら動けない
        if (!isStart)
            return;

        // Oculus Goでの移動
        if (OVRManager.isHmdPresent)
        {
            moveOculusGo();
        }
        // 開発用にPCでの移動
        else
        {
            movePc();
        }
    }

    /**
     * カメラの向きをXY平面に射影した方向を取得
     */
    private Vector3 getCameraFoward()
    {
        // カメラの向き
        Vector3 cameraDir = Camera.main.transform.forward;
        // 上下移動しないようXY平面に射影
        return Vector3.ProjectOnPlane(cameraDir, Vector3.up);
    }

    /**
     * Oculus Goでの移動
     */
    private void moveOculusGo()
    {
        if (OVRInput.Get(OVRInput.Button.One))
        {
            // タッチパッドのタッチ位置
            Vector2 touchPadPt = OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad);
            //Debug.Log("touchPadPt: " + touchPadPt.ToString());
            // タッチパッドの横の方を押したときは移動しない
            if (Mathf.Abs(touchPadPt.y) > 0.2)
            {
                // カメラの向きに進む
                Vector3 direction = getCameraFoward();
                if (touchPadPt.y > 0)
                {
                    // 前進
                    rb.MovePosition(transform.position + direction * speed * Time.fixedDeltaTime);
                }
                else
                {
                    // 後進
                    rb.MovePosition(transform.position - direction * speed * Time.fixedDeltaTime);
                }
            }
        }
    }

    /**
     * PCでの移動
     */
    private void movePc()
    {
        // 左右矢印キーが押されたら向きを回転
        if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0)
        {
            transform.Rotate(0, Input.GetAxis("Horizontal") * rotateSpeed, 0);
        }
        // 上下矢印キーが押されたら前後進
        if (Mathf.Abs(Input.GetAxis("Vertical")) > 0)
        {
            if (Input.GetAxis("Vertical") > 0)
                rb.MovePosition(transform.position + transform.forward * speed * Time.fixedDeltaTime);
            else
                rb.MovePosition(transform.position - transform.forward * speed * Time.fixedDeltaTime);
        }
    }
}
