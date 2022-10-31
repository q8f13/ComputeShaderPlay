using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class GOLMain : MonoBehaviour
{
    const int RES = 512;
    RenderTexture _rt;
    Texture2D _rtMiddle;

    [SerializeField]
    ComputeShader _golShader;

    [SerializeField]
    RawImage _img;
    [SerializeField]
    RawImage _imgMiddle;

    RectTransform _imgRt;

    float[,] _grid_data;

    bool _ready = false;
    bool _do_update = true;

    int _kernelMain;
	private Canvas _canvas;

    private void Start() {

        _imgRt = _img.GetComponent<RectTransform>();
        _canvas = FindObjectOfType<Canvas>();
        _rt = new RenderTexture(RES, RES, 0);
        _rt.enableRandomWrite = true;
        _rt.filterMode = FilterMode.Point;
        _rt.Create();

        _rtMiddle = new Texture2D(RES, RES, TextureFormat.RGB24, 1, true);
        _rtMiddle.filterMode = FilterMode.Point;

        Color[] colors = new Color[RES*RES];
        for(int i=0;i<RES;i++)
        {
            for(int j=0;j<RES;j++)
            {
                float val = Mathf.Round(Random.value);
                colors[j*RES + i] = new Color(val, val, val, 1.0f);
            }
        }

        _rtMiddle.SetPixels(0, 0,RES,RES, colors);
        _rtMiddle.Apply();
        Graphics.Blit(_rtMiddle, _rt);

        _img.texture = _rt;
        _imgMiddle.texture = _rtMiddle;

        _kernelMain = _golShader.FindKernel("CSMain");

        _golShader.SetTexture(_kernelMain, "Result", _rt);
        _golShader.SetFloat("Resolution", RES);

        _golShader.Dispatch(_kernelMain, _rt.width / 8, _rt.height / 8, 1);

        _ready = true;
    }

	// public Vector2 ScreenToRectPos(Vector2 screen_pos)
	// {
	// 	if (canvas.renderMode != RenderMode.ScreenSpaceOverlay && canvas.worldCamera != null)
	// 	{
	// 		//Canvas is in Camera mode
	// 		Vector2 anchorPos;
	// 		RectTransformUtility.ScreenPointToLocalPointInRectangle(_imgRt, screen_pos, canvas.worldCamera, out anchorPos);
	// 		return anchorPos;
	// 	}
	// 	else
	// 	{
	// 		//Canvas is in Overlay mode
	// 		Vector2 anchorPos = screen_pos - new Vector2(_imgRt.position.x, _imgRt.position.y);
	// 		anchorPos = new Vector2(anchorPos.x / _imgRt.lossyScale.x, anchorPos.y / _imgRt.lossyScale.y);
	// 		return anchorPos;
	// 	}
	// }

    private void Update() {
        if(!_ready)
            return;

        _do_update = !Input.GetMouseButton(0);

        _img.enabled = _do_update;
        _imgMiddle.enabled = !_do_update;
        if(!_do_update)
        {
            if(Input.GetMouseButtonDown(0))
            {
                RenderTexture.active = _rt;
                _rtMiddle.ReadPixels(new Rect(0, 0, RES, RES), 0, 0, false);
                _rtMiddle.Apply();
                RenderTexture.active = null;
            }
            else
            {
                Vector2 lp = UIUtils.ScreenToRectPos(Input.mousePosition, _imgRt, _canvas);

                int x = Mathf.RoundToInt(lp.x) + RES/2;
                int y = Mathf.RoundToInt(lp.y) + RES/2;
                for(int i = x -10;i< x + 10;i++)
                {
                    if(i < 0 || i >= RES)
                        continue;
                    for(int j=y-10;j<y+10;j++)
                    {
                        if(j < 0 || j >= RES)
                            continue;
                        _rtMiddle.SetPixel(i,j, Color.white);
                    }
                }
                _rtMiddle.Apply();
                Graphics.Blit(_rtMiddle, _rt);
            }
        }
        else
        {
            if(Time.frameCount % 20 == 0)
            {
                _golShader.SetTexture(_kernelMain, "Result", _rt);
                _golShader.Dispatch(_kernelMain, _rt.width / 8, _rt.height / 8, 1);
            }
        }
    }
}
