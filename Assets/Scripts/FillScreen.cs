using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FillScreen : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private Camera _camera;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _camera = Camera.main;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateFillScreen();
    }

    private void UpdateFillScreen()
    {
        var height = _camera.orthographicSize*2;
        var width = height*Screen.width/Screen.height;

        var aspect = (float)Screen.width / Screen.height;
        var imgAspect =  _renderer.sprite.bounds.extents.x/ _renderer.sprite.bounds.extents.y;

        if (aspect >= imgAspect)
        {
            transform.localScale = Vector3.one*width/(2*_renderer.sprite.bounds.extents.x);
        }
        else
        {
            transform.localScale = Vector3.one * height / (2 * _renderer.sprite.bounds.extents.y);
        }
        

    }
}
