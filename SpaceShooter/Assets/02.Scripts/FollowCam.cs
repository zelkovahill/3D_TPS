using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    // 따라가야 할 대상을 연결할 변수
    public Transform target;

    // Main Camera 자신의 Transform 컴포넌트
    private Transform camTr;

    // 따라갈 대상으로부터 떨어질 거리
    [Range(2.0f, 20.0f)]
    public float distance = 10.0f;

    // Y축으로 이동할 높이
    [Range(1.0f, 10.0f)]
    public float height = 2.0f;

    // 반응 속도
    public float damping = 10.0f;

    // 카메라 LookAt의 Offset 값
    public float targetOffset = 2.0f;

    // SmoothDamp에서 사용할 변수
    private Vector3 velocity = Vector3.zero;

    // =============================

    private void Start()
    {
        if (!target)
        {
            target = GameObject.Find("Player").transform;
        }
        // Main Camera 자신의 Transform 컴포넌트를 추출
        camTr = GetComponent<Transform>();
    }

    private void LateUpdate()
    {
        // 추적해야 할 대상의 뒤쪽으로 distance 만큼 이동
        // 높이를 height 만큼 이동
        Vector3 pos = target.position + (-target.forward * distance) + (Vector3.up * height);
        // 카메라의 위치 = 타깃의 위치 + (타깃의 뒤쪽 방향 * 떨어질 거리) + (y축 방향 * 높이)

        // 구면 선형 보간 함수를 사용해 부드럽게 위치를 변경
        // camTr.position = Vector3.Slerp(camTr.position, pos, Time.deltaTime * damping);  // 시작 위치, 목표 위치, 시간

        // SmoothDamp을 이용한 위치 보간
        camTr.position = Vector3.SmoothDamp(camTr.position, pos, ref velocity, damping);    // 시작 위치, 목표 위치, 현재 속도, 목표 위치까지 도달할 시간

        // Camera를 피벗 좌표를 향해 회전
        camTr.LookAt(target.position + (target.up * targetOffset));
    }

}
