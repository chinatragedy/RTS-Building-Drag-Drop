using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickUpTest : MonoBehaviour
{
	public Building SelectedBuilding { get; private set; }
	private GameObject selectedGo;
	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		//if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
		//{
		//	// 点击在开启"Raycast Target"的UI组件上，不对物体进行操作
		//	return;
		//}

		if (Input.GetMouseButtonDown(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			// 选中了有效的对象
			if (Physics.Raycast(ray, out hit, 500f, LayerMask.GetMask("Building")))
			{
				// 以前没有选中物体，直接拾取
				if (null == selectedGo)
				{
					PickUp(hit);
				}
				// 和已经拾取的物体一样，放下
				else if (selectedGo == hit.transform.gameObject)
				{
					LayDown();
				}
				// 和已经拾取的物体不一样，先放下上次拾取的物体，再拾取新物体
				else
				{
					LayDown();
					PickUp(hit);
				}
			}
			else // 点击空白处
			{
				//SelectedBuilding = null;
				if (null != selectedGo)
				{
					LayDown();
				}
			}
		}
	}


	private void PickUp(RaycastHit hit)
	{
		selectedGo = hit.transform.gameObject;
		selectedGo.transform.localScale = Vector3.one * 1.3f;
	}
	private void LayDown()
	{
		selectedGo.transform.localScale = Vector3.one;
		selectedGo = null;
	}

	#region Tools
	public T FindComponent<T>(GameObject go) //where T : MonoBehaviour
	{
		T component = go.GetComponent<T>();
		if (component == null)
		{
			component = go.transform.parent.gameObject.GetComponent<T>();
		}
		return component;
	}
	#endregion
}