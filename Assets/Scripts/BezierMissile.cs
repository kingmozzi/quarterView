using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierMissile : Bullet
{
    Vector3[] m_points = new Vector3[4];

    float m_timerMax;
    float m_timerCurrent;
    public float m_speed =2;
    public float _newPointFromStart = 6.0f; // 시작 지점을 기준으로 얼마나 꺾일지.
    public float _newPointFromEnd = 3.0f; // 도착 지점을 기준으로 얼마나 꺾일지.

    public void init(Transform _start, Transform _end)
    {
        m_timerMax=Random.Range(0.8f, 1.0f);
        m_points[0] = _start.position + -1.0f * _start.forward;

        m_points[1] = _start.position +
            (_newPointFromStart * Random.Range(-2.0f, 2.0f) * _start.right) +
            (_newPointFromStart * Random.Range(-0.15f, 1.0f) * _start.up) +
            (_newPointFromStart * -2.0f * _start.forward);
        
        m_points[2] = _end.position +
            (_newPointFromEnd * Random.Range(-1.0f, 1.0f) * _start.right) +
            (_newPointFromEnd * Random.Range(-1.0f, 1.0f) * _end.up) +
            (_newPointFromEnd * Random.Range(0.8f, 1.0f) * _end.forward);
    
        m_points[3] = _end.position;

        transform.position = _start.position;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (m_timerCurrent > m_timerMax)
        {
            return;
        }

        // 경과 시간 계산.
        m_timerCurrent += Time.deltaTime * m_speed;

        // 베지어 곡선으로 X,Y,Z 좌표 얻기.
        transform.position = new Vector3(
            CubicBezierCurve(m_points[0].x, m_points[1].x, m_points[2].x, m_points[3].x),
            CubicBezierCurve(m_points[0].y, m_points[1].y, m_points[2].y, m_points[3].y),
            CubicBezierCurve(m_points[0].z, m_points[1].z, m_points[2].z, m_points[3].z)
        );
    }

        private float CubicBezierCurve(float a, float b, float c, float d)
    {
        // (0~1)의 값에 따라 베지어 곡선 값을 구하기 때문에, 비율에 따른 시간을 구했다.
        float t = m_timerCurrent / m_timerMax; // (현재 경과 시간 / 최대 시간)

        // 방정식.
        /*
        return Mathf.Pow((1 - t), 3) * a
            + Mathf.Pow((1 - t), 2) * 3 * t * b
            + Mathf.Pow(t, 2) * 3 * (1 - t) * c
            + Mathf.Pow(t, 3) * d;
        */

        // 이해한대로 편하게 쓰면.
        float ab = Mathf.Lerp(a, b, t);
        float bc = Mathf.Lerp(b, c, t);
        float cd = Mathf.Lerp(c, d, t);

        float abbc = Mathf.Lerp(ab, bc, t);
        float bccd = Mathf.Lerp(bc, cd, t);

        return Mathf.Lerp(abbc, bccd, 0f);
    }
}
