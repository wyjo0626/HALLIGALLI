using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainScene_Btn : MonoBehaviour
{
    public void OnClickLoadScene(string scene) {
        SceneManager.LoadScene(scene);
    }
}
