using System;
using UnityEngine;
using UnityEngine.UI;

public enum StarType
{
    Blue = 0,
    Green = 1,
    Orange = 2,
    Purple = 3,
    Red = 4,
}

public class Star : MonoBehaviour
{
    // unity的脚本，Public的对象/变量，可以在Inspector检查器视图里面可视化编辑
    public int Row = 0;
    public int Column = 0;
    public StarType starType = StarType.Blue;
    public Button button;

    public float moveSpeed = 2;
    public int moveDownCount = 0;
    public bool IsMoveDown = false;
    public int targetRow = 0;

    public int moveLeftCount = 0;
    public bool IsMoveLeft = false; // 开关
    public int targetColumn = 0;    // 目标列

    // 游戏对象身上的脚本组件Star启动时会调用一次
    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(StarClick);
    }

    // 更新方法，每帧会调用执行一次
    private void Update()
    {
        // 注意，全部用LocalPosition
        if (IsMoveDown)
        {
            // 到达目标位置之后
            transform.localPosition += moveSpeed * Time.deltaTime * new Vector3(0, -1, 0);
            if (transform.localPosition.y <= targetRow * GameManager.instance.starWidth)
            {
                // 重置位置
                transform.localPosition = new Vector3(transform.localPosition.x, targetRow * GameManager.instance.starWidth, transform.localPosition.z);
                IsMoveDown = false; // 开关关闭
                // 重置行列值
                moveDownCount = 0;
                Row = targetRow;
            }
        }

        if (IsMoveLeft)
        {
            transform.localPosition += moveSpeed * Time.deltaTime * new Vector3(-1, 0, 0);
            if (transform.localPosition.x <= targetColumn * GameManager.instance.starWidth)
            {
                transform.localPosition = new Vector3(targetColumn * GameManager.instance.starWidth, transform.localPosition.y, transform.localPosition.z);
                IsMoveLeft = false;
                moveLeftCount = 0;
                Column = targetColumn;
            }
        }
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(StarClick);
    }

    public void StarClick()
    {
        if (GameManager.instance.gameState != GameState.StandBy) return;

        // 寻找相邻且颜色相同的星星
        GameManager.instance.FindTheSameStar(this);
        // 把相邻且相同的星星销毁
        GameManager.instance.DestroyTheSameStarList();
        // 销毁了之后，星星的移动
    }

    public void DestorySelf()
    {
        Destroy(gameObject);
    }

    public void OpenMoveDown()
    {
        IsMoveDown = true;
        targetRow = Row - moveDownCount;
    }

    public void OpenMoveLeft()
    {
        IsMoveLeft = true;
        targetColumn = Column - moveLeftCount;
    }
}
