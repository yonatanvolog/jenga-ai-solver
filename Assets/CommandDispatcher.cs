using System;
using UnityEngine;

public class CommandDispatcher : MonoBehaviour
{
    public PythonListener pythonListener;

    // Enum for all the commands
    public enum Command
    {
        StartGame,
        // You can add more commands here as needed
    }

    void Start()
    {
        if (pythonListener == null)
        {
            pythonListener = GameObject.FindObjectOfType<PythonListener>();
        }
    }

    // Method to send a command (generic)
    public void SendCommand(Command command)
    {
        string commandString = ConvertCommandToString(command);
        pythonListener.SendData(commandString);
    }

    // Method to send the StartGame command with player1 and player2 params
    public void DispatchStartGame(int player1, int player2, int num_of_rounds)
    {

        string commandString = $"start {player1} {player2} {num_of_rounds}";
        Debug.Log("sending to python: " + commandString);

        pythonListener.SendData(commandString);
    }
    
    public void DispatchFinishedMove()
    {
        string commandString = $"finished_move";
        Debug.Log("sending to python: " + commandString);

        pythonListener.SendData(commandString);
    }
    
    public void DispatchEndGame()
    {
        string commandString = $"end_game";
        Debug.Log("sending to python: " + commandString);

        pythonListener.SendData(commandString);
    }

    // Method to handle converting enum to the appropriate command string
    private string ConvertCommandToString(Command command)
    {
        switch (command)
        {
            case Command.StartGame:
                return "StartGame";
            // Handle other commands here
            default:
                throw new ArgumentException("Invalid command");
        }
    }
}