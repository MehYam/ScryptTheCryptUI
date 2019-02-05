using UnityEngine;

 public class AnimationAutoDestroy : MonoBehaviour
{
    [SerializeField] private float additionalSeconds = 0f;
    [SerializeField] private GameObject destroyTarget = null;
    void Start()
    { 
        var target = destroyTarget ?? gameObject;
        Destroy(target, GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length + additionalSeconds - 0.1f); 
    }
 }