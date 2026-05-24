using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardSwapTask : InteractableObject
{
    private Canvas CardUI;
    private Slider SliderUi;
    private bool IsFinished = false;


    private void Awake()
    {
        ProgressTasks.TaskSetup();
    }

    public override void Interact()
    {
        base.Interact();
        CardUI.enabled = true;

        // BẮT BUỘC KHÓA CHÂN NGƯỜI CHƠI CHÍNH
        if (Player.localPlayer != null)
        {
            Player.localPlayer.hasControl = false;
        }
    }

    public override void ExitAction()
    {
        base.ExitAction();
        CardUI.enabled = false;
        SliderUi.value = 0;

        // TRẢ LẠI QUYỀN ĐIỀU KHIỂN KHI THOÁT TASK
        if (Player.localPlayer != null)
        {
            Player.localPlayer.hasControl = true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        base.Initialize();

        // THÊM THAM SỐ (true) ĐỂ TÌM ĐƯỢC CANVAS NGAY CẢ KHI ĐANG ẨN
        CardUI = GetComponentInChildren<Canvas>(true);

        if (CardUI != null)
        {
            SliderUi = CardUI.GetComponentInChildren<Slider>(true);
        }
        else
        {
            Debug.LogError("LỖI: Không tìm thấy Canvas UI nào trong đối tượng " + gameObject.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        base.CheckInteractionInput();

        if (Using)
        {
            if (SliderUi.value >= SliderUi.maxValue)
            {
                IsFinished = true;
                ExitAction();
                SetCanInteract(false);
                ProgressTasks.SetProgress(ProgressTasks.GetProgress() + ProgressTasks.GetDistributedValue());
            }


        }

    }
}
