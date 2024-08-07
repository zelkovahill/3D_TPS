using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 반드시 필요한 컴포넌트를 명시해 해당 컴포넌트가 삭제되는 것을 방지하는 어트리뷰트
[RequireComponent(typeof(AudioSource))]
public class FireCtrl : MonoBehaviour
{
    // 총알 프리팹
    public GameObject bullet;

    // 총알 발사 좌표
    public Transform firePos;

    // 총소리에 사용할 오디오 음원
    public AudioClip fireSfx;

    // AudioSource 컴포넌트를 저장할 변수
    private new AudioSource audio;

    // Muzzle Flash의 MeshRenderer 컴포넌트
    private MeshRenderer muzzleFlash;

    // Raycast 결괏값을 저장하기 위한 구조체 선언
    private RaycastHit hit;

    // 레이캐스트는 특정 레이어만을 감지하게 할 수 있다.
    // 특정레이어만 검출하는 것이 물리 엔진의 부하를 덜어준다.


    // ===============================

    private void Start()
    {
        audio = GetComponent<AudioSource>();

        // firePos 하위에 있는 MuzzleFlash의 MeshRenderer 컴포넌트 추출
        muzzleFlash = firePos.GetComponentInChildren<MeshRenderer>();

        // 처음 시작할 때 비활성화
        muzzleFlash.enabled = false;

        if (bullet == null)
        {
            bullet = Resources.Load<GameObject>("Bullet");
        }

        if (firePos == null)
        {
            firePos = GameObject.Find("FirePos").transform;
        }
    }

    private void Update()
    {
        // Ray를 시각적으로 표시하기 위해 사용
        Debug.DrawRay(firePos.position, firePos.forward * 10.0f, Color.green);

        // 마우스 왼쪽 버튼을 클릭했을 때 Fire 함수 호출
        if (Input.GetMouseButtonDown(0))
        {
            Fire();

            // Ray를 발사
            if (Physics.Raycast(firePos.position, firePos.forward, out hit, 10.0f, 1 << 7)) // 광선의 발사 원점 / 광선의 발사 방향 / 광선에 맞은 결과 데이터 / 광선의 거리 / 감지하는 범위인 레이어 마스크
            {
                Debug.Log($"Hit={hit.transform.name}");

                hit.transform.GetComponent<MonsterCtrl>()?.OnDamage(hit.point, hit.normal);
            }

            // Physics.Raycast(발사원점, 발사방향, out 결괏값, 광선거리, 검출할_레이어)

            // RaycastHit 구조체의 주요 속성
            // collider : 맞은 게임오브젝트의 Collider 반환
            // transform : 맞은 게임오브젝트의 Transform 반환
            // point : 맞은 위치의 월드 좌푯값 반환(Vector3)
            // distance : 발사 위치와 맞은 위치 사이의 거리
            // normal : Ray가 맞은 표면의 법선 벡터 

            // 근접 센서, NPC의 시야각, 공중에 떠있는 상태 체크 등에 사용
        }
    }

    private void Fire()
    {
        // 총알 프리팹을 동적으로 생성 (생성할 객체, 위치, 회전)
        Instantiate(bullet, firePos.position, firePos.rotation);

        // 총소리 발생
        audio.PlayOneShot(fireSfx, 1.0f);

        // 총구 화염 효과 코루틴 호출
        StartCoroutine(ShowMuzzleFlash());
        // StartCoroutine("ShowMuzzleFlash");
        // 코루틴 함수명을 문자열로 전달할 수도 있지만,
        // 그럴 경우 가비지 컬렉션이 발생하며 문자열롤 호출한 코루틴은 개별적으로 정지시킬 수 없다는 단점이 있다.
        // 그래서 함수명을 직접 전달하는 것을 권장한다.
    }

    private IEnumerator ShowMuzzleFlash()
    {
        // 오프셋 좌푯값을 랜덤 함수로 생성
        Vector2 offset = new Vector2(Random.Range(0, 2), Random.Range(0, 2)) * 0.5f;
        // 텍스처의 오프셋 값 설정
        muzzleFlash.material.mainTextureOffset = offset;

        // MuzzleFlash의 회전 변경
        float angle = Random.Range(0, 360);
        muzzleFlash.transform.localRotation = Quaternion.Euler(0, 0, angle);

        // MuzzleFlash의 크기 조절
        float scale = Random.Range(1.0f, 2.0f);
        muzzleFlash.transform.localScale = Vector3.one * scale;

        // MuzzleFlash 활성화
        muzzleFlash.enabled = true;

        // 0.2초 동안 대기(정지)하는 동안 메시지 루프로 제어권을 양보
        yield return new WaitForSeconds(0.2f);

        // MuzzleFlash 비활성화
        muzzleFlash.enabled = false;
    }
}
