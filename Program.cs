using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
                int defence = int.Parse(inputs[6]);
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
                    defence,
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

public enum GameMode
{
    Draft,
    Battle
}

public enum CardAbility
{
    Breakthrough = 'B',
    Charge = 'C',
    Guard = 'G',
    Drain = 'D',
    Lethal = 'L',
    Ward = 'W'
}
public enum CardLocation
{
    PlayerHand = 0,
    PlayerField = 1,
    OpponentField = -1
}

public enum CardType
{
    Creature = 0,
    GreenItem = 1,  // My Creatures
    RedItem = 2,    // Opponent Creatures
    BlueItem = 3    // Targetless (-1)
}

class State
{
    public int Turn { get; set; }
    public GameMode GameMode { get {
        return Turn > 30 ? GameMode.Battle : GameMode.Draft;
    }}
    public Deck MyDeck { get; set;} = new Deck();
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
    private Stopwatch _stopwatch = new Stopwatch();

    public GameFactoryBuilder(State state)
    {
        _state = state;
        _stopwatch.Start();
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
        int defence,
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
            defence,
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
            var draft = new Draft(_gameDeck, _state, _stopwatch);
            action = draft.Command();
        } 
        else if (_state.GameMode == GameMode.Battle)
        {
            var battle = new Battle(_playerMana, _gameDeck, _stopwatch);
            action = battle.Command();
        }

        return action;
    }
}

class GameCard : IEquatable<GameCard>
{
    public int CardNumber { get; set; }
    public int InstanceId { get; set; }
    public CardLocation Location { get; set; }
    public CardType CardType { get; set; }
    public int Cost { get; set; }
    public int Attack { get; set; }
    public int Defence { get; set; }
    public IEnumerable<CardAbility> Abilities { get; set; }
    public int MyHealthChange { get; set; }
    public int OpponentHealthChange { get; set; }
    public int CardDraw { get; set; }

    public GameCard(
        int cardNumber,
        int instanceId,
        int location,
        int cardType,
        int cost,
        int attack,
        int defence,
        string abilities,
        int myHealthChange,
        int opponentHealthChange,
        int cardDraw
    ) {
        CardNumber = cardNumber;
        InstanceId = instanceId;
        Location = (CardLocation) location;
        CardType = (CardType) cardType;
        Cost = cost;
        Attack = attack;
        Defence = defence;
        Abilities = abilities.Replace("-", "").Select(c => (CardAbility) c);
        MyHealthChange = myHealthChange;
        OpponentHealthChange = opponentHealthChange;
        CardDraw = cardDraw;
    }

    public bool Equals(GameCard other)
    {

        return (InstanceId == -1)
            ? CardNumber == other.CardNumber
            : InstanceId == other.InstanceId;
    }
}

class Draft
{
    private const int DECK_SIZE = 30;
    private const int ABILITY_VALUE = 3;
    private const int CARD_WEIGHTING = 150;
    private Dictionary<int, int> _deckTarget = new Dictionary<int, int> {
        {1, 5},
        {2, 6},
        {3, 7},
        {4, 5},
        {5, 2},
        {6, 1},
        {7, 1}
    };
    private List<GameCard> _cards;
    private State _state;
    private Stopwatch _stopwatch;


    public Draft(List<GameCard> cards, State state, Stopwatch stopwatch)
    {
        _cards = cards;
        _state = state;
        _stopwatch = stopwatch;
    }

    public int Choice()
    {
        var cardSelection = _cards
            .OrderByDescending(g => CardValue(g))
            .First();

        var result = _cards
            .IndexOf(cardSelection);

        _state.MyDeck.AddCard(cardSelection);

        return result;
    }

    public string Command()
    {
        var draftPick = Choice();
        var result = $"PICK {draftPick}";

        return result;
    }

    private double CardValue(GameCard card)
    {
        var cardCost = card.Cost;
        if (cardCost < 1) cardCost = 1;
        if (cardCost > 7) cardCost = 7;

        var cardValue = 
            // The cost curve push
            ((double) (_deckTarget[cardCost] - _state.MyDeck.CostCurve[cardCost]) / (double) DECK_SIZE) * CARD_WEIGHTING +
            // Abilities adjustment
            (double) card.Abilities.Count() * (double) ABILITY_VALUE +
            // Fighting abilities
            (double) card.Attack + (double) card.Defence;

        return cardValue;
    }
}

class Battle
{
    private const int SUMMON_LIMIT = 6;
    private int _mana;
    private List<GameCard> _gameDeck;
    private Stopwatch _stopwatch;
    private List<GameCard> _opponentCreatureCards;
    private List<GameCard> _summonableCards;
    private List<GameCard> _myCreatureCards;
    private int _summonSlots;
    private List<GameCard> _summonedCards = new List<GameCard>();

    public Battle(int mana, List<GameCard> gameDeck, Stopwatch stopwatch)
    {
        _mana = mana;
        _gameDeck = gameDeck;
        _stopwatch = stopwatch;
        _opponentCreatureCards = OpponentCreatureCards();
        _summonableCards = SummonableCards();
        _myCreatureCards = MyCreatureCards();
        _summonableCards = SummonableCards();
        _summonSlots = SUMMON_LIMIT - _myCreatureCards.Count;
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

    private List<GameCard> OpponentCreatureCards()
    {
        var result = _gameDeck
            .Where(c => c.Location == CardLocation.OpponentField)
            .ToList();

        return result;
    }

    private GameCard SummonCreatureChoice()
    {
        // Pick a summonable card (semi-random)
        var result = _summonableCards
            .Where(c => c.Cost <= _mana &&
                c.CardType == CardType.Creature)        
            .FirstOrDefault();
        _summonableCards.Remove(result);
        
        // record the spend
        _mana -= result?.Cost ?? 0;

        return result;
    }

    private (GameCard ItemCard, int Target) SummonItemChoice()
    {
        var result = ((GameCard) null, 0);
        do
        {
            // Pick a no creature item card
            result = (_summonableCards
                .Where(c => c.Cost <= _mana &&
                    c.CardType == CardType.BlueItem)        
                .FirstOrDefault(), -1);
            _summonableCards.Remove(result.Item1);
            if (result.Item1 != null) break;

            // Target an opponent creature if one exists
            var creature = _opponentCreatureCards.FirstOrDefault();
            if (creature != null) {
            result = (_summonableCards
                .Where(c => c.Cost <= _mana &&
                    c.CardType == CardType.RedItem)        
                .FirstOrDefault(), creature.InstanceId);
                if (result.Item1 != null)
                {
                    _summonableCards.Remove(result.Item1);
                    creature.Attack += result.Item1.Attack;
                    creature.Defence += result.Item1.Defence;
                    if (creature.Defence < 1) _opponentCreatureCards.Remove(creature);
                    break;
                }
            }

            // Target my creature in play
            creature = _myCreatureCards.FirstOrDefault();
            if (creature != null) {
            result = (_summonableCards
                .Where(c => c.Cost <= _mana &&
                    c.CardType == CardType.GreenItem)        
                .FirstOrDefault(), creature.InstanceId);
                if (result.Item1 != null)
                {
                    _summonableCards.Remove(result.Item1);
                    creature.Attack += result.Item1.Attack;
                    creature.Defence += result.Item1.Defence;
                    break;
                }
            }

            // Target my creature summoning sickness
            creature = _summonedCards.FirstOrDefault();
            if (creature != null) {
            result = (_summonableCards
                .Where(c => c.Cost <= _mana &&
                    c.CardType == CardType.GreenItem)        
                .FirstOrDefault(), creature.InstanceId);
                if (result.Item1 != null)
                {
                    _summonableCards.Remove(result.Item1);
                    creature.Attack += result.Item1.Attack;
                    creature.Defence += result.Item1.Defence;
                    break;
                }
            }

        } while (false);

        // record the spend
        _mana -= result.Item1?.Cost ?? 0;

        return result;
    }

    private List<string> Summon()
    {
        var result = new List<string>();

        for (GameCard summonCard; (summonCard = SummonCreatureChoice()) != null && _summonSlots > 0; )
        {
            var summonOutput = $"SUMMON {summonCard.InstanceId}";
            _summonSlots--;

            if (summonCard.Abilities.Any(a => a == CardAbility.Charge))
            {
                // If summon has charge then record it as in the playfield
                _myCreatureCards.Add(summonCard);
            } else {
                _summonedCards.Add(summonCard);
            }

            result.Add(summonOutput);
        }

        for ((GameCard ItemCard, int Target) itemDetail; (itemDetail = SummonItemChoice()).ItemCard != null; )
        {
            var summonOutput = $"USE {itemDetail.ItemCard.InstanceId} {itemDetail.Target}";

            result.Add(summonOutput);
        }

        return result;
    }

    private (int attacker, int target) AttackChoice(GameCard card)
    {
        var targetGuard = _opponentCreatureCards
            .Where(c => c.Abilities.Any(c2 => c2 == CardAbility.Guard)
                && c.Defence > 0)
            .FirstOrDefault();
        var targetId = -1;

        var attackDetail = (card.InstanceId, targetId);

        return attackDetail;
    }

    private List<string> AttackDefenders(
        List<GameCard> myCreatures, 
        List<GameCard> targetCreatures
    ) {
        var targetGuards = targetCreatures
            .Where(c => c.Abilities.Any(c2 => c2 == CardAbility.Guard)
                && c.Defence > 0);
        
        foreach(var targetGuard in targetGuards)
        {

        }
        throw new NotImplementedException();
    }

    private List<string> Attack()
    {
        var result = new List<string>();

        var cards = _myCreatureCards
            .Where(c => c.Attack > 0).ToList();
        var battleTeams = GetAttackCombos();

        result.AddRange(RemoveDefenders(
            battleTeams,
            cards));

        foreach(var card in cards)
        {
            var detail = AttackChoice(card);
            var attackOutput = $"ATTACK {detail.attacker} {detail.target}";

            result.Add(attackOutput);
        }

        return result;
    }

    // This is potentially a very slow routine, but with a 6 card limit I think it is worth a try
    private Dictionary<int, List<BattleTeam>> GetAttackCombos()
    {
        var result = new Dictionary<int, List<BattleTeam>>();
        var combos = Combinations.GetAllCombos<GameCard>(_myCreatureCards);
        foreach(var combo in combos)
        {
            var battleTeam = new BattleTeam(
                combo,
                int.Parse(combo.Sum(c => c.Attack).ToString()),
                int.Parse(combo.Sum(c => c.Defence).ToString())
            );
            if (!result.ContainsKey(battleTeam.AttackStrength))
            {
                result.Add(battleTeam.AttackStrength, new List<BattleTeam>());
            }
            result[battleTeam.AttackStrength].Add(battleTeam);
        }

        return result;
    }

    private List<string> RemoveDefenders(
        Dictionary<int, List<BattleTeam>> battleTeams,
        List<GameCard> attackers
    )
    {
        var guards = _opponentCreatureCards
            .Where(c => c.Abilities.Contains(CardAbility.Guard));
        var unbeatenGuard = false;
        var result = new List<string>();

        foreach (var guard in guards)
        {
            var lowestPowerWin = battleTeams.Keys
                .Cast<int?>()
                .Where(k => k >= guard.Defence)
                .OrderBy(k => k)
                .FirstOrDefault();
            if (lowestPowerWin == null)
            {
                unbeatenGuard = true;
                continue;
            }

            // Just grab the first, don't worry about most appropriate
            var battleTeam =  battleTeams[(int) lowestPowerWin][0];

            // scrub the team members from combos and attackers
            foreach (var member in battleTeam.Team)
            {
                attackers.RemoveAll(c => c.Equals(member));
                var emptyKeys = new List<int>();
                foreach(var checkTeam in battleTeams)
                {
                    // 3 deep loop, wtf, needs sorting
                    checkTeam.Value.RemoveAll(bt => bt.Team.Any(gc => gc.Equals(member)));
                    if (checkTeam.Value.Count == 0)
                    {
                        emptyKeys.Add(checkTeam.Key);
                    }
                }
                foreach(var btKey in emptyKeys)
                {
                    battleTeams.Remove(btKey);
                }
                var attackOutput = $"ATTACK {member.InstanceId} {guard.InstanceId}";
                result.Add(attackOutput);
            }
        }

        if (unbeatenGuard)
        {
            attackers.Clear();
        }

        return result;
    }

    public string Command()
    {
        var actions = new List<string>();

        actions.AddRange(Summon());
        actions.AddRange(Attack());

        var result = actions.Count > 0
            ? string.Join(";", actions)
            : "PASS";

        return result;
    }
}

class BattleTeam
{
    public IEnumerable<GameCard> Team { get; set; }
    public int AttackStrength { get; set; }
    public int DefenceStrength { get; set; }

    public BattleTeam(
        IEnumerable<GameCard> team,
        int attackStrength,
        int defenceStrength
    ) {
        Team = team;
        AttackStrength = attackStrength;
        DefenceStrength = defenceStrength;
    }
}

class Deck
{
    public List<GameCard> Cards = new List<GameCard>();
    public Dictionary<int, int> CostCurve = new Dictionary<int, int> {
        {1, 0},
        {2, 0},
        {3, 0},
        {4, 0},
        {5, 0},
        {6, 0},
        {7, 0}
    };
    public Dictionary<CardAbility, int> AbilityCurve = new Dictionary<CardAbility, int> {
        {CardAbility.Breakthrough, 0},
        {CardAbility.Charge, 0},
        {CardAbility.Drain, 0},
        {CardAbility.Guard, 0},
        {CardAbility.Lethal, 0},
        {CardAbility.Ward, 0}
    };
    public Dictionary<CardType, int> TypeCurve = new Dictionary<CardType, int> {
        {CardType.BlueItem, 0},
        {CardType.Creature, 0},
        {CardType.GreenItem, 0},
        {CardType.RedItem, 0}
    };

    public void AddCard(GameCard card)
    {
        var cardCost = card.Cost;
        if (cardCost < 1) cardCost = 1;
        if (cardCost > 7) cardCost = 7;

        Cards.Add(card);
        CostCurve[cardCost]++;
        foreach(var ability in card.Abilities)
        {
            AbilityCurve[ability]++;
        }
        TypeCurve[card.CardType]++;
    }
}

// All credit to this article:
// https://www.geeksforgeeks.org/print-all-possible-combinations-of-r-elements-in-a-given-array-of-size-n/
// Which helped me understand this lifted solution:
// https://stackoverflow.com/questions/7802822/all-possible-combinations-of-a-list-of-values
public static class Combinations
{
    public static List<List<T>> GetAllCombos<T>(List<T> list)
    {
    int comboCount = (int) Math.Pow(2, list.Count) - 1;
    List<List<T>> result = new List<List<T>>();
    for (int i = 1; i < comboCount + 1; i++)
    {
        // make each combo here
        result.Add(new List<T>());
        for (int j = 0; j < list.Count; j++)
        {
            if ((i >> j) % 2 != 0)
                result.Last().Add(list[j]);
        }
    }
    return result;
    }
}

public static class Tools
{
    public static void Log(string logMessage)
    {
        Console.Error.WriteLine(logMessage);
    }   
}