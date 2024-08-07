using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelegateDemo : MonoBehaviour
{
    // 델리게이트 선언
    private delegate float SumHandler(float a, float b);

    // 델리게이트 타입의 변수 선언
    private SumHandler sumHandler;

    private void Start()
    {
        // 델리게이트 변수에 함수(메서드) 연결(할당)
        sumHandler = Sum;

        // 델리게이트 실행
        float sum = sumHandler(3.0f, 4.0f);

        // 결과값 출력
        Debug.Log($"델리게이트 실행 결과 : {sum}");

        // 델리게이트 변수에 람다식 연결
        sumHandler = (a, b) => a + b;
        float sum2 = sumHandler(10.0f, 5.0f);
        Debug.Log($"람다식 델리게이트 실행 결과 : {sum2}");

        // 델리게이트 변수에 무명 메서드 연결
        sumHandler = delegate (float a, float b) { return a + b; };
        float sum3 = sumHandler(20.0f, 30.0f);
        Debug.Log($"무명 메서드 델리게이트 실행 결과 : {sum3}");
    }

    // 덧셈 연산을 하는 함수
    private float Sum(float a, float b)
    {
        return a + b;
    }


}
