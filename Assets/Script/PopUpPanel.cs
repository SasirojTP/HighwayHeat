using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PopUpPanel : MonoBehaviour
{
    public void OnClickLoadSceneButton()
    {
        SceneManager.LoadScene(0);
    }
    
}
