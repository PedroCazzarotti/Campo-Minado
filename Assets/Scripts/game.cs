using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class game : MonoBehaviour
{
    public int width = 16;
    public int height = 16;
    public int mineCount = 32;

    private board board;
    private cell[,] state;
    private bool gameOver;

    public void OnValidate()
    {
        mineCount = Mathf.Clamp(mineCount, 0, width * height);
    }

    private void Awake()
    {
        board = GetComponentInChildren<board>();
    } 

    private void Start()
    {
        NewGame();
    }

    private void NewGame()
    {
        state = new cell[width, height];
        gameOver = false;

        GenerateCells();
        GenerateMines();
        GenerateNumbers();

        Camera.main.transform.position = new Vector3(width / 2f, height / 2f, -10f);
        board.Draw(state);
    }

    private void GenerateCells()
    {
        for(int x = 0; x < width; x++){
            for(int y = 0; y < height; y++){
                cell cell = new cell();
                cell.position = new Vector3Int(x, y, 0);
                cell.type = cell.Type.Empty;
                state[x, y] = cell;
            }
        }
    }

    private void GenerateMines()
    {
        for(int i = 0; i < mineCount; i++)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            while(state[x, y].type == cell.Type.Mine)
            {
                x++;

                if(x >= width){
                    x = 0;
                    y++;

                    if(y >= height){
                        y = 0;
                    }
                }
            }

            state[x, y].type = cell.Type.Mine;
            //state[x, y].revealed = true;//DEIXA AS MINAS REVELADAS
        }
    }

    private void GenerateNumbers()
    {
        for(int x = 0; x < width; x++){
            for(int y = 0; y < height; y++){
                cell cell = state[x, y];

                if(cell.type == cell.Type.Mine){
                    continue;
                }

                cell.number = CountMines(x, y);

                if(cell.number > 0){
                    cell.type = cell.Type.Number;
                }

                //cell.revealed = true;
                state[x, y] = cell;
            }
        }
    }

    private int CountMines(int cellX, int cellY)
    {
        int count = 0;

        for(int adjacentX = -1; adjacentX <= 1; adjacentX++)
        {
            for(int adjacentY = -1; adjacentY <= 1; adjacentY++)
            {
                if(adjacentX == 0 && adjacentY == 0){
                    continue;
                }

                int x = cellX + adjacentX;
                int y = cellY + adjacentY;

                if(GetCell(x, y).type == cell.Type.Mine){
                    count++;
                }
            }
        }

        return count;
    }

    private void Update()
    {

        if(Input.GetKeyDown(KeyCode.R)){
            NewGame();
        }

        else if(!gameOver)
        {
            if(Input.GetMouseButtonDown(1)){
                Flag();
            }else if(Input.GetMouseButtonDown(0)){
                Reveal();
            }
        }
        
    }

    private void Flag()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
        cell cell = GetCell(cellPosition.x, cellPosition.y);

        if(cell.type == cell.Type.Invalid || cell.revealed){
            return;
        }

        cell.flagged = !cell.flagged;
        state[cellPosition.x, cellPosition.y] = cell;
        board.Draw(state);
    }

    private void Reveal()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
        cell cell = GetCell(cellPosition.x, cellPosition.y);

        if(cell.type == cell.Type.Invalid || cell.revealed || cell.flagged){
            return;
        }

        switch (cell.type)
        {
            case cell.Type.Mine:
                Explode(cell);
                break;

            case cell.Type.Empty:
                Flood(cell);
                CheckWinCondition();
                break;

            default:
                cell.revealed = true;
                state[cellPosition.x, cellPosition.y] = cell;
                CheckWinCondition();
                break;
        }

        board.Draw(state);
    }

    private void Flood(cell cell)
    {
        if(cell.revealed) return;
        if(cell.type == cell.Type.Mine || cell.type == cell.Type.Invalid) return;

        cell.revealed = true;
        state[cell.position.x, cell.position.y] = cell;

        if(cell.type == cell.Type.Empty){
            Flood(GetCell(cell.position.x - 1, cell.position.y));
            Flood(GetCell(cell.position.x + 1, cell.position.y));
            Flood(GetCell(cell.position.x, cell.position.y - 1));
            Flood(GetCell(cell.position.x, cell.position.y + 1));
        }
    }

    private void Explode(cell cell)
    {
        Debug.Log("game over");
        gameOver = true;

        cell.revealed = true;
        cell.exploded = true;
        state[cell.position.x, cell.position.y] = cell;

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                cell = state[x, y];

                if(cell.type == cell.Type.Mine){
                    cell.revealed = true;
                    state[x, y] = cell;
                }
            }
        }
    }

    private void CheckWinCondition()
    {
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                cell cell = state[x, y];

                if(cell.type != cell.Type.Mine && !cell.revealed){
                    return;
                }
            }
        }

        Debug.Log("Winner!");
        gameOver = true;

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                cell cell = state[x, y];

                if(cell.type == cell.Type.Mine){
                    cell.flagged = true;
                    state[x, y] = cell;
                }
            }
        }
    }

    private cell GetCell(int x, int y)
    {
        if(isValid(x, y)){
            return state[x, y];
        }else{
            return new cell(); 
        }
    }

    private bool isValid(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

}
