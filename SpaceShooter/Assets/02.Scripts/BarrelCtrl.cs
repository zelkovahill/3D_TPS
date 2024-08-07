using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelCtrl : MonoBehaviour
{
    // 폭발 효과 파티클을 연결할 변수
    public GameObject expEffect;

    // 결괏값을 저장할 정적 배열을 미리 선언
    private Collider[] colls = new Collider[10];

    // 무작위로 적용할 텍스처 배열
    public Texture[] textures;

    // 하위에 있는 Mesh Renderer 컴포넌트를 저장할 변수
    private new MeshRenderer renderer;

    // 컴포넌트를 저장할 변수
    private Transform tr;
    private Rigidbody rb;

    // 총알 맞은 횟수를 누적시킬 변수
    private int hitCount = 0;

    // 폭발 반경
    public float radius = 10.0f;

    // ===============================

    private void Start()
    {
        if (expEffect == null)
        {
            // 폭발 효과 프리팹 로드
            expEffect = Resources.Load<GameObject>("SmallExplosionEffect");
        }

        tr = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();

        // 하위에 있는 Mesh Renderer 컴포넌트를 추출    
        renderer = GetComponentInChildren<MeshRenderer>();

        // 난수 발생
        int randomIndex = Random.Range(0, textures.Length);

        // 텍스처 지정
        renderer.material.mainTexture = textures[randomIndex];
    }

    private void OnCollisionEnter(Collision coll)
    {
        if (coll.collider.CompareTag("Bullet"))
        {
            // 총알 맞은 횟수를 증가시키고 3회 이상 맞으면 폭발 처리
            if (++hitCount == 3)
            {
                ExpBarrel();
            }
        }
    }

    private void ExpBarrel()
    {
        // 폭발 효과 파티클 생성
        GameObject exp = Instantiate(expEffect, tr.position, Quaternion.identity);

        // 폭발 효과 파티클 5초후에 제거
        Destroy(exp, 5.0f);

        // Rigidbody 컴포넌트의 mass를 1.0으로 수정해 무게를 가볍게 함
        // rb.mass = 1.0f;
        // 위로 솟구치는 힘을 가함
        // rb.AddForce(Vector3.up * 1500.0f);

        // 간접 폭발력 전달
        IndirectDamage(tr.position);

        // 3초 후에 드럼통 제거
        Destroy(gameObject, 3.0f);
    }

    private void IndirectDamage(Vector3 pos)
    {
        // 주변에 있는 드럼통을 모두 추출
        // Collider[] colls = Physics.OverlapSphere(pos, radius, 1 << 6);  // 1 << 6 : Barrel 레이어
        Physics.OverlapSphereNonAlloc(pos, radius, colls, 1 << 6);

        // 1<<8 | 1<<9 : 8번째 비트와 9번째 비트를 OR 연산하여 8번째와 9번째 레이어를 검출
        // ~(1<<8) : 8번째 비트를 NOT 연산하여 8번째 레이어를 제외한 모든 레이어를 검출


        // Physics.OverlapSphere 함수는 실행 시 Sphere 범위에 검출될 개수가 명확하지 않을 때만 사용해야 한다.
        // 메모리 Garbage가 발생하기 때문

        // Sphere 범위에 검출될 개수가 명확할 때는 Garbage가 발생하지 않는 Physics.SphereCastNonAlloc 함수를 권장한다.
        // 결괏값을 저장할 정적 배열을 미리 선언해 사용하며 실행 중에 배열의 크기를 변경할 수 없다.

        foreach (var coll in colls)
        {
            // null 확인 추가
            if (coll != null)
            {
                // 폭발 범위에 포함된 드럼통의 Rigidbody 컴포넌트 추출
                rb = coll.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    // 드럼통의 무게를 가볍게 함
                    rb.mass = 1.0f;

                    // freezeRotation 제한값을 해제
                    rb.constraints = RigidbodyConstraints.None;

                    // 폭발력을 전달
                    rb.AddExplosionForce(1500.0f, pos, radius, 1200.0f);
                    // Destroy(coll.gameObject, 3.0f);
                }
            }
        }
    }
}