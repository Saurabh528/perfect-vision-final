using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SpriteColorCalibrate : MonoBehaviour
{
	public float changePeriod, randomPeriod;
    
    public ColorChannel _ColorChannel;
	public Material[] materials;
	float switchRemainTime;
	// Start is called before the first frame update
	void Start()
    {
		if(materials == null)
			materials = new Material[0];//In case of adding componet from script at runtime.
		Color color = Color.white;
		if (_ColorChannel == ColorChannel.CC_Background)
			color = ColorCalibration.BackColor;
		else if (_ColorChannel == ColorChannel.CC_Red)
			color = ColorCalibration.RedColor;
		else if (_ColorChannel == ColorChannel.CC_Cyan)
			color = ColorCalibration.CyanColor;
		else
		{
			if(Random.value > 0.5f)
				color = ColorCalibration.RedColor;
			else
				color = ColorCalibration.CyanColor;
		}
		SetColor(color);
		if (changePeriod == 0)
			Destroy(this);
		else
			switchRemainTime = changePeriod;
	}

	void SetColor(Color color)
	{
		foreach (Material mat in materials)
		{
			mat.color = color;
		}

		SpriteRenderer render = GetComponent<SpriteRenderer>();
		if (render)
		{
			render.color = color;
			return;
		}

		Camera cam = GetComponent<Camera>();
		if (cam)
		{
			cam.backgroundColor = color;
			return;
		}

		MeshRenderer mrender = GetComponent<MeshRenderer>();
		if (mrender)
		{
			mrender.material.color = new Color(color.r, color.g, color.b, mrender.material.color.a);
			return;
		}

		Image image = GetComponent<Image>();
		if (image)
		{
			image.color = color;
			return;
		}
	}

	private void Update()
	{
		if(switchRemainTime != 0){
			switchRemainTime -= Time.deltaTime;
			if(switchRemainTime < 0)
			{
				switchRemainTime = changePeriod + Random.value * randomPeriod;
				if(switchRemainTime == 0)
					switchRemainTime = 0.1f;
				Color color = (Random.value > 0.5f) ? ColorCalibration.RedColor : ColorCalibration.CyanColor;
				SetColor(color);
			}
		}
		
	}
}
