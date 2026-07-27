using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

// 씬 곳곳에 흩어진 런타임 상태를 읽어
// 로컬 저장용 스냅샷 하나로 조립하는 역할을 담당한다.
public class GameStateAssembler : MonoBehaviour
{
    private const int CurrentSaveSchemaVersion = 1;

    [SerializeField] private MiddleDB middleDB;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private TokenManager tokenManager;
    [SerializeField] private FarmLevelManager farmLevelManager;
    [SerializeField] private GoldManager goldManager;
    [SerializeField] private TileManager tileManager;
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private QuestManager questManager;

    // 버튼 테스트용 메서드.
    // 현재 스냅샷을 JSON 파일로 저장하고 요약 정보를 콘솔에 출력한다.
    public void DebugCreateSnapshot()
    {
        GameStateSnapshot snapshot = CreateSnapshot(GetDefaultUserId());
        string savedPath = SaveSnapshotJsonToDesktop(snapshot);

        Debug.Log(
            $"Snapshot created | userId: {snapshot.userId}, " +
            $"tiles: {snapshot.tiles.Length}, " +
            $"inventory: {snapshot.inventory.Length}, " +
            $"currentToken: {snapshot.currentToken} | savedPath: {savedPath}",
            this);
    }

    // 버튼 테스트용 메서드.
    // 서버 헬스체크를 호출해 백엔드 연결 상태를 바로 확인한다.
    public void DebugCheckHealth()
    {
        APIController.Health.Check(
            onSuccess: response =>
            {
                Debug.Log(
                    $"Health check success | status: {response.status}, database: {response.database}, time: {response.time}",
                    this);
            },
            onError: error =>
            {
                Debug.LogError($"[GameStateAssembler] Health check failed: {error}", this);
            });
    }

    // 버튼 테스트용 메서드.
    // 현재 게임 상태 스냅샷을 로컬 json 파일로 저장한다.
    public void DebugSendSnapshot()
    {
        GameStateSnapshot snapshot = CreateSnapshot(GetDefaultUserId());

        if (!TryValidateSnapshot(snapshot, out string validationError))
        {
            Debug.LogError($"[GameStateAssembler] Snapshot validation failed: {validationError}", this);
            return;
        }

        if (!LocalGameSaveRepository.TrySave(snapshot, out string savedPath, out string saveError))
        {
            Debug.LogError($"[GameStateAssembler] DebugSendSnapshot failed: {saveError}", this);
            return;
        }

        Debug.Log(
            $"Local snapshot saved | userId: {snapshot.userId}, " +
            $"tiles: {snapshot.tiles.Length}, " +
            $"inventory: {snapshot.inventory.Length}, " +
            $"currentToken: {snapshot.currentToken} | savedPath: {savedPath}",
            this);
    }

    private void Awake()
    {
        // 직접 연결되지 않았을 때를 대비해 필요한 매니저를 자동으로 찾는다.
        if (middleDB == null)
        {
            middleDB = FindFirstObjectByType<MiddleDB>();
        }

        if (inventoryManager == null)
        {
            inventoryManager = FindFirstObjectByType<InventoryManager>();
        }

        if (tokenManager == null)
        {
            tokenManager = TokenManager.Instance != null
                ? TokenManager.Instance
                : FindFirstObjectByType<TokenManager>();
        }

        if (farmLevelManager == null)
        {
            farmLevelManager = FindFirstObjectByType<FarmLevelManager>();
        }

        if (goldManager == null)
        {
            goldManager = FindFirstObjectByType<GoldManager>();
        }

        if (tileManager == null)
        {
            tileManager = FindFirstObjectByType<TileManager>();
        }

        if (characterManager == null)
        {
            characterManager = CharacterManager.Instance != null
                ? CharacterManager.Instance
                : FindFirstObjectByType<CharacterManager>();
        }

        if (questManager == null)
        {
            questManager = FindFirstObjectByType<QuestManager>(FindObjectsInactive.Include);
        }
    }

    public void OnClickSaveButton()
    {
        SaveData();
        AudioManager.Instance.PlaySFX(SfxType.Click);
    }

    public void SaveData()
    {
        string userId = GetDefaultUserId();
        GameStateSnapshot snapshot = CreateSnapshot(userId);

        if (!TryValidateSnapshot(snapshot, out string validationError))
        {
            Debug.LogError($"[GameStateAssembler] SaveData validation failed: {validationError}", this);
            return;
        }

        if (!LocalGameSaveRepository.TrySave(snapshot, out string savedPath, out string saveError))
        {
            Debug.LogError($"[GameStateAssembler] SaveData failed: {saveError}", this);
            return;
        }

        Debug.Log($"SaveData success | localPath: {savedPath}, savedAt: {snapshot.savedAt}", this);
    }

    public void GetData(Action onLoaded = null, Action onNewStart = null, Action<string> onFailed = null)
    {
        if (!LocalGameSaveRepository.TryLoad(out GameStateSnapshot snapshot, out string loadedPath, out string loadError))
        {
            if (!string.IsNullOrWhiteSpace(loadError))
            {
                string errorMessage = $"[GameStateAssembler] GetData failed: {loadError}";
                Debug.LogError(errorMessage, this);
                onFailed?.Invoke(errorMessage);
                return;
            }

            ApplyDefaultState(false);
            Debug.Log($"GetData result | hasSnapshot: false | localPath: {loadedPath}", this);
            onNewStart?.Invoke();
            return;
        }

        if (!TryValidateSnapshot(snapshot, out string validationError))
        {
            string errorMessage = $"[GameStateAssembler] GetData validation failed: {validationError}";
            Debug.LogError(errorMessage, this);
            onFailed?.Invoke(errorMessage);
            return;
        }

        ApplyLoadedSnapshot(snapshot);
        Debug.Log($"GetData success | userId: {snapshot.userId}, savedAt: {snapshot.savedAt}, localPath: {loadedPath}", this);
        onLoaded?.Invoke();
    }

    public void StartNewGame(int worldSeed)
    {
        if (middleDB != null)
        {
            middleDB.SetWorldSeed(worldSeed);
            middleDB.SetGuaranteedStartCoord(new Vector2Int(7, 4));
        }

        ApplyDefaultState(true);
    }

    // 현재 게임 상태를 저장하기 쉬운 형태의 스냅샷으로 묶는다.
    public GameStateSnapshot CreateSnapshot(string userId)
    {
        // 씬에 흩어진 런타임 상태를 저장 가능한 하나의 스냅샷으로 모은다.
        return new GameStateSnapshot
        {
            schemaVersion = CurrentSaveSchemaVersion,
            userId = userId,
            worldSeed = middleDB != null ? middleDB.WorldSeed : 0,
            savedAt = DateTime.UtcNow.ToString("o"),
            tiles = BuildTileDtos(),
            inventory = BuildInventoryDtos(),
            currentToken = BuildCurrentToken(),
            farmLevel = BuildFarmLevel(),
            farmNowExp = BuildFarmNowExp(),
            gold = BuildGold(),
            characterID = BuildCharacterID(),
            quest = BuildQuest()
        };
    }

    // MiddleDB에 들어 있는 전체 타일 상태를 저장용 DTO 배열로 변환한다.
    private TileStateDto[] BuildTileDtos()
    {
        if (middleDB == null)
        {
            Debug.LogWarning("[GameStateAssembler] MiddleDB reference is missing.", this);
            return Array.Empty<TileStateDto>();
        }

        middleDB.EnsureInitialized();

        List<TileStateDto> result = new List<TileStateDto>(middleDB.TileCount);

        for (int y = 0; y < middleDB.Height; y++)
        {
            for (int x = 0; x < middleDB.Width; x++)
            {
                Vector2Int coord = new Vector2Int(x, y);
                MiddleDB.TileState state = middleDB.GetTileState(coord);

                if (state == null)
                {
                    continue;
                }

                result.Add(new TileStateDto
                {
                    id = state.id,
                    tileType = state.tileType.ToString(),
                    cropType = state.cropType.ToString(),
                    cropState = state.cropState.ToString(),
                    variantIndex = state.variantIndex,
                    growDuration = state.growDuration,
                    maxTime = state.maxTime
                });
            }
        }

        return result.ToArray();
    }

    // 인벤토리 슬롯 중 실제 아이템이 들어 있는 슬롯만 추려서 DTO로 변환한다.
    private InventoryItemDto[] BuildInventoryDtos()
    {
        // 빈 슬롯은 제외하고 실제 보유 아이템만 저장한다.
        if (inventoryManager == null)
        {
            Debug.LogWarning("[GameStateAssembler] InventoryManager reference is missing.", this);
            return Array.Empty<InventoryItemDto>();
        }

        List<InventoryItemDto> result = new List<InventoryItemDto>();

        foreach (InventorySlot slot in inventoryManager.slots)
        {
            if (slot == null || slot.IsEmpty || slot.item == null)
            {
                continue;
            }

            result.Add(new InventoryItemDto
            {
                itemId = slot.item.itemId,
                count = slot.count
            });
        }

        return result.ToArray();
    }

    // 현재 토큰 수치를 읽어 스냅샷에 포함한다.
    private int BuildCurrentToken()
    {
        if (tokenManager == null)
        {
            Debug.LogWarning("[GameStateAssembler] TokenManager reference is missing.", this);
            return 0;
        }

        return tokenManager.token;
    }

    private int BuildFarmLevel()
    {
        if (farmLevelManager == null)
        {
            Debug.LogWarning("[GameStateAssembler] FarmLevelManager reference is missing.", this);
            return 1;
        }

        return Mathf.Max(1, farmLevelManager.farmLevel);
    }

    private int BuildFarmNowExp()
    {
        if (farmLevelManager == null)
        {
            Debug.LogWarning("[GameStateAssembler] FarmLevelManager reference is missing.", this);
            return 0;
        }

        return Mathf.Max(0, farmLevelManager.nowfarmExp);
    }

    private int BuildGold()
    {
        if (goldManager == null)
        {
            Debug.LogWarning("[GameStateAssembler] GoldManager reference is missing.", this);
            return 0;
        }

        return Mathf.Max(0, goldManager.GetGold());
    }

    private int BuildCharacterID()
    {
        if (characterManager == null)
        {
            characterManager = CharacterManager.Instance != null
                ? CharacterManager.Instance
                : FindFirstObjectByType<CharacterManager>();
        }

        if (characterManager == null)
        {
            Debug.LogWarning("[GameStateAssembler] CharacterManager reference is missing.", this);
            return 0;
        }

        return Mathf.Max(0, characterManager.CharacterID);
    }

    private QuestStateDto BuildQuest()
    {
        if (questManager == null)
        {
            questManager = FindFirstObjectByType<QuestManager>(FindObjectsInactive.Include);
        }

        if (questManager == null)
        {
            Debug.LogWarning("[GameStateAssembler] QuestManager reference is missing.", this);
            return null;
        }

        return questManager.CreateState();
    }

    private string GetDefaultUserId()
    {
        PlayerId playerId = NetworkManager.Instance.GetPlayerId();
        if (playerId != null && !string.IsNullOrWhiteSpace(playerId.userId))
        {
            return playerId.userId;
        }

        return "Unity";
    }

    private bool TryValidateSnapshot(GameStateSnapshot snapshot, out string error)
    {
        error = null;

        if (snapshot == null)
        {
            error = "snapshot is null.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(snapshot.userId))
        {
            error = "userId is required.";
            return false;
        }

        if (snapshot.currentToken < 0)
        {
            error = "currentToken must be 0 or greater.";
            return false;
        }

        if (snapshot.farmLevel <= 0)
        {
            error = "farmLevel must be 1 or greater.";
            return false;
        }

        if (snapshot.farmNowExp < 0)
        {
            error = "farmNowExp must be 0 or greater.";
            return false;
        }

        if (snapshot.gold < 0)
        {
            error = "gold must be 0 or greater.";
            return false;
        }

        if (snapshot.characterID < 0)
        {
            error = "characterID must be 0 or greater.";
            return false;
        }

        if (snapshot.quest != null)
        {
            if (snapshot.quest.currentQuestIndex < 0
                || snapshot.quest.currentQuestID < 0
                || snapshot.quest.currentQuestProgressNow < 0
                || snapshot.quest.currentQuestProgressMax < 0)
            {
                error = "quest has invalid values.";
                return false;
            }
        }

        if (snapshot.inventory == null)
        {
            error = "inventory is required.";
            return false;
        }

        for (int i = 0; i < snapshot.inventory.Length; i++)
        {
            InventoryItemDto item = snapshot.inventory[i];
            if (item == null)
            {
                error = $"inventory[{i}] is null.";
                return false;
            }

            if (item.itemId < 0 || item.count < 0)
            {
                error = $"inventory[{i}] has invalid values.";
                return false;
            }
        }

        if (snapshot.tiles == null)
        {
            error = "tiles is required.";
            return false;
        }

        int expectedTileCount = middleDB != null ? middleDB.TileCount : 135;

        if (snapshot.tiles.Length != expectedTileCount)
        {
            error = $"tiles length must be {expectedTileCount}, but was {snapshot.tiles.Length}.";
            return false;
        }

        bool[] seenIds = new bool[expectedTileCount];

        for (int i = 0; i < snapshot.tiles.Length; i++)
        {
            TileStateDto tile = snapshot.tiles[i];
            if (tile == null)
            {
                error = $"tiles[{i}] is null.";
                return false;
            }

            if (tile.id < 0 || tile.id >= expectedTileCount)
            {
                error = $"tiles[{i}].id must be between 0 and {expectedTileCount - 1}, but was {tile.id}.";
                return false;
            }

            if (seenIds[tile.id])
            {
                error = $"tiles[{i}].id {tile.id} is duplicated.";
                return false;
            }

            seenIds[tile.id] = true;

            if (string.IsNullOrWhiteSpace(tile.tileType)
                || string.IsNullOrWhiteSpace(tile.cropType)
                || string.IsNullOrWhiteSpace(tile.cropState))
            {
                error = $"tiles[{i}] has empty string fields.";
                return false;
            }

            if (tile.growDuration < 0f || tile.maxTime < 0f)
            {
                error = $"tiles[{i}] has invalid grow time values.";
                return false;
            }
        }

        for (int id = 0; id < seenIds.Length; id++)
        {
            if (!seenIds[id])
            {
                error = $"tiles is missing id {id}.";
                return false;
            }
        }

        return true;
    }

    private string SaveSnapshotJsonToDesktop(GameStateSnapshot snapshot)
    {
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        string fileName = $"game_snapshot_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        string filePath = Path.Combine(desktopPath, fileName);
        string json = JsonConvert.SerializeObject(snapshot, Formatting.Indented);

        File.WriteAllText(filePath, json);
        return filePath;
    }

    private void ApplyLoadedSnapshot(GameStateSnapshot snapshot)
    {
        // 로드된 값을 타일, 인벤토리, 재화, 농장 레벨 순서로 현재 씬에 반영한다.
        if (snapshot == null)
        {
            return;
        }

        if (middleDB != null && snapshot.worldSeed != 0)
        {
            middleDB.SetWorldSeed(snapshot.worldSeed);
        }

        if (middleDB != null)
        {
            middleDB.LoadTileStates(snapshot.tiles);
        }

        if (tileManager != null)
        {
            tileManager.RefreshAllTiles();
            tileManager.RefreshNavigationGraph();
        }

        if (inventoryManager != null)
        {
            inventoryManager.LoadInventory(snapshot.inventory);
        }

        if (tokenManager != null)
        {
            tokenManager.SetToken(snapshot.currentToken);
        }

        if (goldManager != null)
        {
            goldManager.SetGold(snapshot.gold);
        }

        if (farmLevelManager != null)
        {
            farmLevelManager.InitializeFromBackend(new FarmLevelStateDto
            {
                farmLevel = snapshot.farmLevel > 0 ? snapshot.farmLevel : 1,
                farmNowExp = Mathf.Max(0, snapshot.farmNowExp)
            });
        }

        if (characterManager != null)
        {
            characterManager.SetCharacterIDWithoutSFX(Mathf.Max(0, snapshot.characterID));
        }

        ApplyQuestState(snapshot.quest);
    }

    private void ApplyDefaultState(bool activateQuest)
    {
        // 저장본이 없을 때 새 게임 시작 전의 기본 상태를 맞춰 둔다.
        if (middleDB != null)
        {
            middleDB.ResetToDefaultState();
        }

        if (tileManager != null)
        {
            tileManager.RefreshAllTiles();
            tileManager.RefreshNavigationGraph();
        }

        if (inventoryManager != null)
        {
            inventoryManager.LoadInventory(null);
        }

        if (tokenManager != null)
        {
            tokenManager.SetToken(tokenManager.MaxTokenCount);
        }

        if (goldManager != null)
        {
            goldManager.InitializeFromBackend(null);
        }

        if (farmLevelManager != null)
        {
            farmLevelManager.InitializeFromBackend(null);
        }

        if (characterManager != null)
        {
            characterManager.SetCharacterIDWithoutSFX(0);
        }

        if (activateQuest)
        {
            ApplyQuestState(null);
        }
    }

    private void ApplyQuestState(QuestStateDto state)
    {
        if (questManager == null)
        {
            questManager = FindFirstObjectByType<QuestManager>(FindObjectsInactive.Include);
        }

        if (questManager == null)
        {
            Debug.LogWarning("[GameStateAssembler] QuestManager reference is missing. Quest UI cannot be activated.", this);
            return;
        }

        if (!questManager.gameObject.activeSelf)
        {
            questManager.gameObject.SetActive(true);
        }

        questManager.InitializeFromBackend(state);
    }
}

[Serializable]
// 로컬 파일에 저장할 게임 상태의 최상위 묶음.
public class GameStateSnapshot
{
    // 내부 조립용 전체 스냅샷. 저장 요청 직전 DTO로 다시 변환된다.
    public int schemaVersion;
    public string userId;
    public int worldSeed;
    public string savedAt;
    public TileStateDto[] tiles;
    public InventoryItemDto[] inventory;
    public int currentToken;
    public int farmLevel;
    public int farmNowExp;
    public int gold;
    public int characterID;
    public QuestStateDto quest;
}

[Serializable]
// 타일 하나를 저장하기 위한 최소 상태 정보.
public class TileStateDto
{
    public int id;
    public string tileType;
    public string cropType;
    public string cropState;
    public int variantIndex;
    public float growDuration;
    public float maxTime;
}

[Serializable]
// 인벤토리 아이템 한 종류의 수량 정보.
public class InventoryItemDto
{
    public int itemId;
    public int count;
}

[Serializable]
public class HealthCheckResponse
{
    public string status;
    public string database;
    public string time;
}

[Serializable]
public class SnapshotUploadResponse
{
    public string id;
    public string userId;
    public int currentToken;
    public int farmLevel;
    public int farmNowExp;
    public int gold;
    public int characterID;
    public QuestStateDto quest;
    public int tileCount;
    public int inventoryCount;
    public string savedAt;
}
