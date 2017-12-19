using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RtsCamera : MonoBehaviour
{
	public GameObject goCameraRoot = null;
	public GameObject goCamera = null;

	private float zoomMax = 100;
	private float zoomMin = 20;
	private float zoomCurrent = 30f;
	private float zoomSpeed = 0.1f;

	private float moveRange = 10;
	private float moveSpeed = 0.015f;

	private Vector2 touch0_PrevPos;
	private Vector2 touch1_PrevPos;

#if UNITY_STANDALONE || UNITY_EDITOR
	private Vector2 oldMousePos;
	private Vector2 newMousePos;
	private Vector3 vCamRootPosOld;
	private bool bInTouch;
#endif
	// Use this for initialization
	void Start()
	{
		goCameraRoot.transform.position = new Vector3(-5f, 0, -5f);
		goCamera.transform.localPosition = new Vector3(0, 0, -zoomCurrent);
	}

	// Update is called once per frame
	void Update()
	{
		#region Mobile Platform
#if !UNITY_STANDALONE && !UNITY_EDITOR
		//单指拖动
		if (Input.touchCount == 1)
		{
			Touch touch0 = Input.GetTouch(0);

			if (touch0.deltaPosition.magnitude > 0.2f)
			{
				// 手指在屏幕上滑动产生的向量
				Vector3 vDelta = touch0.deltaPosition * moveSpeed;

				// 求摄像机前方的方向向量（在X-Z平面上）
				Vector3 vForward = goCameraRoot.transform.forward;
				vForward.y = 0.0f;
				vForward.Normalize();

				// 求摄像机右方的方向向量（在X-Z平面上）
				Vector3 vRight = goCameraRoot.transform.right;
				vRight.y = 0.0f;
				vRight.Normalize();

				// 通过手指在屏幕的滑动方向，计算摄像机在空间中的位移
				Vector3 vMove = -vForward * vDelta.y + -vRight * vDelta.x;
				Vector3 targetPos = goCameraRoot.transform.position + vMove;

				//限制前后滑动的范围
				float offsetV = Vector3.Distance(Vector3.zero, targetPos) *
					Vector3.Dot(targetPos.normalized, goCameraRoot.transform.forward.normalized);//Cosθ
				if (offsetV > -moveRange && offsetV < moveRange)
				{
					goCameraRoot.transform.position += -vForward * vDelta.y;
				}

				//限制左右滑动的范围
				float offsetH = Vector3.Distance(Vector3.zero, targetPos) *
					Vector3.Dot(targetPos.normalized, goCameraRoot.transform.right.normalized);//Cosθ
				if (offsetH > -moveRange && offsetH < moveRange)
				{
					goCameraRoot.transform.position += -vRight * vDelta.x;
				}
			}
		}
		//双指缩放
		else if (Input.touchCount == 2)
		{
			// 存储两个手指的触点信息
			Touch touch0 = Input.GetTouch(0);
			Touch touch1 = Input.GetTouch(1);

			// 计算每个触点在上一帧的位置
			touch0_PrevPos = touch0.position - touch0.deltaPosition;
			touch1_PrevPos = touch1.position - touch1.deltaPosition;

			// 上一帧两个触点之间的距离
			float prevTouchDeltaMag = (touch0_PrevPos - touch1_PrevPos).magnitude;//求长度
																				  // 这一帧两个触点之间的距离
			float touchDeltaMag = (touch0.position - touch1.position).magnitude;
			// 两次距离求差，就是手指缩放变化的距离
			float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

			zoomCurrent += deltaMagnitudeDiff * zoomSpeed;
			zoomCurrent = Mathf.Clamp(zoomCurrent, zoomMin, zoomMax);
			goCamera.transform.localPosition = new Vector3(0, 0, -zoomCurrent);//沿Z轴方向移动，调节远近
		}
#endif
		#endregion

		#region Editor or PC Test
#if UNITY_STANDALONE || UNITY_EDITOR

		//鼠标拖拽
		if (Input.GetMouseButtonDown(0))
		{
			oldMousePos = Input.mousePosition;
			//Debug.Log("oldMousePos:" + oldMousePos.ToString());

			vCamRootPosOld = goCameraRoot.transform.position;
			//Debug.Log("vCamRootPosOld:" + vCamRootPosOld.ToString());
		}
		if (Input.GetMouseButton(0))
		{
			newMousePos = Input.mousePosition;
			//Debug.Log("newMousePos:" + newMousePos.ToString());


			if (Vector2.Distance(newMousePos, oldMousePos) > 0.01f)
			{
				Debug.Log("Move!");

				Vector3 vDelta = (newMousePos - oldMousePos) * 0.008f;

				Vector3 vForward = goCameraRoot.transform.forward;
				vForward.y = 0.0f;
				vForward.Normalize();

				Vector3 vRight = goCameraRoot.transform.right;
				vRight.y = 0.0f;
				vRight.Normalize();

				Vector3 vMove = -vForward * vDelta.y + -vRight * vDelta.x;
				goCameraRoot.transform.position = vCamRootPosOld + vMove;
			}
		}


		{//滑轮缩放
			zoomCurrent -= Input.GetAxis("Mouse ScrollWheel") * 5;
			zoomCurrent = Mathf.Clamp(zoomCurrent, zoomMin, zoomMax);
			goCamera.transform.localPosition = new Vector3(0, 0, -zoomCurrent);
		}
#endif
		#endregion
	}
}