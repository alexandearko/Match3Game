using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    wait,
    move
}

public class Board : MonoBehaviour {

    public GameState currentState = GameState.move;
    public int ancho;
    public int alto;
    public int offSet;
    public GameObject tilePrefab;
    public GameObject[] dots;
    public GameObject destroyEffect;
    private BackgroundTile[,] allTiles;
    public GameObject[,] allDots;
    private FindMatches findMatches;
    public Dot currentDot;

	// Use this for initialization
	void Start () {

        findMatches = FindObjectOfType<FindMatches>();
        allTiles = new BackgroundTile[ancho, alto]; //inicia el tablero de juego
        allDots = new GameObject[ancho, alto];
        SetUp();
		
	}
	
    private void SetUp()
    {
        for (int i = 0; i < ancho; i++)
        {
            for (int j = 0; j < alto; j++)
            {
                Vector2 tempPosicion = new Vector2(i, j + offSet);
                GameObject backgroundTile = Instantiate(tilePrefab, tempPosicion, Quaternion.identity) as GameObject;
                backgroundTile.transform.parent = this.transform;
                backgroundTile.name = "(" + i + "," + j + ")";
                int dotToUse = Random.Range(0, dots.Length);

                int maxIteraciones = 0;
                while(MatchesAt(i, j, dots[dotToUse]) && maxIteraciones < 100)
                {
                    dotToUse = Random.Range(0, dots.Length);
                    maxIteraciones++;
                }
                maxIteraciones = 0;

                GameObject dot = Instantiate(dots[dotToUse], tempPosicion, Quaternion.identity);
                dot.GetComponent<Dot>().fila = j;
                dot.GetComponent<Dot>().columna = i;

                dot.transform.parent = this.transform;
                dot.name = "(" + i + "," + j + ")";
                allDots[i, j] = dot;
            }
        }

    }

    private bool MatchesAt(int columna, int fila, GameObject pieza)
    {
        if(columna > 1 && fila > 1)
        {
            if(allDots[columna -1,fila].tag == pieza.tag && allDots[columna-2, fila].tag == pieza.tag)
            {
                return true;
            }
            if (allDots[columna, fila -1].tag == pieza.tag && allDots[columna, fila -2].tag == pieza.tag)
            {
                return true;
            }

        }
        else if (columna <= 1 || fila <= 1)
        {
            if(fila > 1)
            {
                if (allDots[columna, fila - 1].tag == pieza.tag && allDots[columna, fila - 2].tag == pieza.tag)
                {
                    return true;
                }
            }
            if (columna > 1)
            {
                if (allDots[columna - 1, fila].tag == pieza.tag && allDots[columna - 2, fila].tag == pieza.tag)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void DestroyMatchesAt(int columna, int fila)
    {
        if (allDots[columna, fila].GetComponent<Dot>().isMatched)
        {
            //cuantos elementos hay en el matched pieces list de findmatches?
            if(findMatches.currentMatches.Count == 4 || findMatches.currentMatches.Count == 7)
            {
                findMatches.CheckBombs();
            }
            findMatches.currentMatches.Remove(allDots[columna, fila]);
            GameObject particle = Instantiate(destroyEffect, allDots[columna,fila].transform.position, Quaternion.identity);
            Destroy(particle, .5f);
            Destroy(allDots[columna, fila]);
            allDots[columna, fila] = null;
        }
    }

    public void DestroyMatches()
    {
        for (int i = 0; i < ancho; i++)
        {
            for (int j = 0; j < alto; j++)
            {
                if (allDots[i,j] != null)
                {
                    DestroyMatchesAt(i, j);
                }
            }
        }

        StartCoroutine(DisminuyeFilaCo());
    }

    private IEnumerator DisminuyeFilaCo()
    {
        int nullCount = 0;
        for (int i = 0; i < ancho; i++)
        {
            for (int j = 0; j < alto; j++)
            {
                if(allDots[i,j] == null)
                {
                    nullCount++;
                }
                else if(nullCount > 0)
                {
                    allDots[i, j].GetComponent<Dot>().fila -= nullCount;
                    allDots[i, j] = null;
                }
            }
            nullCount = 0;
        }

        yield return new WaitForSeconds(.4f);

        StartCoroutine(FillBoardCo());
    }

    private void Rellenar()
    {
        for(int i = 0; i < ancho; i++)
        {
            for(int j = 0; j < alto; j++)
            {
                if(allDots[i,j] == null)
                {
                    Vector2 tempPosicion = new Vector2(i, j + offSet);
                    int dotToUse = Random.Range(0, dots.Length);
                    GameObject pieza = Instantiate(dots[dotToUse], tempPosicion, Quaternion.identity);
                    allDots[i, j] = pieza;
                    pieza.GetComponent<Dot>().fila = j;
                    pieza.GetComponent<Dot>().columna = i;
                }
            }
        }
    }

    private bool MatchesOnBoard()
    {
        for (int i = 0; i < ancho; i++)
        {
            for (int j = 0; j < alto; j++)
            {
                if (allDots[i, j] != null)
                {
                    if (allDots[i, j].GetComponent<Dot>().isMatched)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private IEnumerator FillBoardCo()
    {
        Rellenar();
        yield return new WaitForSeconds(.5f);

        while (MatchesOnBoard())
        {
            yield return new WaitForSeconds(.5f);
            DestroyMatches();
        }
        findMatches.currentMatches.Clear();
        currentDot = null;
        yield return new WaitForSeconds(.5f);
        currentState = GameState.move;
    }
}
