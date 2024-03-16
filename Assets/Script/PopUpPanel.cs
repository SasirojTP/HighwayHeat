using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class PopUpPanel : MonoBehaviour
{
    [SerializeField] Button okButton;
    void Start()
    {
        okButton.onClick.AddListener(OnClickOkButton);
    }

    void OnClickOkButton()
    {
        SceneManager.LoadScene(0);
    }
    
}
