using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCtrl : MonoBehaviour
{
    // 접근해야 하는 컴포넌트는 반드시 변수에 할당한 후 사용
    // [SerializeField] // 인스펙터 뷰에 노출


    private Transform tr;                       // 컴포넌트를 캐시 처리할 변수
    private Animation anim;                   // Animation 컴포넌트를 저장할 변수

    public float moveSpeed = 10.0f;   // 이동 속력 변수

    public float turnSpeed = 80.0f;     // 회전 속도 변수

    // 초기 생명 값
    private readonly float initHp = 100.0f;

    // 현재 생명 값
    public float currHp;

    // Hpbar 연결할 변수
    private Image hpBar;

    // 델리게이트 선언
    public delegate void PlayerDieHandler();

    // 이벤트 선언
    public static event PlayerDieHandler OnPlayerDie;


    // ================================================================

    // void 대신 IEnumerator로 코루틴 함수로 변경
    private IEnumerator Start()
    {
        // Hpbar 연결
        hpBar = GameObject.FindGameObjectWithTag("Hp_Bar")?.GetComponent<Image>();
        // ? 연산자는 null 체크를 할 때 코드를 간결하게 해주는 역할을 한다.
        // null이 아니라면 GetComponent<Image>()를 실행하고 null이라면 null을 반환한다.

        // var hpBar = GameObject.FindGameObjectWithTag("Hp_Bar").GetComponent<Image>();
        // if(hpBar != null)
        // {
        //     this.hpBar = hpBar;
        // }


        // HP 초기화
        currHp = initHp;
        DisplayHealth();

        tr = GetComponent<Transform>();                 // Transform 컴포넌트를 추출해 변수에 대입
        anim = GetComponent<Animation>();            // Animation 컴포넌트를 추출해 변수에 대입

        // tr = this.gameObject.GetComponent<Transform>();
        // tr = GetComponent("Transform") as Transform;
        // tr = (Transform)GetComponent(typeof(Transform));


        // 애니메이션 실행
        anim.Play("Idle");

        // 처음 시작할 때 쓰레깃값(노이즈)이 넘어온다면 코루틴을 활용해 허용 임계치를 적용할 수 있다.
        turnSpeed = 0.0f;
        yield return new WaitForSeconds(0.3f);
        turnSpeed = 80.0f;
    }


    private void Update()
    {
        // GetAxis -> -1.0f ~ 1.0f 사이의 연속적인 값
        // GetAxisRaw -> -1.0f, 0.0f, 1.0f 세가지 값만 리턴
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        float r = Input.GetAxisRaw("Mouse X");

        float deadzone = 0.1f;

        if (Mathf.Abs(r) < deadzone)
        {
            r = 0;
        }

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

    private void OnTriggerEnter(Collider coll)
    {
        // 충돌한 Collider가 몬스터의 Punch이면 Player의 HP 차감
        if (currHp >= 0.0f && coll.CompareTag("Punch"))
        {
            currHp -= 10.0f;
            DisplayHealth();
            print($"Player HP = {currHp / initHp}");
            // print("Player HP = " + (currHp / initHp).ToString());

            // Player의 생명이 0 이하이면 사망 처리
            if (currHp <= 0.0f)
            {
                PlayerDie();
            }
        }
    }

    private void PlayerDie()
    {
        print("Player Die!!!");

        // // Monster 태그를 가진 모든 게임오브젝트를 찾아옴
        // GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");

        // // 모든 몬스터의 OnPlayerDie 함수를 순차적으로 호출
        // foreach (GameObject monster in monsters)
        // {
        //     monster.SendMessage("OnPlayerDie", SendMessageOptions.DontRequireReceiver);
        // }
        // SendMessage 함수는 첫 번째 인자로 전달한 함수명과 동일한 함수가 해당 게임오브젝트의 스크립트에 있다면 실행하라는 명령이다.
        // SendMessageOptions.DontRequireReceiver는 해당 함수가 없어도 에러를 발생시키지 않는다.

        // 주인공 사망 이벤트 호출(발생)
        OnPlayerDie();

        // GameManager 스크립트의 IsGameOver 프로퍼티 값을 변경
        //GameObject.Find("GameManager").GetComponent<GameManager>().IsGameOver = true;

        GameManager.instance.IsGameOver = true;
    }

    private void DisplayHealth()
    {
        hpBar.fillAmount = currHp / initHp;
    }
}
