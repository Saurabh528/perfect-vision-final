using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FlyScript : MonoBehaviour
{
	public enum HighlightMode
	{
		Normal,
		Right,
		Wrong
	};
	public float m_Speed = 1;
	public float m_XScale = 1;
	public float m_YScale = 1;

	Vector3 m_Pivot;
	Vector3 m_PivotOffset;
	Vector3 m_prevPos;
	float m_Phase;
	bool m_Invert;
	float m_2PI = Mathf.PI * 2;
	public Sprite _sHighlight, _sSelectRight, _sSelectWrong;
	public SpriteRenderer _renderHightlight;
	void Awake()
	{
		m_Speed += Random.Range(-0.3f, 0.3f);
		m_XScale += Random.Range(-0.3f, 0.3f);
		m_YScale += Random.Range(-0.3f, 0.3f);
		m_Pivot = transform.position;
		m_prevPos = transform.position;
		m_Phase = Random.Range(0f, m_2PI);
		transform.position = m_Pivot + (m_Invert ? m_PivotOffset : Vector3.zero) + new Vector3(Mathf.Cos(m_Phase) * (m_Invert ? -1 : 1) * m_XScale, Mathf.Sin(m_Phase) * m_YScale, 0);
		float nextTime = m_Phase + 0.1f;
		Vector3 nextpos = m_Pivot + (m_Invert ? m_PivotOffset : Vector3.zero) + new Vector3(Mathf.Cos(nextTime) * (m_Invert ? -1 : 1) * m_XScale, Mathf.Sin(nextTime) * m_YScale, 0);
		transform.up = (nextpos - transform.position).normalized;
	}

	void Update()
	{
		m_PivotOffset = Vector3.right * 2 * m_XScale;
		float realspeed = m_Speed + GamePlayController.GetDifficultyValue(1, 0, 20, 5);
		m_Phase += realspeed * Time.deltaTime;
		if (m_Phase > m_2PI)
		{
			m_Invert = !m_Invert;
			m_Phase -= m_2PI;
		}
		if (m_Phase < 0) m_Phase += m_2PI;

		transform.position = m_Pivot + (m_Invert ? m_PivotOffset : Vector3.zero) + new Vector3(Mathf.Cos(m_Phase) * (m_Invert ? -1 : 1) * m_XScale, Mathf.Sin(m_Phase) * m_YScale, 0);
		transform.up = Vector3.Lerp(transform.up, (transform.position - m_prevPos).normalized, Time.deltaTime * 10);
		_renderHightlight.transform.up = Vector3.up;
		_renderHightlight.transform.up = Vector3.up;
		m_prevPos = transform.position;
	}

	public void ShowHighlight(bool enable, HighlightMode mode = HighlightMode.Normal)
	{
		if (enable)
		{
			_renderHightlight.enabled = true;
			if (mode == HighlightMode.Normal)
				_renderHightlight.sprite = _sHighlight;
			else if (mode == HighlightMode.Right)
				_renderHightlight.sprite = _sSelectRight;
			else if (mode == HighlightMode.Wrong)
				_renderHightlight.sprite = _sSelectWrong;
		}
		else
			_renderHightlight.enabled = false;
	}
}
