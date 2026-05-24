using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DownloadMinigame : InteractableObject
{
    public float Download_Speed = 0.065f;
    private Canvas DownloadUI;
    private Slider SliderUi;
    private bool IsFinished = false;

    private void Awake()
    {
        ProgressTasks.TaskSetup();
    }

    // Start is called before the first frame update
    void Start()
    {
        base.Initialize();

        // THÊM THAM SỐ (true) ĐỂ TÌM ĐƯỢC CANVAS NGAY CẢ KHI ĐANG ẨN
        DownloadUI = GetComponentInChildren<Canvas>(true);

        if (DownloadUI != null)
        {
            SliderUi = DownloadUI.GetComponentInChildren<Slider>(true);
        }
        else
        {
            Debug.LogError("LỖI: Không tìm thấy Canvas UI nào trong đối tượng " + gameObject.name);
        }
    }

    public override void Interact()
    {
        base.Interact();
        DownloadUI.enabled = true;

        // BẮT BUỘC KHÓA CHÂN NGƯỜI CHƠI CHÍNH
        if (Player.localPlayer != null)
        {
            Player.localPlayer.hasControl = false;
        }
    }

    public override void ExitAction()
    {
        base.ExitAction();
        DownloadUI.enabled = false;
        SliderUi.value = 0f;

        // TRẢ LẠI QUYỀN ĐIỀU KHIỂN KHI THOÁT TASK
        if (Player.localPlayer != null)
        {
            Player.localPlayer.hasControl = true;
        }
    }

    // Update is called once per frame
    void Update()
    {

        base.CheckInteractionInput();


        if (Using)
        {
            if (SliderUi.value < SliderUi.maxValue)
            {
                SliderUi.value += Download_Speed * Time.deltaTime;
            }
            else
            {
                IsFinished = true;
                ExitAction();
                SetCanInteract(false);
                ProgressTasks.SetProgress(ProgressTasks.GetProgress() + ProgressTasks.GetDistributedValue());
            }


        }
    }
}
