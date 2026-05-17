using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Body_Dead : MonoBehaviour
{
    [SerializeField] SpriteRenderer bodySprite;

    public void SetColor(Color newColor)
    {
        bodySprite.color = newColor;
    }

}