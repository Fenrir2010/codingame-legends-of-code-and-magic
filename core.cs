using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    static void Main(string[] args)
    {
        string[] inputs;

        // game loop
        while (true)
        {
            for (int i = 0; i < 2; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int playerHealth = int.Parse(inputs[0]);
                int playerMana = int.Parse(inputs[1]);
                int playerDeck = int.Parse(inputs[2]);
                int playerRune = int.Parse(inputs[3]);
            }
            int opponentHand = int.Parse(Console.ReadLine());
            int cardCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < cardCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int cardNumber = int.Parse(inputs[0]);
                int instanceId = int.Parse(inputs[1]);
                int location = int.Parse(inputs[2]);
                int cardType = int.Parse(inputs[3]);
                int cost = int.Parse(inputs[4]);
                int attack = int.Parse(inputs[5]);
                int defense = int.Parse(inputs[6]);
                string abilities = inputs[7];
                int myHealthChange = int.Parse(inputs[8]);
                int opponentHealthChange = int.Parse(inputs[9]);
                int cardDraw = int.Parse(inputs[10]);
            }

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            Console.WriteLine("PASS");
        }
    }
}

class GameFactory
{
    public int _cardNumber { get; set; }
    public int _instanceId { get; set; }
    public int _location { get; set; }
    public int _cardType { get; set; }
    public int _cost { get; set; }
    public int _attack { get; set; }
    public int _defense { get; set; }
    public string _abilities { get; set; }
    public int _myHealthChange { get; set; }
    public int _opponentHealthChange { get; set; }
    public int _cardDraw { get; set; }

    public GameFactory(
        int cardNumber,
        int instanceId,
        int location,
        int cardType,
        int cost,
        int attack,
        int defense,
        string abilities,
        int myHealthChange,
        int opponentHealthChange,
        int cardDraw
    )
    {
        _cardNumber = cardNumber;
        _instanceId = instanceId;
        _location = location;
        _cardType = cardType;
        _cost = cost;
        _attack = attack;
        _defense = defense;
        _abilities = abilities;
        _myHealthChange = myHealthChange;
        _opponentHealthChange = opponentHealthChange;
        _cardDraw = cardDraw;
    }
}