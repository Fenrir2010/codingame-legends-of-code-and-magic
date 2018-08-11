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
        var state = new State();

        // game loop
        while (true)
        {
            var game = new GameFactoryBuilder(state);
            for (int i = 0; i < 2; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int playerHealth = int.Parse(inputs[0]);
                int playerMana = int.Parse(inputs[1]);
                int playerDeck = int.Parse(inputs[2]);
                int playerRune = int.Parse(inputs[3]);
                game.AddDetail(
                    playerHealth,
                    playerMana,
                    playerDeck,
                    playerRune
                );
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

                game.AddCard(
                    cardNumber,
                    instanceId,
                    location,
                    cardType,
                    cost,
                    attack,
                    defense,
                    abilities,
                    myHealthChange,
                    opponentHealthChange,
                    cardDraw
                );
            }

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            Console.WriteLine(game.Run());
        }
    }
}

enum GameMode
{
    Draft,
    Battle
}

enum CardLocation
{
    PlayerHand = 0,
    PlayerField = 1,
    OpponentField = 2
}

class State
{
    public int Turn { get; set; }
    public GameMode GameMode { get {
        return Turn > 30 ? GameMode.Battle : GameMode.Draft;
    }}  
}

class GameFactoryBuilder
{
    private State _state;
    private int _playerHealth;
    private int _playerMana;
    private int _playerDeck;
    private int _playerRune;
    private int _opponentHealth;
    private int _opponentMana;
    private int _opponentDeck;
    private int _opponentRune;
    private List<GameCard> _gameDeck = new List<GameCard>();

    public GameFactoryBuilder(State state)
    {
        _state = state;
    }

    public void AddDetail(
        int Health,
        int Mana,
        int Deck,
        int Rune
    ) {
        if (_playerHealth == 0)
        {
            _playerHealth = Health;
            _playerMana = Mana;
            _playerDeck = Deck;
            _playerRune = Rune;
        }
        else
        {
            _opponentHealth = Health;
            _opponentMana = Mana;
            _opponentDeck = Deck;
            _opponentRune = Rune;
        }
    }

    public void AddCard(
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
    ) {
        _gameDeck.Add(new GameCard(
            cardNumber,
            instanceId,
            location,
            cardType,
            cost,
            attack,
            defense,
            abilities,
            myHealthChange,
            opponentHealthChange,
            cardDraw
        ));
    }

    public string Run()
    {
        _state.Turn++;

        string action = "";

        if (_state.GameMode == GameMode.Draft)
        {
            var draft = new Draft(_gameDeck);
            action = draft.Command();
        } 
        else if (_state.GameMode == GameMode.Battle)
        {
            var battle = new Battle(_playerMana, _gameDeck);
            action = battle.Command();
        }

        return action;
    }
}

class GameCard
{
    public int CardNumber { get; }
    public int InstanceId { get; }
    public CardLocation Location { get; }
    public int CardType { get; }
    public int Cost { get; }
    public int Attack { get; }
    public int Defense { get; }
    public string Abilities { get; }
    public int MyHealthChange { get; }
    public int OpponentHealthChange { get; }
    public int CardDraw { get; }

    public GameCard(
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
    ) {
        CardNumber = cardNumber;
        InstanceId = instanceId;
        Location = (CardLocation) location;
        CardType = cardType;
        Cost = cost;
        Attack = attack;
        Defense = defense;
        Abilities = abilities;
        MyHealthChange = myHealthChange;
        OpponentHealthChange = opponentHealthChange;
        CardDraw = cardDraw;
    }
}

class Draft
{
    private List<GameCard> _cards;

    public Draft(List<GameCard> cards)
    {
        _cards = cards;
    }

    public int Choice()
    {
        var result = _cards
            .IndexOf(
                _cards
                .OrderBy(g => g.Cost)
                .First()
            );

        return result;
    }

    public string Command()
    {
        var draftPick = Choice();
        var result = $"PICK {draftPick}";

        return result;
    }
}

class Battle
{
    private int _mana;
    private List<GameCard> _gameDeck;

    public Battle(int mana, List<GameCard> gameDeck)
    {
        _mana = mana;
        _gameDeck = gameDeck;
    }

    private List<GameCard> SummonableCards()
    {
        var result = _gameDeck
            .Where(c => c.Location == CardLocation.PlayerHand)
            .ToList();

        return result;
    }

    private List<GameCard> MyCreatureCards()
    {
        var result = _gameDeck
            .Where(c => c.Location == CardLocation.PlayerField)
            .ToList();

        return result;
    }

    private int? SummonChoice()
    {   
        var cards = SummonableCards();

        // Pick a summonable card (semi-random)
        var result = cards.Where(c => c.Cost < _mana)
            .FirstOrDefault();
        // record the spend
        _mana -= result?.Cost ?? 0;

        return result?.InstanceId;
    }

    private List<string> Summon()
    {
        var result = new List<string>();

        for (int? id; (id = SummonChoice()) != null; )
        {
            var summonOutput = $"SUMMON {id}";

            result.Add(summonOutput);
        }

        return result;
    }

    private (int attacker, int target) AttackChoice(GameCard card)
    {
        var attackDetail = (card.InstanceId, -1);

        return attackDetail;
    }

    private List<string> Attack()
    {
        var result = new List<string>();

        var cards = MyCreatureCards();

        foreach(var card in cards)
        {
            var detail = AttackChoice(card);
            var attackOutput = $"ATTACK {detail.attacker} {detail.target}";

            result.Add(attackOutput);
        }

        return result;
    }

    public string Command()
    {
        var actions = new List<string>();

        actions.AddRange(Summon());
        actions.AddRange(Attack());

        var result = actions.Count() > 0
            ? string.Join(";", actions)
            : "PASS";

        return result;
    }
}