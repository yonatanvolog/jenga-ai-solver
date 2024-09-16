using System;
using UnityEngine;

public class CommandDispatcher : MonoBehaviour
{
    [SerializeField] private PythonListener pythonListener;

    public enum Command
    {
        StartGame
        // Add more commands as needed
    }

    void Start()
    {
        if (pythonListener == null)
        {
            pythonListener = FindObjectOfType<PythonListener>();
        }
    }

    public void SendCommand(Command command)
    {
        string commandString = ConvertCommandToString(command);
        pythonListener.SendData(commandString);
    }

    public void DispatchStartGame(int player1, int player2, int num_of_rounds)
    {
        string commandString = $"start {player1} {player2} {num_of_rounds}";
        Debug.Log("sending to python: " + commandString);
        pythonListener.SendData(commandString);
    }

    public void DispatchFinishedMove(int level, string color)
    {
        string commandString = $"finished_move {level} {color}";
        Debug.Log("sending to python: " + commandString);
        pythonListener.SendData(commandString);
    }

    public void DispatchEndGame()
    {
        string commandString = "end_game";
        Debug.Log("sending to python: " + commandString);
        pythonListener.SendData(commandString);
    }

    private string ConvertCommandToString(Command command)
    {
        switch (command)
        {
            case Command.StartGame:
                return "StartGame";
            default:
                throw new ArgumentException("Invalid command");
        }
    }
}