// The Game Events used across the Game.
// Anytime there is a need for a new event, it should be added here.

using UnityEngine;

public static class GameEventsHandler
{
    public static readonly GameInitializeEvent GameInitializeEvent = new GameInitializeEvent();
    public static readonly GameStartEvent GameStartEvent = new GameStartEvent();
    public static readonly GameOverEvent GameOverEvent = new GameOverEvent();
    public static readonly ChangeLevelNumberEvent ChangeLevelNumberEvent = new ChangeLevelNumberEvent();
    public static readonly ChangeCoinsCountEvent ChangeCoinsCountEvent = new ChangeCoinsCountEvent();
    public static readonly ItemDestoyedEvent ItemDestoyedEvent = new ItemDestoyedEvent();
    public static readonly StageChangeEvent StageChangeEvent = new StageChangeEvent();
    public static readonly SpecialItemCreateEvent SpecialItemCreateEvent = new SpecialItemCreateEvent();
    public static readonly ItemsFallEvent ItemsFallEvent = new ItemsFallEvent();
    public static readonly DebugEvent DebugEvent = new DebugEvent();
    public static readonly TurnMadeEvent TurnMadeEvent = new TurnMadeEvent();
}

public class GameEvent {}

public class GameStartEvent : GameEvent
{
    //public int LevelNumber;
}

public class GameOverEvent : GameEvent
{
    public bool IsWin;
}

public class ChangeLevelNumberEvent : GameEvent
{
    public int CurrentLevelNumber;
}

public class ChangeCoinsCountEvent : GameEvent
{
    public int TotalValue;
    public int LevelValue;
}

public class ItemDestoyedEvent : GameEvent
{
    public ItemColor Color;
}

public class StageChangeEvent : GameEvent
{
    public int Stage;
}

public class TurnMadeEvent : GameEvent
{
}

public class ItemsFallEvent : GameEvent
{
    public bool IsFalling;
}

public class GameInitializeEvent : GameEvent
{
    public Sprite[] ActiveSprites;
}

public class DebugEvent : GameEvent
{
    public int MaxTurns;
    public int Target;
}

public class SpecialItemCreateEvent : GameEvent
{
    
}

