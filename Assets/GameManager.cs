using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;

public class GameManager : SingleTon<GameManager>
{
    public const int NOPLAYER = 0;
    public const int P1 = 1;
    public const int P2 = 2;

    public int gameTurn = P1;  // 游戏回合，PLAYER1代表这个游戏回合是玩家1的，PLAYER2代表是玩家2的回合，NOPLAYER代表结束
    public int totalMoves = 0;      // 两个玩家总共进行的回合数
    public int [,] chessBoard;      // 井字棋盘
    public bool mode = true;        // true为单人，false为双人
    private List<Image> imgs;

    public Transform parent_btn;
    public Text text_state;
    public Text text_turn;

    public Sprite img_blue;
    public Sprite img_red;

    private void Awake()
    {
        var prefab_btn = Resources.Load<Button>("prefab_btn");
        imgs = new List<Image>(16);
        for (int i = 0; i < 9; i++)
        {
            Button btn = GameObject.Instantiate<Button>(prefab_btn, parent_btn);
            Image image = btn.transform.Find("Image").GetComponent<Image>();
            imgs.Add(image);
            image.sprite = null;
            int curValue = i;
            btn.onClick.AddListener(() => {
                OnClickChess(curValue % 3, curValue / 3);}
            );
        }
        chessBoard = new int[3, 3];
        Init(true);
        OnLoadState();
    }

    private void Init(bool _mode, bool reset = true)
    {
        // 设置游戏的玩家数
        mode = _mode;
        if (reset)
        {
            gameTurn = P1;
            totalMoves = 0;
            for (int i = 0; i < 9; i++)
            {
                chessBoard[i % 3, i / 3] = NOPLAYER;
            }
        }
        OnRefresh();
    }

    private void AIMove()
    {
        if(!mode || totalMoves >= 9 || CheckWinner() != NOPLAYER)
        {
            return;
        }
        System.Random ran = new System.Random();
        int randomNum = ran.Next(0, 8);
        int xIndex, yIndex;
        do {
            // 算出随机数对应的横坐标和纵坐标
            xIndex = randomNum % 3;
            yIndex = randomNum / 3;
            // 如果棋盘上对应的棋格已有玩家，那么随机数加一寻找另外没有玩家的棋格
            randomNum = (randomNum + 1) % 8;
        } while (chessBoard[xIndex, yIndex] != NOPLAYER);
        // 设置棋格和游戏回合，总回合数加一
        chessBoard[xIndex, yIndex] = gameTurn;
        gameTurn = (gameTurn == P1 ? P2 : P1);
        totalMoves++;
        print("机器人走了一步");
    }

    // 检查胜利
    private int CheckWinner()
    {
        // 检查3行3列的6种赢的情况
        for (int i = 0; i < 3; ++i) {
            if (chessBoard[i, 0] != NOPLAYER &&
                (chessBoard[i, 0] ^ chessBoard[i, 1]) == 0 &&
                (chessBoard[i, 1] ^ chessBoard[i, 2]) == 0) {
                gameTurn = NOPLAYER;
                return chessBoard[i, 0];
            }
            if (chessBoard[0, i] != NOPLAYER &&
                (chessBoard[0, i] ^ chessBoard[1, i]) == 0 &&
                (chessBoard[1, i] ^ chessBoard[2, i]) == 0) {
                gameTurn = NOPLAYER;
                return chessBoard[0, i];
            }
        }
        // 检查对角线的2种赢的情况
        if (chessBoard[1, 1] != NOPLAYER) {
            if ((chessBoard[0, 0] ^ chessBoard[1, 1]) == 0 &&
                (chessBoard[1, 1] ^ chessBoard[2, 2]) == 0) {
                gameTurn = NOPLAYER;
                return chessBoard[1, 1];
            }
            if ((chessBoard[0, 2] ^ chessBoard[1, 1]) == 0 &&
                (chessBoard[1, 1] ^ chessBoard[2, 0]) == 0)
            {
                gameTurn = NOPLAYER;
                return chessBoard[1, 1];
            }
        }
        // 没人胜出，那么返回NOPLAYER，代表没有赢家
        return NOPLAYER;
    }

    // 刷新显示
    private void OnRefresh()
    {
        text_state.text = GetTipText();
        text_turn.text = $"Turn: {totalMoves}";
        for (int i = 0; i < 9; i++)
        {
            int state = chessBoard[i % 3, i / 3];
            Image image = imgs[i];
            image.gameObject.SetActive(state != NOPLAYER);
            image.sprite = state == P1 ? img_blue : img_red;
        }
    }

    // 点击-棋盘格子
    private void OnClickChess(int xIndex, int yIndex)
    {
        if (CheckWinner() != NOPLAYER)
        {
            OnRefresh();
            return;
        }
        if (gameTurn == NOPLAYER)
        {
            return;
        }
        if (gameTurn == P1)
        {
            // 存档
            JsonHelper.SaveJsonString(JsonHelper.GetJson());
        }
        int Player = chessBoard[xIndex, yIndex];
        switch (Player) {
            // 如果这个棋格中为NOPLAYER
            case NOPLAYER:
                // 选择游戏模式并点击棋格后，该棋格设置为这个游戏回合对应的玩家，游戏回合转换，总回合数加一
                chessBoard[xIndex, yIndex] = gameTurn;
                gameTurn = (gameTurn == P1 ? P2 : P1);
                totalMoves++;
                // 如果是单人游戏模式且游戏还未结束，那么电脑直接来走一步
                if (mode && totalMoves < 9 && CheckWinner() == NOPLAYER)
                {
                    AIMove();
                }
                OnRefresh();
                break;
            // 如果这个棋格中为PLAYER1，无反应
            case P1:
                break;
            // 如果这个棋格中为PLAYER2，无反应
            case P2:
                break;
        }
    }
    // 点击-切换模式
    public void OnSetMode()
    {
        Init(!mode, false);
        if (mode && gameTurn == P2)
        {
            AIMove();
            OnRefresh();
        }
    }
    // 点击-重置
    public void OnReset()
    {
        Init(mode, true);
    }
    // 点击-撤回
    public void OnLoadState()
    {
        string jsonData = JsonHelper.GetJsonString();
        if (jsonData == "") return;
        JsonData data = JsonMapper.ToObject(jsonData);
        MapData map = JsonMapper.ToObject<MapData>(data["Map"].ToString());
        mode = map.mode;
        totalMoves = map.totalMoves;
        gameTurn = P1;   // 因为1时存档，所以固定为1
        for (int k = 0; k < 3; k++)
        {
            for (int j = 0; j < 3; j++)
            {
                chessBoard[k, j] = map.chessBoard[k * 3 + j];
            }
        }
        OnRefresh();
    }

    // 根据是否有赢家获得提示框中的内容
    private string GetTipText()
    {
        switch (CheckWinner()) {
            // 如果没有赢家
            case NOPLAYER:
                if (totalMoves == 0)
                {
                    return "Click on the grid to start";
                }
                else if (totalMoves == 9)
                {
                    return "No winner";
                }
                else
                {
                    if (mode)
                    {
                        return "Single player mode";
                    }
                    else
                    {
                        return $"Two player mode, Players{gameTurn}";
                    }
                }
            case P1:
                return "Player 1 wins";
            case P2:
                return "Player 2 wins";
            default:
                return "";
        }
    }
}
