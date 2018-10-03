using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserPointerController : MonoBehaviour
{
    [SerializeField]
    private Transform rightHandAnchor;

    [SerializeField]
    private Transform leftHandAnchor;

    [SerializeField]
    private Transform centerEyeAnchor;

    [SerializeField]
    private LineRenderer laserPointerRenderer;

    // レーザーの最大距離
    private float maxDistance = 2.5f;

    // レーザーの発射口
    private Transform pointer;
    // キャッチしたオブジェクトの1フレーム前の位置
    private Vector3 lastCatchObjPosition;
    // レーザーが当たったオブジェクト
    private Rigidbody hitRb;
    // キャッチしたオブジェクト
    private Rigidbody catchRb;

    private Transform Pointer
    {
        get
        {
            // 現在アクティブなコントローラーを取得
            var controller = OVRInput.GetActiveController();
            if (controller == OVRInput.Controller.RTrackedRemote)
            {
                return rightHandAnchor;
            }
            else if (controller == OVRInput.Controller.LTrackedRemote)
            {
                return leftHandAnchor;
            }
            // どちらも取れなければ目の間からビームが出る
            return centerEyeAnchor;
        }
    }

    void Start()
    {
    }

    void Update()
    {
        UpdateLaser();

        if (catchRb)
        {
            // キャッチしたオブジェクトをトリガー離したら投げる
            if (Input.GetKeyUp(KeyCode.Space) || OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger))
            {
                ThrowObject();
            }
        }

        if (hitRb)
        {
            // レーザーが当たっているオブジェクトをトリガー押したら取る
            if (Input.GetKeyDown(KeyCode.Space) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
            {
                CatchObject();
            }
        }

        if (catchRb)
        {
            // 投げたときの速度計算にキャッチしたオブジェクトの位置を保存しておく
            lastCatchObjPosition = catchRb.transform.position;
        }
    }

    private void UpdateLaser()
    {
        pointer = Pointer;

        // Rayを作成
        Ray pointerRay = GenerateRay();

        // レーザーの起点
        laserPointerRenderer.SetPosition(0, pointerRay.origin);

        RaycastHit hitInfo;
        if (Physics.Raycast(pointerRay, out hitInfo, maxDistance))
        {
            // Rayがヒットしたらそこまで
            laserPointerRenderer.SetPosition(1, hitInfo.point);
            hitRb = hitInfo.rigidbody;
        }
        else
        {
            // Rayがヒットしなかったら向いている方向にMaxDistance伸ばす
            laserPointerRenderer.SetPosition(1, pointerRay.origin + pointerRay.direction * maxDistance);
            hitRb = null;
        }
    }

    /**
     * Oculus GoとPCでRayの場所と方向を変えているので、それに従ってRayを生成
     */
    private Ray GenerateRay()
    {
        if (OVRManager.isHmdPresent)
        {
            // コントローラー位置からRayを飛ばす
            return new Ray(pointer.position, pointer.forward);
        }
        else
        {
            // PCではカメラのちょい下から斜め下に飛ばす
            return new Ray(pointer.position + new Vector3(0, -1.4f, 0), pointer.forward + new Vector3(0, -0.15f, 0));
        }
    }

    /**
     * オブジェクトをキャッチする
     */
    private void CatchObject()
    {
        FixedJoint pointerJoint = pointer.gameObject.GetComponent<FixedJoint>();
        if (pointerJoint)
        {
            // まだキャッチしたままのオブジェクトがあったら離しておく
            pointerJoint.connectedBody = null;
            Destroy(pointerJoint);
        }
        catchRb = hitRb;
        // FixedJointを使ってPointerとレーザーの当たったオブジェクトをくっつける
        pointerJoint = pointer.gameObject.AddComponent<FixedJoint>();
        pointerJoint.breakForce = 20000;
        pointerJoint.breakTorque = 20000;
        pointerJoint.connectedBody = catchRb;
    }

    /**
     * オブジェクトを離す
     */
    private void ThrowObject()
    {
        FixedJoint pointerJoint = pointer.gameObject.GetComponent<FixedJoint>();
        if (pointerJoint)
        {
            // FixedJointを削除し、オブジェクトを切り離す
            pointerJoint.connectedBody = null;
            Destroy(pointerJoint);
            // OVRInput.GetLocalControllerVelocityでやりたかったがzeroを返すので、位置情報から速度ベクトルを計算
            Vector3 catchRbVelocity = (catchRb.transform.position - lastCatchObjPosition) / Time.deltaTime;
            // 家具の重量を加味するようAddForceで力を加える
            catchRb.AddForce(catchRbVelocity, ForceMode.Impulse);
        }
        catchRb = null;
    }
}