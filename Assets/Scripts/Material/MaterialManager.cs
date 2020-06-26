using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(SpriteRenderer))]
public class MaterialManager : MonoBehaviour
{
    public AbstractMaterial material;
    public int amount;
    public long id;

    private bool spriteSet = false;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if(material != null)
        {
            spriteRenderer.sprite = material.uiSprite;
            spriteSet = true;
        }
    }

    private void Update()
    {
        if(!spriteSet && material != null)
        {
            spriteRenderer.sprite = material.uiSprite;
            spriteSet = true;
        }
    }
}
