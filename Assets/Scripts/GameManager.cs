using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Ready = 0,
    StandBy = 1,
    Moving = 2,
    GameOver = 3,
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance; // 静态实例，实例之后，就可以通过类名直接调用
    public GameState gameState = GameState.Ready;

    public int MaxRow = 12;
    public int MaxColumn = 10;

    public float starWidth = 48;

    public GameObject[] stars;
    public Transform starListTransform;
    public List<Star> StarList = new();

    public List<Star> theSameStarList = new();

    public AudioSource clearAudio;
    public AudioSource bgAudio;

    public GameObject[] particaleList;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(instance);
        }
    }

    private void Start()
    {
        CreateStarList();

        if (bgAudio != null)
        {
            bgAudio.Play();
        }

        gameState = GameState.StandBy;
    }

    // 生成一个maxRow*maxColumn的星星矩阵；并且该矩阵的星星颜色是随机的
    private void CreateStarList()
    {
        // for循环
        // 0，1，2，3，4，5...，maxRow-1
        for (int r = 0; r < MaxRow; r++)
        {
            for (int c = 0; c < MaxColumn; c++)
            {
                // 当前星星的行列值为r，c
                // r行对应的是y坐标；c列对应的是x坐标
                // Step1：生成（随机）
                int starIndex = Random.Range(0, 5);// 0,1,2,3,4
                if (stars.Length > starIndex)
                {
                    GameObject originalStar = stars[starIndex];
                    if (originalStar != null)
                    {
                        // 克隆originalStar，将克隆出来的cloneStar设置为starListTransform的子物体
                        GameObject cloneStar = Instantiate(originalStar, starListTransform);

                        // Step2：计算出当前星星所处的位置
                        Vector3 currentPos = new(c * starWidth, r * starWidth, cloneStar.transform.position.z);

                        // Step3：设置位置
                        // cloneStar.transform.position Unity中的世界坐标
                        // cloneStar.transform.localPosition 检查器视图中的坐标
                        cloneStar.transform.localPosition = currentPos;

                        // Step4：设置星星的行列信息
                        if (cloneStar.TryGetComponent(out Star cloneStarCom))
                        {
                            cloneStarCom.Row = r;
                            cloneStarCom.Column = c;

                            // Step5：将星星的Star脚本组件添加到集合中
                            StarList.Add(cloneStarCom);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 寻找相邻且颜色相同的星星
    /// 递归
    /// </summary>
    /// <param name="star">相同的星星</param>
    public void FindTheSameStar(Star star)
    {
        int row = star.Row;
        int column = star.Column;
        // StarList集合里面
        foreach (var restStar in StarList)
        {
            // 上 row+1,column
            CompareStar(star, restStar, row + 1, column);
            // 下 row-1,column
            CompareStar(star, restStar, row - 1, column);
            // 左 row,column-1
            CompareStar(star, restStar, row, column - 1);
            // 右 row,column+1
            CompareStar(star, restStar, row, column + 1);
        }
    }

    /// <summary>
    /// 比较两个星星和位置等信息是否符合条件
    /// </summary>
    /// <param name="star"></param>
    /// <param name="restStar"></param>
    /// <param name="row"></param>
    /// <param name="column"></param>
    private void CompareStar(Star star, Star restStar, int row, int column)
    {
        if (restStar.Row == row && restStar.Column == column)
        {
            if (restStar.starType == star.starType)
            {
                if (!theSameStarList.Contains(restStar))
                {
                    theSameStarList.Add(restStar);
                    FindTheSameStar(restStar);
                }
            }
        }
    }

    public void DestroyTheSameStarList()
    {
        if (theSameStarList.Count != 0)
        {
            if (clearAudio != null)
            {
                clearAudio.Play();
            }
        }

        foreach (var sameStar in theSameStarList)
        {
            // 播放粒子特效   1、粒子特效生成播放  2、销毁
            int index = (int)sameStar.starType;
            GameObject currentParticle = particaleList[index];
            GameObject clonePar = Instantiate(currentParticle, starListTransform);
            clonePar.transform.localPosition = sameStar.transform.localPosition;
            Destroy(clonePar, clonePar.GetComponent<ParticleSystem>().main.duration);

            //Destroy(sameStar.gameObject);
            StarList.Remove(sameStar);  // Step1：把剩余星星集合StarList清理一下
            sameStar.DestorySelf();     // Step2：销毁
        }

        // 向下移动
        // 1.从剩余的星星中找到需要向下移动的星星    
        foreach (var restStar in StarList)
        {
            // 2.计算需要向下移动的步数
            foreach (var item in theSameStarList)
            {
                // 找到比要销毁星星的行数高的星星且在同一列上
                if (item.Row < restStar.Row && item.Column == restStar.Column)
                {
                    restStar.moveDownCount++;
                }
            }

            if (restStar.moveDownCount > 0)
            {
                // move Down
                restStar.OpenMoveDown();
            }
        }

        // 向左移动
        // Step1：判断是否有一整列消失，计算需要向左移动的列数
        // 8，7，6，5...0
        for (int col = MaxColumn - 1; col >= 0; col--)
        {
            bool isEmpty = true;    // 默认猜测第col列是空的
            foreach (var restStar in StarList)
            {
                if (restStar.Column == col)
                {
                    isEmpty = false;    // 说明猜测错误，col列其实不为空
                }
            }
            // 如果isEmpty == true。说明猜测对了
            if (isEmpty)
            {
                // 找到右边所有星星，并计算需要向左移动的列数
                foreach (var restStar in StarList)
                {
                    if (restStar.Column > col)
                    {
                        restStar.moveLeftCount++;
                    }
                }
            }


        }

        // MoveLeft
        foreach (var restStar in StarList)
        {
            if (restStar.moveLeftCount > 0)
            {
                // move
                restStar.OpenMoveLeft();
            }
        }

        // 清理theSameStarList
        theSameStarList.Clear();

        Invoke(nameof(JudgeOver), 1);
    }

    public List<Star> judgeOverListStar = new();

    private void JudgeOver()
    {
        bool isOver = true;
        // 遍历剩余所有星星，是否有相邻且颜色相同的星星
        foreach (var item in StarList)
        {
            FindTheSameStar(item);
        }
        if (theSameStarList.Count == 0)
        {
            isOver = false;
            print("GameOver");
        }
        theSameStarList.Clear();
    }
}
