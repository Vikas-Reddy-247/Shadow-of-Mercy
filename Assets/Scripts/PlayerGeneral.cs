using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGeneral : MonoBehaviour
{
    private Animator anim;
    public Transform guardLock;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void StealthTakedown()
    {
        //When takedown mechanic is called the animation runs
        anim.SetTrigger("strangle");
    }
}
