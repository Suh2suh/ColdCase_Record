using UnityEngine;



public class DeActivateOnStart : MonoBehaviour
{
    
    void Start()
    {
        ActivationController.ActivateObj(this.gameObject, false);
    }


}
