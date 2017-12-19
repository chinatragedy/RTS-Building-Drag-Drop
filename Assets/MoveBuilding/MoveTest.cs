using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTest : MonoBehaviour
{
	public GameObject goCameraRoot = null;
	public GameObject goCamera = null;
	public Plane xzPlane;

	private float zoomMax = 100;
	private float zoomMin = 20;
	private float zoomCurrent = 30f;
	private float zoomSpeed = 0.1f;

	private float moveRange = 10;
	private float moveSpeed = 0.015f;

	private Vector2 touch0_PrevPos;
	private Vector2 touch1_PrevPos;

#if TEST || UNITY_STANDALONE || UNITY_EDITOR
	private Vector2 oldMousePos;
	private Vector2 newMousePos;
	private Vector3 oldCamRootPos;

#endif

	private GameObject oldSelectedGo;
	private GameObject selectedGo;


	private bool isDrag;
	private bool isHold;

	//private bool isSelectThisClick;

	private OptionType optionType;

	enum OptionType
	{
		none = 0,
		tureClick,
		falseClick,
		trueDrag,
		falseDrag
	}
	// Use this for initialization
	void Start()
	{
		goCameraRoot.transform.position = new Vector3(-5f, 0, -5f);
		goCamera.transform.localPosition = new Vector3(0, 0, -zoomCurrent);

		xzPlane = new Plane(new Vector3(0f, 1f, 0f), 0f);
	}

	// Update is called once per frame
	void Update()
	{


		#region 
#if TEST //UNITY_ANDROID || UNITY_STANDALONE || UNITY_EDITOR

		if (Input.touchCount == 1)
		{
			/// 按下事件(用于记录一些旧数据)
			if (Input.GetMouseButtonDown(0))
			{ 
				oldMousePos = Input.mousePosition;
				oldCamRootPos = goCameraRoot.transform.position;
				isDrag = false;
			}

			/// 按住事件
			if (Input.GetMouseButton(0))
			{
				newMousePos = Input.mousePosition;

				Ray ray = Camera.main.ScreenPointToRay(newMousePos);
				RaycastHit hit;

				if (Vector2.Distance(newMousePos, oldMousePos) > 5f && !isDrag)
				{
					isDrag = true;
					//TODO:显示网格			
				}

				///滑动部分
				if (isDrag)
				{
					if (Physics.Raycast(ray, out hit, 500f, LayerMask.GetMask("Building")) && !isHold)
					{
						selectedGo = hit.transform.gameObject;
						if (null != oldSelectedGo && null != selectedGo && oldSelectedGo == selectedGo)
						{
							isHold = true;
						}
					}

					if (isHold)
					{
						// 拖拽选中建筑 move selected building
						float enter;
						xzPlane.Raycast(ray, out enter);
						Vector3 targetPos = ray.GetPoint(enter);

						oldSelectedGo.transform.position = targetPos;
					}
					else
					{
						// 拖拽相机 move rootCamera
						Vector3 vDelta = (newMousePos - oldMousePos) * 0.008f;

						Vector3 vForward = goCameraRoot.transform.forward;
						vForward.y = 0.0f;
						vForward.Normalize();

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
						//goCameraRoot.transform.position = oldCamRootPos + vMove;
					}
				}
			}

			/// 抬起事件
			if (Input.GetMouseButtonUp(0))
			{
				if (isDrag) { isHold = false; return; }

				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;

				// 选中了有效的对象
				if (Physics.Raycast(ray, out hit, 500f, LayerMask.GetMask("Building")))
				{
					selectedGo = hit.transform.gameObject;

					// 以前没有选中物体，直接拾取
					if (null == oldSelectedGo)
					{
						PickUp(selectedGo);
						selectedGo = null;
					}
					// 和已经拾取的物体一样，放下
					else if (oldSelectedGo == selectedGo)
					{
						LayDown();
					}
					// 和已经拾取的物体不一样，先放下上次拾取的物体，再拾取新物体
					else
					{
						LayDown();
						PickUp(selectedGo);
						selectedGo = null;
					}
				}
				else// 点击空白处
				{
					LayDown();
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
		{//滑轮缩放
			zoomCurrent -= Input.GetAxis("Mouse ScrollWheel") * 5;
			zoomCurrent = Mathf.Clamp(zoomCurrent, zoomMin, zoomMax);
			goCamera.transform.localPosition = new Vector3(0, 0, -zoomCurrent);
		}
#endif
		#endregion


		//if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
		//{
		//	// 点击在开启"Raycast Target"的UI组件上，不对物体进行操作
		//	return;
		//}
	}

	private void PickUp(GameObject selectedGo)
	{
		if (null != selectedGo)
		{
			oldSelectedGo = selectedGo;
			oldSelectedGo.transform.localScale = Vector3.one * 1.3f;
		}
	}
	private void LayDown()
	{
		if (null != oldSelectedGo)
		{
			oldSelectedGo.transform.localScale = Vector3.one;
			oldSelectedGo = null;
		}
	}
}