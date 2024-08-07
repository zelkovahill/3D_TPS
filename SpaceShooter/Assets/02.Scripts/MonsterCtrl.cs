using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterCtrl : MonoBehaviour
{
    // 몬스터의 상태 정보
    public enum State
    {
        IDLE,
        TRACE,
        ATTACK,
        DIE
    }

    // 몬스터의 현재 상태
    public State state = State.IDLE;

    // 추적 사정거리
    public float traceDist = 10.0f;

    // 공격 사정거리
    public float attackDist = 2.0f;

    // 몬스터의 사망 여부
    public bool isDie = false;

    // 컴포넌트의 캐시를 처리할 변수
    private Transform monsterTr;
    private Transform playerTr;
    private NavMeshAgent agent;
    private Animator anim;

    // Animator 파라미터의 해시값 추출
    private readonly int hashTrace = Animator.StringToHash("IsTrace");
    private readonly int hashAttack = Animator.StringToHash("IsAttack");
    private readonly int hashHit = Animator.StringToHash("Hit");
    private readonly int hashPlayerDie = Animator.StringToHash("PlayerDie");
    private readonly int hashSpeed = Animator.StringToHash("Speed");
    private readonly int hashDie = Animator.StringToHash("Die");
    // 파라미터의 이름을 문자열로 전달할 경우 오탈자가 있다면 오류가 발생
    // 또한, 애니메이터 컨트롤에 정의한 파라미터는 모두 해시 테이블로 관리하기 때문에 문자열로 호출할 때마다 내부의 해시테이블을 검색하므로 속도 면에서 불리하다.
    // 따라서, 미리 해시값을 추출하여 변수에 저장해두고 사용하는 것이 좋다.

    // 혈흔 효과 프리팹
    private GameObject bloodEffect;

    // 몬스터의 생명 변수
    private int hp = 100;


    // ============================================================

    private void Awake()
    {
        anim = GetComponent<Animator>();

        // 몬스터의 Transform 할당
        monsterTr = GetComponent<Transform>();

        // 추적 대상인 Player의 Transform 할당
        playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();  // 태그가 지정되지 않은 경우 null을 반환
        // Find 계열의 함수는 처리 속도가 느리기 때문에 Update함수에서 사용하지 않는다.
        // 따라서 Awake나 Start 함수에서 처리한다.

        // NavMeshAgent 컴포넌트 할당
        agent = GetComponent<NavMeshAgent>();

        // NavMeshAgent의 자동 회전 기능 비활성화
        agent.updateRotation = false;

        // 추적 대상의 위치를 설정하면 바로 추적 시작
        // agent.destination = playerTr.position;
        // agent.SetDestination(playerTr.position); // 위와 동일한 기능

        // bloodEffect 프리팹 로드
        bloodEffect = Resources.Load<GameObject>("BloodSprayEffect");
        // Resources.Load<T>() 함수는 Resources 폴더에 있는 프리팹을 로드할 때 사용한다.
    }

    private void Update()
    {
        // 목적지까지 남은 거리로 회전 여부 판단
        if (agent.remainingDistance >= 2.0f)
        {
            // 에이전트의 이동 방향
            Vector3 direction = agent.desiredVelocity;

            if (direction != Vector3.zero)
            {
                // 회전 각도(쿼터니언) 산출
                Quaternion rot = Quaternion.LookRotation(direction);

                // 구면 선형보간 함수로 부드러운 회전 처리
                monsterTr.rotation = Quaternion.Slerp(monsterTr.rotation, rot, Time.deltaTime * 10.0f);

                // MavMeshAgent의 주요 속성
                // updatePosition / 위치를 자동으로 이동시키는 옵션
                // updateRotation / 자동으로 회전시키는 옵션
                // remainingDistance / 목적지까지 남은 거리
                // velocity / 에이전트의 현재 속도
                // desiredVelocity / 장애물 회피를 고려한 이동 방향
                // pathPending / 목적지까지의 최단거리 계산이 완료됐는지 여부
                // isPathStale / 계산한 경로의 유효성 여부 (동적 장애물, OffMeshLink)
            }
        }
    }

    // 스크립트가 활성화될 때마다 호출되는 함수
    private void OnEnable()
    {
        // 이벤트 발생 시 수행할 함수 연결
        PlayerCtrl.OnPlayerDie += this.OnPlayerDie;

        // 몬스터의 상태를 체크하는 코루틴 함수 호출
        StartCoroutine(CheckMonsterState());
        // Update 함수에서 처리할 수도 있지만 매 프레임 시랳ㅇ하는 것은 성능상 오버헤드를 줄 수 있으므로
        // 0.2초 또는 0.3초 간격으로 적 캐릭터의 상태를 조사해도 이상 없는 로직이라면 코루틴을 사용하는 것이 좋다.

        // 상태에 따라 몬스터의 행동을 수행하는 코루틴 함수 호출
        StartCoroutine(MonsterAction());
    }

    private IEnumerator CheckMonsterState()
    {
        while (!isDie)
        {
            // 0.3초 동안 중지(대기)하는 동안 제어권을 메시지 루프에 양보
            yield return new WaitForSeconds(0.3f);
            // 일종의 Sleep 기능

            // 몬스터의 상태가 Die일 때 코루틴을 종료
            if (state == State.DIE)
            {
                yield break;
            }

            // 몬스터와 주인공 캐릭터 사이의 거리 측정
            float distance = Vector3.Distance(playerTr.position, monsterTr.position);

            // 공격 사정거리 범위로 들어왔는지 확인
            if (distance <= attackDist)
            {
                state = State.ATTACK;
            }
            // 추적 사정거리 범위로 들어왔는지 확인
            else if (distance <= traceDist)
            {
                state = State.TRACE;
            }
            else
            {
                state = State.IDLE;
            }
        }
    }

    private IEnumerator MonsterAction()
    {
        while (!isDie)
        {
            switch (state)
            {
                // IDLE 상태
                case State.IDLE:
                    // 추적 중지
                    agent.isStopped = true; //  이동을 멈춤
                    anim.SetBool(hashTrace, false); // Animator의 IsTrace 파라미터를 false로 설정
                    break;

                // 추적 상태
                case State.TRACE:
                    // 추적 대상의 위치를 넘겨줌
                    agent.SetDestination(playerTr.position);    // 목적지 설정
                    agent.isStopped = false;   // 이동을 재개
                    anim.SetBool(hashTrace, true);  // Animator의 IsTrace 파라미터를 true
                    anim.SetBool(hashAttack, false); // Animator의 IsAttack 파라미터를 false
                    break;

                // 공격 상태
                case State.ATTACK:
                    anim.SetBool(hashAttack, true);  // Animator의 IsAttack 파라미터를 true
                    break;

                // 사망 상태
                case State.DIE:
                    isDie = true;
                    agent.isStopped = true;     // 추적 정지
                    anim.SetTrigger(hashDie);   // 사망 애니메이션 실행
                    GetComponent<CapsuleCollider>().enabled = false; // 몬스터의 Collider 컴포넌트 비활성화

                    // 일정 시간 대기 후 오브젝트 풀링으로 환원
                    yield return new WaitForSeconds(3.0f);

                    // 사망 후 다시 사용할 때를 위해 hp 값 초기화
                    hp = 100;
                    isDie = false;
                    
                    state = State.IDLE;

                    // 몬스터의 Collider 컴포넌트 활성화
                    GetComponent<CapsuleCollider>().enabled = true;

                    // 몬스터를 비활성화
                    this.gameObject.SetActive(false);
                    break;
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    private void OnCollisionEnter(Collision coll)
    {
        if (coll.collider.CompareTag("Bullet"))
        {
            // 충돌한 총알을 삭제
            Destroy(coll.gameObject);

            // // 피격 리액션 애니메이션 실행
            // anim.SetTrigger(hashHit);

            // // 총알의 충돌 지점
            // Vector3 Pos = coll.GetContact(0).point;
            // // 충돌 지점의 좌푯값은 배열로 반환되므로 첫 번째 요소를 추출

            // // 총알의 충돌 지점의 법선 벡터
            // Quaternion rot = Quaternion.LookRotation(-coll.GetContact(0).normal);
            // // 혈흔 효과는 Z축으로 피가 튀는 파티클로 회전 각도를 총알 법선 벡터의 반대 방향으로 설정해야 한다.

            // // 혈흔 효과를 생성하는 함수 호출
            // ShowBloodEffect(Pos, rot);

            // // 몬스터의 hp 차감
            // hp -= 10;
            // if (hp <= 0)
            // {
            //     state = State.DIE;

            //     // 몬스터가 사망했을 때 50점을 추가
            //     GameManager.instance.DisplayScore(50);
            // }

        }
    }

    private void ShowBloodEffect(Vector3 pos, Quaternion rot)
    {
        // 혈흔 효과 생성
        GameObject blood = Instantiate(bloodEffect, pos, rot);
        Destroy(blood, 1.0f);
    }

    private void OnDrawGizmos()
    {
        // 추적 사정거리 표시
        if (state == State.TRACE)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(monsterTr.position, traceDist);   // (중심점, 반지름)
        }
        // 공격 사정거리 표시
        else if (state == State.ATTACK)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(monsterTr.position, attackDist);  // (중심점, 반지름)
        }
    }

    private void OnPlayerDie()
    {
        // 몬스터의 상태를 체크하는 코루틴 함수를 모두 정지 시킴
        StopAllCoroutines();

        // 추적을 정지하고 애니메이션을 수행
        agent.isStopped = true;
        anim.SetFloat(hashSpeed, Random.Range(0.8f, 1.2f));
        anim.SetTrigger(hashPlayerDie);
    }


    // 스크립트가 비활성화될 때마다 호출되는 함수
    private void OnDisable()
    {
        // 기존에 연결된 함수 해제
        PlayerCtrl.OnPlayerDie -= this.OnPlayerDie;
    }

    // 레이캐스트를 사용해 데미지를 입히는 로직
    public void OnDamage(Vector3 pos, Vector3 normal)
    {
        // 피격 리액션 애니메이션 실행
        anim.SetTrigger(hashHit);

        Quaternion rot = Quaternion.LookRotation(normal);

        // 혈흔 효과를 생성하는 함수 호출
        ShowBloodEffect(pos, rot);

        // 몬스터의 hp 차감
        hp -= 30;
        if (hp <= 0)
        {
            state = State.DIE;

            // 몬스터가 사망했을 때 50점을 추가
            GameManager.instance.DisplayScore(50);
        }

    }

}
