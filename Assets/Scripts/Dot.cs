using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour {

    [Header("Board Variables")]
    public int columna;
    public int fila;
    public int columnaAnterior;
    public int filaAnterior;
    public int targetX;
    public int targetY;
    public bool isMatched = false;

    private FindMatches findMatches;
    private Board board;
    public GameObject otroDot;
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private Vector2 tempPosicion;

    [Header ("header stuff")]
    public float swipeAngle = 0;
    public float swipeResist = 1f;

    [Header("Powerup stuff")]
    public bool isColumnBomb;
    public bool isRowBomb;
    public GameObject columnArrow;
    public GameObject rowArrow;



    // Use this for initialization
    void Start () {

        isColumnBomb = false;
        isRowBomb = false;

        findMatches = FindObjectOfType<FindMatches>();
        board = FindObjectOfType<Board>();
        //targetX = (int)transform.position.x;
        //targetY = (int)transform.position.y;
        //columna = targetX;
        //fila = targetY;
        //columnaAnterior = columna;
        //filaAnterior = fila;
    }
	
	// Update is called once per frame
	void Update () {

       /*if (isMatched)
        {
            SpriteRenderer mySprite = GetComponent<SpriteRenderer>();
            mySprite.color = new Color(0f, 0f, 0f, 2f);
        }*/

        targetX = columna;
        targetY = fila;
        if (Mathf.Abs(targetX - transform.position.x) > .1)
        {//mover hacia
            tempPosicion = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosicion, .6f);
            if(board.allDots[columna,fila]!= this.gameObject)
            {
                board.allDots[columna, fila] = this.gameObject;
            }
            findMatches.FindAllMatches();
        }
        else
        {
            tempPosicion = new Vector2(targetX, transform.position.y);
            transform.position = tempPosicion;
        }
        if (Mathf.Abs(targetY - transform.position.y) > .1)
        {//mover hacia
            tempPosicion = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPosicion, .6f);
            if (board.allDots[columna, fila] != this.gameObject)
            {
                board.allDots[columna, fila] = this.gameObject;
            }
            findMatches.FindAllMatches();
        }
        else
        {
            tempPosicion = new Vector2(transform.position.x, targetY);
            transform.position = tempPosicion;
        }
    }

    public IEnumerator CheckMoveCo()
    {
        yield return new WaitForSeconds(.5f);
        if (otroDot != null)
        {
            if (!isMatched && !otroDot.GetComponent<Dot>().isMatched)
            {
                otroDot.GetComponent<Dot>().fila = fila;
                otroDot.GetComponent<Dot>().columna = columna;
                fila = filaAnterior;
                columna = columnaAnterior;
                yield return new WaitForSeconds(.5f);
                board.currentDot = null;
                board.currentState = GameState.move;
            }
            else
            {
                board.DestroyMatches();
            }
            //otroDot = null;
        }
        
    }

    private void OnMouseDown()
    {
        if (board.currentState == GameState.move)
        {
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Debug.Log(firstTouchPosition);
        }

    }

    private void OnMouseUp()
    {
        if (board.currentState == GameState.move)
        {
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalcularAngulo();
        }
    }

    void CalcularAngulo()
    {
        if (Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y) > swipeResist || Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x) > swipeResist)
        {
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
            MoverPiezas();
            board.currentState = GameState.wait;
            board.currentDot = this;
        }
        else
        {
            board.currentState = GameState.move;
        }
    }

    void MoverPiezas()
    {
        if(swipeAngle > -45 && swipeAngle <= 45 && columna < board.ancho - 1)
        {
            //rightSwipe
            otroDot = board.allDots[columna + 1, fila];
            columnaAnterior = columna;
            filaAnterior = fila;
            otroDot.GetComponent<Dot>().columna -= 1;
            columna += 1;
        }
        else if (swipeAngle > 45 && swipeAngle <= 135 && fila < board.alto - 1)
        {
            //upSwipe
            otroDot = board.allDots[columna, fila + 1];
            columnaAnterior = columna;
            filaAnterior = fila;
            otroDot.GetComponent<Dot>().fila -= 1;
            fila += 1;
        }
        else if ((swipeAngle > 135 || swipeAngle <= -135) && columna > 0)
        {
            //leftSwipe
            otroDot = board.allDots[columna - 1, fila];
            columnaAnterior = columna;
            filaAnterior = fila;
            otroDot.GetComponent<Dot>().columna += 1;
            columna -= 1;
        }
        else if (swipeAngle < -45 && swipeAngle >= -135 && fila > 0)
        {
            //downSwipe
            otroDot = board.allDots[columna, fila - 1];
            columnaAnterior = columna;
            filaAnterior = fila;
            otroDot.GetComponent<Dot>().fila += 1;
            fila -= 1;
        }
        StartCoroutine(CheckMoveCo());
    }

    void FindMatches()
    {
        if(columna > 0 && columna < board.ancho - 1)
        {
            GameObject leftDot1 = board.allDots[columna - 1, fila];
            GameObject rightDot1 = board.allDots[columna + 1, fila];
            if (leftDot1 != null && rightDot1 != null)
            {
                if (leftDot1.tag == this.gameObject.tag && rightDot1.tag == this.gameObject.tag)
                {
                    leftDot1.GetComponent<Dot>().isMatched = true;
                    rightDot1.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }
        }
        if (fila > 0 && fila < board.alto - 1)
        {
            GameObject upDot1 = board.allDots[columna, fila + 1];
            GameObject downDot1 = board.allDots[columna, fila - 1];
            if (upDot1 != null && downDot1 != null)
            {
                if (upDot1.tag == this.gameObject.tag && downDot1.tag == this.gameObject.tag)
                {
                    upDot1.GetComponent<Dot>().isMatched = true;
                    downDot1.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }
        }
    }

    public void MakeRowBomb()
    {
        isRowBomb = true;
        GameObject arrow = Instantiate(rowArrow, transform.position, Quaternion.identity);
        arrow.transform.parent = this.transform;
    }

    public void MakeColumnBomb()
    {
        isColumnBomb = true;
        GameObject arrow = Instantiate(columnArrow, transform.position, Quaternion.identity);
        arrow.transform.parent = this.transform;
    }



}
