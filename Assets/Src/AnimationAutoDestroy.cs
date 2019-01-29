using UnityEngine;

 public class AnimationAutoDestroy : MonoBehaviour
{
    [SerializeField] private float additionalSeconds = 0f;
    void Start()
    { 
        Destroy(gameObject, GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length + additionalSeconds - 0.1f); 
    }
 }