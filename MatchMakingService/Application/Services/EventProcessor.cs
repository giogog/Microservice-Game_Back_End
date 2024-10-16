using Match.Contracts;
using Match.Domain.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.Json;
using static Match.Application.Services.EventProcessor;

namespace Match.Application.Services;

public class EventProcessor : IEventProcessor
{
    // A set of all available game modes and their properties to avoid repeated database calls.
    // If a new game mode is added, an event will be caught and the new mode will be added to this set.
    private HashSet<GamemodeDto> _gamemodes = new();

    // Player queues organized by region. Each region key maps to a dictionary where the key is a tuple (PlayerId, PlayerNickName),
    // and the value is the game mode that the player wants to play. This prevents fetching regions from the database frequently.
    // When players join or leave matchmaking, corresponding events update the relevant player data in these queues.
    private ConcurrentDictionary<string, ConcurrentDictionary<string, List<Player>>> PlayersQueue = new();

    private readonly ILogger<EventProcessor> _logger;

    //Services 
    private readonly IGamemodeService _gamemodeService;
      
    public EventProcessor(ILogger<EventProcessor> logger, IGamemodeService gamemodeService)
    {
        _logger = logger;
        _gamemodeService = gamemodeService;
        SetGamemodes();
  
    }

  
    // Processes different types of events such as joining or leaving matchmaking, or creating new regions and game modes.
    public GameCreated? ProcessMatchMakingEvent(string @event)
    {
        var eventType = GetEventType(@event);

        if(eventType == EventType.JoinedMatchMaking)
        {
            var joinQueueEventDto = JsonSerializer.Deserialize<JoinedMatchMakingDto>(@event);

            var newPlayer = new Player(joinQueueEventDto.PlayerId, joinQueueEventDto.PlayerName, joinQueueEventDto.Lvl);

            //var players = PlayersQueue.Where(q=>q.Key == joinQueueEventDto?.Region)?
            //    .Select(v=>v.Value)?
            //    .FirstOrDefault();
            
            if (!PlayersQueue.TryGetValue(joinQueueEventDto.Region, out var PlayersOnRegion))
            {
                PlayersOnRegion = new ConcurrentDictionary<string, List<Player>>
                {
                    [joinQueueEventDto.GameMode] = new List<Player> { newPlayer }
                };

                PlayersQueue.TryAdd(joinQueueEventDto.Region, PlayersOnRegion);

            }
            
            if (!PlayersOnRegion.TryGetValue(joinQueueEventDto.GameMode, out var playersOnGameMode))
            {
                playersOnGameMode = new List<Player>();
            }

            if (!playersOnGameMode.Any(p => p.Id == joinQueueEventDto.PlayerId))
            {
                 playersOnGameMode.Add(newPlayer);

            }
            

            PlayersOnRegion.TryAdd(joinQueueEventDto.GameMode, playersOnGameMode);

            var maxPlayers = _gamemodes.FirstOrDefault(g => g.Name == joinQueueEventDto.GameMode);

            if (maxPlayers == null)
            {
                _logger.LogError("Gamemode not Found");
                return null;
            }



            if (playersOnGameMode.Count() == maxPlayers.Maxplayers)
            {
                _logger.LogInformation("Match found. Creating game.");
                var data = playersOnGameMode.ToList();
                playersOnGameMode.Clear();
                var gameCreated = new GameCreated(joinQueueEventDto.GameMode, maxPlayers.Maxscore, joinQueueEventDto.Region, data);
                return gameCreated;
            }



        }
        else if (eventType == EventType.LeftMatchMaking)
        {
            var leftQueueEventDto = JsonSerializer.Deserialize<LeftMatchMakingDto>(@event);
            if (leftQueueEventDto == null) return null;

            // Remove the player from the queue when they leave matchmaking.
            if (PlayersQueue.TryGetValue(leftQueueEventDto.Region, out var playersongameMode))
            {
                if (playersongameMode.TryGetValue(leftQueueEventDto.GameMode, out var playerstoleft))
                {
                    playerstoleft.Remove(playerstoleft.FirstOrDefault(p => p.Id == leftQueueEventDto.PlayerId));
                }
            }
        }
        else if (eventType == EventType.RegionCreated)
        {
            var newRegionEventDto = JsonSerializer.Deserialize<RegionCreatedDto>(@event);
            if (newRegionEventDto == null) return null;

            // Add a new region if it is created.
            PlayersQueue.TryAdd(newRegionEventDto.Name, new ConcurrentDictionary<string, List<Player>>());
        }
        else if (eventType == EventType.GamemodeCreated)
        {
            var newGamemodeEventDto = JsonSerializer.Deserialize<GamemodeCreatedDto>(@event);
            if (newGamemodeEventDto == null) return null;

            // Add the new game mode to the set of available game modes.
            _gamemodes.Add(new GamemodeDto(newGamemodeEventDto.Name, newGamemodeEventDto.MaxPlayers, newGamemodeEventDto.MaxScore));
        }

        return null;
    }
    

    // Load all game modes from the GameDomain service and store them in the HashSet.
    private void SetGamemodes()
    {
        var gamemodes = _gamemodeService.GetGamemodes();
        if (gamemodes == null)
        {
            _logger.LogError("Failed to retrieve gamemodes.");
        }
        else
        {
            _gamemodes = gamemodes.ToHashSet();
        }
    }

    // Helper method to determine the type of event from the serialized event data.
    private EventType GetEventType(string @event)
    {
        var eventDto = JsonSerializer.Deserialize<GenericEventDto>(@event);
        return eventDto?.Event switch
        {
            "JoinedMatchMaking" => EventType.JoinedMatchMaking,
            "LeftMatchMaking" => EventType.LeftMatchMaking,
            "RegionCreated" => EventType.RegionCreated,
            "GamemodeCreated" => EventType.GamemodeCreated,
            _ => EventType.Undetermined
        };
    }
}
