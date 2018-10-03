using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    // 鍵オブジェクト
    [SerializeField]
    private GameObject key;

    // ドアが開いた効果音
    private AudioSource openSound;

    // Use this for initialization
    void Start()
    {
        openSound = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        // 当たったオブジェクトが鍵と一致したらドアを開ける
        if (collision.gameObject == key)
        {
            // 効果音を鳴らす
            openSound.Play();
            // ドアを開くアニメーションを実行
            Animator animator = transform.parent.GetComponent<Animator>();
            animator.SetTrigger("open");
            // 鍵は削除する
            Destroy(collision.gameObject);
        }
    }
}
