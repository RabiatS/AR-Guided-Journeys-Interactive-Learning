using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Manages the Tic-Tac-Toe game logic including player turns, win detection, and simple AI.
/// </summary>
public class TicTacToeManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button[] gridButtons; // 9 buttons for the 3x3 grid
    [SerializeField] private Text statusText;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button closeButton;
    
    [Header("Game Settings")]
    [SerializeField] private bool playAgainstAI = true;
    [SerializeField] private Sprite xSprite;
    [SerializeField] private Sprite oSprite;
    
    [Header("Colors")]
    [SerializeField] private Color playerColor = Color.blue;
    [SerializeField] private Color aiColor = Color.red;
    [SerializeField] private Color winColor = Color.green;
    
    private int[] board = new int[9]; // 0 = empty, 1 = player (X), -1 = AI (O)
    private bool isPlayerTurn = true;
    private bool gameOver = false;
    
    private void Awake()
    {
        // Setup button listeners
        for (int i = 0; i < gridButtons.Length; i++)
        {
            int index = i; // Capture for lambda
            gridButtons[i].onClick.AddListener(() => OnGridButtonClicked(index));
        }
        
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetGame);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseGame);
        }
        
        ResetGame();
    }
    
    private void OnEnable()
    {
        // Reset game whenever the UI becomes active
        ResetGame();
    }
    
    private void OnGridButtonClicked(int index)
    {
        if (gameOver || board[index] != 0 || !isPlayerTurn)
            return;
        
        // Player makes a move
        MakeMove(index, 1);
        
        // Check for win or draw
        if (CheckWin(1))
        {
            EndGame("You Win! 🎉");
            return;
        }
        
        if (CheckDraw())
        {
            EndGame("It's a Draw!");
            return;
        }
        
        // AI turn
        if (playAgainstAI)
        {
            isPlayerTurn = false;
            statusText.text = "AI's Turn...";
            Invoke(nameof(AIMove), 0.5f); // Small delay for better UX
        }
        else
        {
            isPlayerTurn = !isPlayerTurn;
            statusText.text = isPlayerTurn ? "Player X's Turn" : "Player O's Turn";
        }
    }
    
    private void MakeMove(int index, int player)
    {
        board[index] = player;
        
        // Update button visual
        Image buttonImage = gridButtons[index].GetComponent<Image>();
        if (player == 1)
        {
            gridButtons[index].GetComponentInChildren<Text>().text = "X";
            buttonImage.color = playerColor;
        }
        else
        {
            gridButtons[index].GetComponentInChildren<Text>().text = "O";
            buttonImage.color = aiColor;
        }
        
        gridButtons[index].interactable = false;
    }
    
    private void AIMove()
    {
        // Simple AI: Try to win, then block, then take center, then random
        int move = GetBestMove();
        
        MakeMove(move, -1);
        
        if (CheckWin(-1))
        {
            EndGame("AI Wins!");
            return;
        }
        
        if (CheckDraw())
        {
            EndGame("It's a Draw!");
            return;
        }
        
        isPlayerTurn = true;
        statusText.text = "Your Turn";
    }
    
    private int GetBestMove()
    {
        // 1. Try to win
        int winMove = FindWinningMove(-1);
        if (winMove != -1) return winMove;
        
        // 2. Block player from winning
        int blockMove = FindWinningMove(1);
        if (blockMove != -1) return blockMove;
        
        // 3. Take center if available
        if (board[4] == 0) return 4;
        
        // 4. Take corners
        int[] corners = { 0, 2, 6, 8 };
        List<int> availableCorners = new List<int>();
        foreach (int corner in corners)
        {
            if (board[corner] == 0)
                availableCorners.Add(corner);
        }
        if (availableCorners.Count > 0)
            return availableCorners[Random.Range(0, availableCorners.Count)];
        
        // 5. Take any available spot
        List<int> availableMoves = new List<int>();
        for (int i = 0; i < 9; i++)
        {
            if (board[i] == 0)
                availableMoves.Add(i);
        }
        
        return availableMoves[Random.Range(0, availableMoves.Count)];
    }
    
    private int FindWinningMove(int player)
    {
        // Check all possible moves to find a winning one
        for (int i = 0; i < 9; i++)
        {
            if (board[i] == 0)
            {
                // Try this move
                board[i] = player;
                bool wins = CheckWin(player);
                board[i] = 0; // Undo move
                
                if (wins)
                    return i;
            }
        }
        return -1;
    }
    
    private bool CheckWin(int player)
    {
        // Check all winning combinations
        int[,] winPatterns = new int[,]
        {
            {0, 1, 2}, {3, 4, 5}, {6, 7, 8}, // Rows
            {0, 3, 6}, {1, 4, 7}, {2, 5, 8}, // Columns
            {0, 4, 8}, {2, 4, 6}             // Diagonals
        };
        
        for (int i = 0; i < winPatterns.GetLength(0); i++)
        {
            int a = winPatterns[i, 0];
            int b = winPatterns[i, 1];
            int c = winPatterns[i, 2];
            
            if (board[a] == player && board[b] == player && board[c] == player)
            {
                // Highlight winning combination
                HighlightWin(a, b, c);
                return true;
            }
        }
        
        return false;
    }
    
    private void HighlightWin(int a, int b, int c)
    {
        gridButtons[a].GetComponent<Image>().color = winColor;
        gridButtons[b].GetComponent<Image>().color = winColor;
        gridButtons[c].GetComponent<Image>().color = winColor;
    }
    
    private bool CheckDraw()
    {
        foreach (int cell in board)
        {
            if (cell == 0)
                return false;
        }
        return true;
    }
    
    private void EndGame(string message)
    {
        gameOver = true;
        statusText.text = message;
    }
    
    public void ResetGame()
    {
        // Clear board
        for (int i = 0; i < 9; i++)
        {
            board[i] = 0;
            gridButtons[i].GetComponentInChildren<Text>().text = "";
            gridButtons[i].GetComponent<Image>().color = Color.white;
            gridButtons[i].interactable = true;
        }
        
        isPlayerTurn = true;
        gameOver = false;
        statusText.text = "Your Turn";
    }
    
    private void CloseGame()
    {
        // Find the InteractiveCube and close the game
        InteractiveCube cube = FindObjectOfType<InteractiveCube>();
        if (cube != null)
        {
            cube.CloseMinigame();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}