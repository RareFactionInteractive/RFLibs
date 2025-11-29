# RFLibs.DI - Dependency Injection Framework

A lightweight, attribute-based dependency injection framework for C# with support for multiple lifetime patterns and scope management.

## Features

- 🔄 **Lifetime Patterns**: Singleton (cached) and Transient (new instance per resolve)
- 🎯 **Scope Management**: Global (application-wide) and Scene (cleared on scene unload)
- 💉 **Automatic Injection**: Field injection using `[Inject]` attribute
- 🧪 **Type-Safe**: Compile-time type checking with Result pattern for error handling

---

## Quick Start

### 1. Define Your Services

```csharp
public interface ILogger
{
    void Log(string message);
}

[Lifetime(Lifetime.Singleton)]
public class ConsoleLogger : ILogger
{
    public void Log(string message) => Console.WriteLine(message);
}

[Lifetime(Lifetime.Transient)]
public class Calculator : ICalculator
{
    public int Add(int a, int b) => a + b;
}
```

### 2. Bind Services

```csharp
// Bind a singleton logger that lives globally
DI.Bind<ILogger>(new ConsoleLogger());

// Bind a transient calculator (new instance per resolve)
DI.Bind<ICalculator>(new Calculator());
```

### 3. Resolve Services

```csharp
var logger = DI.Resolve<ILogger>();
if (logger.IsOk)
{
    logger.Ok.Log("Hello, DI!");
}
```

### 4. Use Dependency Injection

```csharp
public class GameManager
{
    [Inject] private ILogger _logger;
    [Inject] private ICalculator _calculator;

    public void Start()
    {
        DI.InjectDependencies(this);
        _logger.Log("GameManager started!");
    }
}
```

---

## Attributes

### `[Lifetime]` - Controls Instantiation

Determines how instances are created when resolving:

```csharp
[Lifetime(Lifetime.Singleton)]  // Reuses same instance (cached)
[Lifetime(Lifetime.Transient)]  // Creates new instance each time
```

**Default:** `Singleton` if not specified

**Examples:**
```csharp
// Singleton - same instance shared everywhere
[Lifetime(Lifetime.Singleton)]
public class GameConfig { }

// Transient - new instance every time
[Lifetime(Lifetime.Transient)]
public class Bullet { }
```

### `[Scope]` - Controls Registration Lifetime

Determines when registrations are cleared:

```csharp
[Scope(Scope.Global)]  // Persists for entire application
[Scope(Scope.Scene)]   // Cleared when scene unloads
```

**Default:** `Global` if not specified

**Examples:**
```csharp
// Global scope - lives forever
[Scope(Scope.Global)]
public class AudioManager { }

// Scene scope - cleared with scene
[Scope(Scope.Scene)]
public class LevelData { }
```

### Combining Attributes

```csharp
// Singleton in Scene - one instance per scene, cleared on scene change
[Lifetime(Lifetime.Singleton), Scope(Scope.Scene)]
public class EnemySpawner { }

// Transient in Global - new instance each time, factory persists forever
[Lifetime(Lifetime.Transient), Scope(Scope.Global)]
public class NetworkRequest { }

// Transient in Scene - new instance each time, cleared with scene
[Lifetime(Lifetime.Transient), Scope(Scope.Scene)]
public class ParticleEffect { }
```

---

## API Reference

### `DI.Bind<T>(T implementation)`

Registers a service implementation.

**Parameters:**
- `implementation`: Instance to bind

**Returns:** `Result<bool, DIErrors>` - Success or error

**Example:**
```csharp
var result = DI.Bind<ILogger>(new ConsoleLogger());
if (result.IsErr)
{
    Debug.LogError("Failed to bind logger");
}
```

### `DI.Resolve<T>()`

Resolves a registered service.

**Returns:** `Result<T, DIErrors>` - Resolved instance or error

**Behavior:**
- Searches Global container first
- Falls back to Scene container
- Returns error if not found

**Example:**
```csharp
var logger = DI.Resolve<ILogger>();
if (logger.IsOk)
{
    logger.Ok.Log("Success!");
}
else
{
    Debug.LogError("Logger not found");
}
```

### `DI.InjectDependencies(object instance)`

Injects dependencies into an object's fields marked with `[Inject]`.

**Parameters:**
- `instance`: Object to inject into

**Example:**
```csharp
public class Player
{
    [Inject] private IWeaponSystem _weapons;
    [Inject] private IHealthSystem _health;
    
    void Awake()
    {
        DI.InjectDependencies(this);
    }
}
```

### `DI.Clear()`

Clears all registrations from both containers.

**Example:**
```csharp
// When changing scenes or restarting
DI.Clear();
```

---

## Common Patterns

### Singleton Pattern

One instance shared across the application:

```csharp
[Lifetime(Lifetime.Singleton), Scope(Scope.Global)]
public class GameState { }

// Usage
DI.Bind<IGameState>(new GameState());
var state1 = DI.Resolve<IGameState>();
var state2 = DI.Resolve<IGameState>();
// state1 == state2 (same instance)
```

### Factory Pattern

Create new instances on demand:

```csharp
[Lifetime(Lifetime.Transient)]
public class Enemy { }

// Usage
DI.Bind<IEnemy>(new Enemy());
var enemy1 = DI.Resolve<IEnemy>();
var enemy2 = DI.Resolve<IEnemy>();
// enemy1 != enemy2 (different instances)
```

### Scene-Scoped Services

Services that reset with each scene:

```csharp
[Lifetime(Lifetime.Singleton), Scope(Scope.Scene)]
public class LevelProgress { }

// In scene load:
DI.Bind<ILevelProgress>(new LevelProgress());

// In scene unload:
DI.Clear(); // Removes scene-scoped services
```

---

## Best Practices

### ✅ Do

- Use `Singleton` for managers and shared state
- Use `Transient` for objects that should be unique each time
- Use `Scope.Scene` for level-specific data
- Check `Result.IsOk` before accessing resolved values
- Clear DI when changing scenes/levels

### ❌ Don't

- Don't bind null implementations
- Don't resolve in performance-critical loops (cache results)
- Don't forget to inject dependencies before using them
- Don't mix DI with manual instantiation patterns

---

## Error Handling

The DI system uses the Result pattern for safe error handling:

```csharp
var service = DI.Resolve<IService>();

if (service.IsOk)
{
    // Success - use service.Ok
    service.Ok.DoSomething();
}
else
{
    // Error - check service.Err
    switch (service.Err)
    {
        case DIErrors.CannotResolve:
            Debug.LogError("Service not registered");
            break;
        case DIErrors.NullBinding:
            Debug.LogError("Attempted to bind null");
            break;
    }
}
```

---

## Example: Complete Game Setup

```csharp
// 1. Define services
public interface IAudioManager { void Play(string sound); }
public interface IInputHandler { Vector2 GetMovement(); }
public interface IScoreTracker { void AddScore(int points); }

[Lifetime(Lifetime.Singleton), Scope(Scope.Global)]
public class AudioManager : IAudioManager
{
    public void Play(string sound) => Debug.Log($"Playing: {sound}");
}

[Lifetime(Lifetime.Singleton), Scope(Scope.Global)]
public class InputHandler : IInputHandler
{
    public Vector2 GetMovement() => new Vector2(Input.GetAxis("Horizontal"), 0);
}

[Lifetime(Lifetime.Singleton), Scope(Scope.Scene)]
public class ScoreTracker : IScoreTracker
{
    private int _score;
    public void AddScore(int points) => _score += points;
}

// 2. Bootstrap (application start)
public class GameBootstrap
{
    void Start()
    {
        DI.Bind<IAudioManager>(new AudioManager());
        DI.Bind<IInputHandler>(new InputHandler());
    }
}

// 3. Level setup
public class LevelManager
{
    void OnLevelLoad()
    {
        DI.Bind<IScoreTracker>(new ScoreTracker());
    }
    
    void OnLevelUnload()
    {
        DI.Clear(); // Removes scene-scoped services
    }
}

// 4. Use in game objects
public class Player
{
    [Inject] private IAudioManager _audio;
    [Inject] private IInputHandler _input;
    [Inject] private IScoreTracker _score;
    
    void Awake()
    {
        DI.InjectDependencies(this);
    }
    
    void Update()
    {
        var movement = _input.GetMovement();
        // ... move player ...
    }
    
    void OnCollectCoin()
    {
        _audio.Play("coin");
        _score.AddScore(100);
    }
}
```

---

## Unity Scene Management

The DI system integrates with Unity to manage scene-scoped services.

### Automatic Scene Management (Recommended)

Add the `DISceneManager` component to a GameObject in your scene for automatic cleanup:

```csharp
// DISceneManager.cs (included in RFLibs.DI.Unity)
using RFLibs.Unity;

public class GameSetup : MonoBehaviour
{
    void Awake()
    {
        // Add DISceneManager to handle automatic cleanup
        gameObject.AddComponent<DISceneManager>();
    }
}
```

This will automatically call `DI.ClearSceneContainer()` when scenes are unloaded.

### Manual Scene Management

Alternatively, manually clear the scene container in your scene management code:

```csharp
using UnityEngine.SceneManagement;
using RFLibs.DI;

public class SceneLoader : MonoBehaviour
{
    void Start()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnSceneUnloaded(Scene scene)
    {
        // Clear scene-scoped services
        DI.ClearSceneContainer();
        Debug.Log("Scene services cleared!");
    }
}
```

### Best Practices for Unity

1. **Bootstrap Scene**: Bind global services in a persistent bootstrap scene
2. **Level Scenes**: Bind scene-scoped services when levels load
3. **Use `ClearSceneContainer()`**: Call when transitioning between levels
4. **Don't use `Clear()`**: Only use `DI.Clear()` when restarting the entire game

**Example:**
```csharp
// Bootstrap.cs - Persistent scene
public class Bootstrap : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        
        // Bind global services
        DI.Bind<IAudioManager>(new AudioManager());
        DI.Bind<IInputHandler>(new InputHandler());
    }
}

// LevelSetup.cs - In each level scene
public class LevelSetup : MonoBehaviour
{
    void Start()
    {
        // Bind level-specific services
        DI.Bind<ILevelData>(new LevelData());
        DI.Bind<IEnemySpawner>(new EnemySpawner());
        
        // Automatic cleanup via DISceneManager
        gameObject.AddComponent<DISceneManager>();
    }
}
```

---

## License

Part of the RFLibs library.
