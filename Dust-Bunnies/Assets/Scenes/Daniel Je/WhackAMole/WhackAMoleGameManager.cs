using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class WhackAMoleGameManager : MonoBehaviour
{
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private float moleStayTime = 1.0f;
    [SerializeField] private TMP_Text scoreDisplay;
    private List<Mole> _moles = new List<Mole>();
    private int _score;
    

    private void Start()
    {
        foreach (Transform child in transform)
        {
            Mole mole = child.GetComponent<Mole>();
            if (mole != null)
            {
                mole.Initialize(this);
                _moles.Add(mole);
            }
        }

        StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval * Random.Range(0.2f, 1.8f));

            Mole mole = GetRandomDownMole();
            if (mole != null)
            {
                mole.Raise(moleStayTime);
            }
        }
    }

    private Mole GetRandomDownMole()
    {
        List<Mole> available = new List<Mole>();

        foreach (var mole in _moles)
        {
            if (!mole.GetIsUp())
                available.Add(mole);
        }

        if (available.Count == 0) return null;

        return available[Random.Range(0, available.Count)];
    }

    public void AddScore(int amount)
    {
        _score += amount;
        scoreDisplay.SetText("Score: " + _score);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Mole mole = hit.collider.GetComponent<Mole>();
                if (mole != null)
                {
                    mole.Hit();
                }
            }
        }
    }
}