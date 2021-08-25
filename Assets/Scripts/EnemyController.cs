using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    Rigidbody[] rbs;
    Animator anim;
    ShooterController shooter;

    public bool aimed;
    void Start()
    {
        shooter = FindObjectOfType<ShooterController>();
        anim = GetComponent<Animator>();
        rbs = GetComponentsInChildren<Rigidbody>();
        Ragdoll(false, transform);//首先取消激活所有节点
    }

    public void Ragdoll(bool state, Transform point)
    {
        anim.enabled = !state;
        foreach (Rigidbody rb in rbs)
        {
            rb.isKinematic = !state;
        }

        if (state == true)
        {
            point.GetComponent<Rigidbody>().AddForce(shooter.transform.forward * 30, ForceMode.Impulse);
        }

    }
}
