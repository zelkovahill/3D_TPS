using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    // 버튼을 연결할 변수
    public Button startButton;
    public Button optionButton;
    public Button shopButton;

    private UnityAction action;

    private void Start()
    {
        // 특정 이벤트가 발생하면 호출할 함수를 연결하기 위해 AddListener(UnityyAction call) 함수를 사용한다.
        // Button에는 onClick이라는 버튼 클릭 이벤트가 정의되어 있다.

        // UnityAction을 사용한 이벤트 연결 방식

        // action = () => OnButtonClicked(startButton.name);
        action = () => OnStartClick();
        startButton.onClick.AddListener(action);

        // 무명 메서드를 활용한 이벤트 연결 방식
        optionButton.onClick.AddListener(delegate { OnButtonClicked(optionButton.name); });

        // 람다식을 활용한 이벤트 연결 방식
        shopButton.onClick.AddListener(() => OnButtonClicked(shopButton.name));
        // action 변수에 함수를 연결하는 방식은 람다식을 사용

        // 델리게이트_타입 변수명 =(매개변수,매개변수2, ...) => 식;
        // 델리게이트_타입 변수명 = (매개변수,매개변수2, ...) => { 로직_1;  로직_2;  ... };

        // 델리게이트_타입 변수명 = () => 식;
        //델리게이트_타입 변수명 = () => { 로직_1;  로직_2;  ... };

        // 취향에 따라 이벤트 연결 방식을 선택하여 사용

    }

    public void OnButtonClicked(string msg)
    {
        Debug.Log($"Button Clicked! : {msg}");
    }

    public void OnStartClick()
    {
        SceneManager.LoadScene("Level_01");
        SceneManager.LoadScene("Play", LoadSceneMode.Additive);

        // SceneManager 주요 함수
        // CreateScene : 새로운 빈 씬을 생성
        // LoadScene : 씬을 로드
        // LoadSceneAsync : 씬을 비동기 방식으로 로드
        // MergeScenes : 소스 씬을 다른 씬을 통합한다.  소스 씬은 모든 게임오브젝트가 통합된 이후 삭제된다.
        // MoveGameObjectToScene : 현재 씬에 있는 특정 게임오브젝트를 다른 씬으로 이동
        // UnloadScene : 현재 씬에 있는 모든 게임오브젝트를 삭제

        // LoadSceneMode.Single : 기존에 로드된 씬을 모두 삭제한 후 새로운 씬을 로드
        // LoadSceneMode.Additive : 기존 씬을 삭제하지 않고 추가해서 새로운 씬을 로드
    }
}
