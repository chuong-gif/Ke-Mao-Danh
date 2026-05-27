using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Body_Dead : MonoBehaviour
{
    [SerializeField] SpriteRenderer bodySprite; // Kéo SpriteRenderer của xác chết vào đây

    public void SetColor(Color newColor) // Hàm này sẽ được gọi để thay đổi màu sắc của xác chết
    {
        bodySprite.color = newColor;
    }

}