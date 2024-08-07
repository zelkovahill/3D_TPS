using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveBullet : MonoBehaviour
{
    // 스파크 파티클 프리팹을 연결할 변수
    public GameObject sparkEffect;

    private void Start()
    {
        if (sparkEffect == null)
        {
            // 스파크 프리팹 로드
            sparkEffect = Resources.Load<GameObject>("SparkEffect");
        }
    }

    private void OnCollisionEnter(Collision coll)
    {
        // tag 와 CompareTag
        // 가비지 컬렉션이 발생하지 않는 CompareTag 함수 사용을 적극적으로 권장

        // 충돌한 게임 오브젝트의 태그값 비교
        if (coll.collider.CompareTag("Bullet"))
        {
            // 첫 번째 충돌 지점의 정보 추출
            ContactPoint contact = coll.GetContact(0);
            // Collision 클래스에 정의된 GetContact, GetContacts 함수를 통해 충돌 정보를 추출

            // Collision.contacts 속성으로도 알 수 있지만 가비지 컬렉션을 발생시키기 때문에 GetContact, GetContacts 함수 사용을 권장

            // 충돌한 총알의 법선 벡터를 쿼터니언 타입을 변환
            Quaternion rot = Quaternion.LookRotation(-contact.normal);

            // 스파크 파티클 동적으로 생성
            GameObject spark = Instantiate(sparkEffect, contact.point, rot);

            // 일정 시간이 지난 후 스파크 파티클 삭제
            Destroy(spark, 0.5f);

            // 충돌한 게임 오브젝트 삭제
            Destroy(coll.gameObject);
        }
    }
}
