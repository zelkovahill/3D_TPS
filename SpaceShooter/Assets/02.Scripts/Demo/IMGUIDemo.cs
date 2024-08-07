using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class IMGUIDemo : MonoBehaviour
{
    // IMGUI (Immediate Made GUI)
    // 코드를 이용해 UI를 표시하는 방법으로 개발 과정에서 간단한 테스트용으로 사용
    // OnGUI 함수에서 코드를 구현
    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 50), "SpaceShooter");

        if (GUI.Button(new Rect(10, 60, 100, 30), "Start"))
        {
            Debug.Log("Start button clicked!");
        }
    }
}
