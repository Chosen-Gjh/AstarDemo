using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Vec2
{
    //地图表格大小
    public static int Row = 9;

    public static int Col = 14;

    //节点信息
    public bool IsBarrier; //是否是障碍物
    public int x;
    public int y;
    public Vec2 parent;

    //总估值
    public int F
    {
        get { return this.G + this.H; }
    }

    //起始节点到此节点的距离估值
    public int G;

    //此节点到终点的距离估值
    public int H;

    public Vec2(int x, int y, Vec2 parent = null)
    {
        this.x = x;
        this.y = y;
        this.parent = parent;
    }
}

public class AstartCtr : MonoBehaviour
{
    public Canvas canvas;
    public GameObject MapUnit;
    public Transform Content;
    public Vec2 StartVec2;
    public Vec2 EndVec2;
    private Image LastStartImage;
    private Image LastEndImage;
    private int time = 0;
    private Queue<Vec2> BFSQue = new Queue<Vec2>();
    private List<string> MarkList = new List<string>();
    List<Vec2> OpenList = new List<Vec2>();
    List<Vec2> CloseList = new List<Vec2>();
    private Vec2[,] Map = new Vec2[10, 15];

    // Start is called before the first frame update
    void Start()
    {
        InitMap();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(1))
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = Input.mousePosition;
            GraphicRaycaster gr = canvas.GetComponent<GraphicRaycaster>();
            List<RaycastResult> results = new List<RaycastResult>();
            gr.Raycast(pointerEventData, results);
            if (results.Count != 0)
            {
                if (results[0].gameObject.GetComponent<Image>().color == Color.black)
                    results[0].gameObject.GetComponent<Image>().color = Color.white;
                else
                {
                    results[0].gameObject.GetComponent<Image>().color = Color.black;
                }
            }
        }
    }

    void InitMap()
    {
        for (int i = 0; i < Vec2.Row; i++)
        {
            for (int j = 0; j < Vec2.Col; j++)
            {
                var NewObj = Instantiate(MapUnit);
                NewObj.transform.SetParent(Content);
                NewObj.transform.name = $"{i + 1}/{j + 1}";
                Map[i + 1, j + 1] = new Vec2(i + 1, j + 1);
                var Image = NewObj.GetComponent<Image>();
                NewObj.GetComponent<Button>()
                    .onClick.AddListener(delegate
                    {
                        if (time % 2 == 0)
                        {
                            if (LastStartImage) LastStartImage.color = Color.white;
                            Image.color = Color.green;
                            StartVec2 = StringTransferVec2(NewObj.name);
                            LastStartImage = Image;
                        }
                        else
                        {
                            if (LastEndImage) LastEndImage.color = Color.white;
                            Image.color = Color.red;
                            EndVec2 = StringTransferVec2(NewObj.name);
                            LastEndImage = Image;
                        }

                        time++;
                    });
            }
        }
    }

    public void ReSetMap()
    {
        for (int i = 0; i < Vec2.Row; i++)
        {
            for (int j = 0; j < Vec2.Col; j++)
            {
                GameObject.Find($"{i + 1}/{j + 1}").GetComponent<Image>().color = Color.white;
            }
        }

        BFSQue.Clear();
        MarkList.Clear();
    }

    Vec2 StringTransferVec2(string rStringVec2)
    {
        var Strs = rStringVec2.Split('/');
        return Map[int.Parse(Strs[0]), int.Parse(Strs[1])];
    }

    string Vec2Transferstring(Vec2 vec2)
    {
        return $"{vec2.x}/{vec2.y}";
    }

    string Vec2Transferstring(int x, int y)
    {
        return $"{x}/{y}";
    }

    public void FindDestinationBFS()
    {
        if (this.StartVec2 == null || this.EndVec2 == null)
        {
            Debug.Log("Please Init Start&End Position!");
            return;
        }

        StartCoroutine(BFSFindeDestination_Handler());
    }

    public void FindDestinationBFS2()
    {
        if (this.StartVec2 == null || this.EndVec2 == null)
        {
            Debug.Log("Please Init Start&End Position!");
            return;
        }

        StartCoroutine(BFSFindeDestination());
    }

    [Obsolete]
    public IEnumerator BFSFindeDestination()
    {
        BFSQue.Enqueue(StartVec2);
        MarkList.Add(Vec2Transferstring(StartVec2));
        int rTime = 0;
        while (this.BFSQue.Count > 0)
        {
            if (rTime > 100000) yield break;
            var CurrentVec2 = BFSQue.Dequeue();
            GameObject.Find(Vec2Transferstring(CurrentVec2)).GetComponent<Image>().color = Color.cyan;
            MarkList.Add(Vec2Transferstring(CurrentVec2));
            //if finded
            if (CurrentVec2.x == EndVec2.x && CurrentVec2.y == EndVec2.y)
            {
                Debug.Log($"Finded EndPos Is{CurrentVec2.x},{CurrentVec2.y}");
                yield break;
            }

            //Enqueue never mark Pos
            if (CurrentVec2.x - 1 > 0 && !MarkList.Contains(Vec2Transferstring(CurrentVec2.x - 1, CurrentVec2.y)))
                BFSQue.Enqueue(Map[CurrentVec2.x - 1, CurrentVec2.y]);
            if (CurrentVec2.y - 1 > 0 && !MarkList.Contains(Vec2Transferstring(CurrentVec2.x, CurrentVec2.y - 1)))
                BFSQue.Enqueue(Map[CurrentVec2.x, CurrentVec2.y - 1]);
            if (CurrentVec2.x + 1 < Vec2.Row && !MarkList.Contains(Vec2Transferstring(CurrentVec2.x + 1, CurrentVec2.y))
            )
                BFSQue.Enqueue(Map[CurrentVec2.x + 1, CurrentVec2.y]);
            if (CurrentVec2.y + 1 < Vec2.Col && !MarkList.Contains(Vec2Transferstring(CurrentVec2.x, CurrentVec2.y + 1))
            )
                BFSQue.Enqueue(Map[CurrentVec2.x, CurrentVec2.y + 1]);
            yield return new WaitForSeconds(0.2f);
            GameObject.Find(Vec2Transferstring(CurrentVec2)).GetComponent<Image>().color = Color.gray;
            rTime++;
        }
    }

    IEnumerator BFSFindeDestination_Handler()
    {
        BFSQue.Enqueue(StartVec2);
        MarkList.Add(Vec2Transferstring(StartVec2));
        while (this.BFSQue.Count > 0)
        {
            var CurrentVec2 = BFSQue.Dequeue();
            GameObject.Find(Vec2Transferstring(CurrentVec2)).GetComponent<Image>().color = Color.cyan;
            MarkList.Add(Vec2Transferstring(CurrentVec2));
            //if finded
            if (CurrentVec2.x == EndVec2.x && CurrentVec2.y == EndVec2.y)
            {
                Debug.Log($"Finded EndPos Is{CurrentVec2.x},{CurrentVec2.y}");
                StopAllCoroutines();
            }

            //Enqueue never mark Pos
            NodeCheck(Map[CurrentVec2.x + 1, CurrentVec2.y]);
            NodeCheck(Map[CurrentVec2.x, CurrentVec2.y - 1]);
            NodeCheck(Map[CurrentVec2.x - 1, CurrentVec2.y]);
            NodeCheck(Map[CurrentVec2.x, CurrentVec2.y + 1]);
            yield return new WaitForSeconds(0.2f);
            GameObject.Find(Vec2Transferstring(CurrentVec2)).GetComponent<Image>().color = Color.gray;
        }

        Debug.Log("Can't Find Destination!");
    }

    void NodeCheck(Vec2 rVec2)
    {
        if (rVec2 == null) return;
        if (rVec2.y < Vec2.Col && rVec2.y > 0 && rVec2.x < Vec2.Row && rVec2.x > 0 &&
            !MarkList.Contains(Vec2Transferstring(rVec2.x, rVec2.y)) &&
            GameObject.Find(Vec2Transferstring(rVec2)).GetComponent<Image>().color != Color.black)
        {
            var Node = Map[rVec2.x, rVec2.y];
            BFSQue.Enqueue(Node);
            GameObject.Find(Vec2Transferstring(Node)).GetComponent<Image>().color = Color.green;
            MarkList.Add(Vec2Transferstring(Node));
        }
    }

    public void AstarIEnumrator()
    {
        StartCoroutine(Astar());
    }

    public IEnumerator Astar()
    {
        this.OpenList.Clear();
        this.CloseList.Clear();
        if (this.StartVec2 == null || this.EndVec2 == null)
        {
            Debug.Log("Please Init Start&End Position!");
            yield break;
        }
        OpenList.Add(StartVec2);
        while (OpenList.Count > 0)
        {
            Vec2 MinCastVec2 = OpenList.Count==1?OpenList[0]:FindMinCastVec2(OpenList); //寻找代价最小的点
            GameObject.Find(Vec2Transferstring(MinCastVec2)).GetComponent<Image>().color = Color.cyan;
            yield return new WaitForSeconds(0.2f);
            OpenList.Remove(MinCastVec2);
            CloseList.Add(MinCastVec2);
            //around current
            // 如果是目的节点，返回
            if (MinCastVec2.x == EndVec2.x && MinCastVec2.y == EndVec2.y)
            {
                //生成路径
                GeneratePath();
                Debug.Log($"End Pos{EndVec2.x}/{EndVec2.y}");
                yield break;
            }
            NeighbourCheck(MinCastVec2,Map[MinCastVec2.x-1,MinCastVec2.y]);
            NeighbourCheck(MinCastVec2,Map[MinCastVec2.x+1,MinCastVec2.y]);
            NeighbourCheck(MinCastVec2,Map[MinCastVec2.x,MinCastVec2.y+1]);
            NeighbourCheck(MinCastVec2,Map[MinCastVec2.x,MinCastVec2.y-1]);
        }
    }
    private void GeneratePath()
    {
        Stack<Vec2> path = new Stack<Vec2>();
        Vec2 node = this.EndVec2;
        while (node.parent != this.StartVec2)
        {
            path.Push(node);
            node = node.parent;
            Debug.Log(node.x+" "+ node.y);
            GameObject.Find(Vec2Transferstring(node)).GetComponent<Image>().color = Color.magenta;
        }
        GameObject.Find(Vec2Transferstring(EndVec2)).GetComponent<Image>().color = Color.magenta;
        //m_Grid.m_Path = path;
    }
    private void NeighbourCheck(Vec2 rCurrent, Vec2 rVec2)
    {
        if (rVec2 == null) return;
        if ( GameObject.Find(Vec2Transferstring(rVec2)).GetComponent<Image>().color==Color.black|| this.CloseList.Contains(rVec2))
        {
            return;
        }
        int rGCost = rCurrent.G + GetDistanceNodes(rCurrent, rVec2);
        // 如果新路径到相邻点的距离更短 或者不在开启列表中
        if (rGCost < rVec2.G || !this.OpenList.Contains(rVec2))
        {
            // 更新相邻点的F，G，H
            rVec2.G = rGCost;
            rVec2.H = GetDistanceNodes(rVec2, this.EndVec2);
            // 设置相邻点的父节点为当前节点
            rVec2.parent = rCurrent;
            // 如果不在开启列表中，加入到开启列表中
            if (!this.OpenList.Contains(rVec2))
            {
                this.OpenList.Add(rVec2);
            }
        }
    }

    /// <summary>
    /// 获得两个节点的距离
    /// </summary>
    /// <param name="node1"></param>
    /// <param name="node2"></param>
    /// <returns></returns>
    private int GetDistanceNodes(Vec2 rvec1, Vec2 rvec2)
    {
        int deltaX = Mathf.Abs(rvec1.x - rvec2.x);
        int deltaY = Mathf.Abs(rvec1.y - rvec2.y);
        if (deltaX > deltaY)
        {
            return deltaY * 14 + 10 * (deltaX - deltaY);
        }
        else
        {
            return deltaX * 14 + 10 * (deltaY - deltaX);
        }
    }

    //寻找最小点
    private Vec2 FindMinCastVec2(List<Vec2> List)
    {
        int targetF = int.MaxValue;
        Vec2 tmp = null;
        foreach (var vec2 in List)
        {
            if (vec2.F < targetF)
            {
                tmp = vec2;
                targetF = vec2.F;
            }
            else if (vec2.F == targetF && vec2.H < tmp.H)
            {
                tmp = vec2;
            }
        }
        return tmp;
    }
}