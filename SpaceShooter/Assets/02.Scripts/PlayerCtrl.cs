using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShortcutManagement;
using UnityEngine;

public class PlayerCtrl : MonoBehaviour
{
    // 접근해야 하는 컴포넌트는 반드시 변수에 할당한 후 사용
    // [SerializeField] // 인스펙터 뷰에 노출


    private Transform tr;                       // 컴포넌트를 캐시 처리할 변수
    private Animation anim;                   // Animation 컴포넌트를 저장할 변수

    public float moveSpeed = 10.0f;   // 이동 속력 변수



    public float turnSpeed = 80.0f;     // 회전 속도 변수

    private void Start()
    {

        tr = GetComponent<Transform>();                 // Transform 컴포넌트를 추출해 변수에 대입
        anim = GetComponent<Animation>();            // Animation 컴포넌트를 추출해 변수에 대입

        // tr = this.gameObject.GetComponent<Transform>();
        // tr = GetComponent("Transform") as Transform;
        // tr = (Transform)GetComponent(typeof(Transform));


        // 애니메이션 실행
        anim.Play("Idle");
    }


    private void Update()
    {
        // GetAxis -> -1.0f ~ 1.0f 사이의 연속적인 값
        // GetAxisRaw -> -1.0f, 0.0f, 1.0f 세가지 값만 리턴
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        float r = Input.GetAxisRaw("Mouse X");

        // Transform 컴포넌트의 position 속성을 변경
        // transform.position += new Vector3(0, 0, 1);

        // **정규화 벡터를 사용한 코드**
        // tr.position += Vector3.forward * 1;

        // Translate 함수
        // 게임오브젝트의 이동 처리를 편하게 할 수 있는 함수
        // void Translate(Vector3 direction, [space relativeTo])

        // 프레임마다 10유닛씩 이동
        // tr.Translate(Vector3.forward * moveSpeed);

        // 매초 10유닛씩 이동
        // tr.Translate(Vector3.forward * moveSpeed * Time.deltaTime * v);

        // 전후좌우 이동 방향 벡터 계산
        Vector3 moveDir = (Vector3.forward * v) + (Vector3.right * h);

        // tr.Translate(moveDir * moveSpeed * Time.deltaTime);

        // 대각선 벡터의 길이는 피타고라스의 정리에 의해 루트2임을 알 수 있다.
        // 따라서 대각선 이동 속도를 일정하게 하기 위해선 정규화 벡터를 사용해야 한다.
        // 피타고라스 정리 : 빗변^2 = 밑변^2 + 높이^2 , 빗변 = 루트(밑변^2 + 높이^2)

        tr.Translate(moveDir.normalized * moveSpeed * Time.deltaTime);

        // Vector3.up 축을 기준으로 turnSpeed만큼의 속도로 회전
        tr.Rotate(Vector3.up * turnSpeed * Time.deltaTime * r);

        // 주인공 캐릭터의 애니메이션 설정
        PlayerAnim(h, v);

    }

    private void PlayerAnim(float h, float v)
    {
        // 키보드 입력값을 기준으로 동작할 애니메이션 수행

        if (v >= 0.1f)
        {
            anim.CrossFade("RunF", 0.25f);  // 전진 애니메이션
        }
        else if (v <= -0.1f)
        {
            anim.CrossFade("RunB", 0.25f);  // 후진 애니메이션
        }
        else if (h >= 0.1f)
        {
            anim.CrossFade("RunR", 0.25f);  // 오른쪽 이동 애니메이션
        }
        else if (h <= -0.1f)
        {
            anim.CrossFade("RunL", 0.25f);  // 왼쪽 이동 애니메이션
        }
        else
        {
            anim.CrossFade("Idle", 0.25f);  // 정지 시 Idle 애니메이션
        }

        // 애니메이션 키프레임을 보간해 부드럽게 보정

    }
}
