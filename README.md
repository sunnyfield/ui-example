# Unity Command-Based UI Architecture Example

**Unity**: 6000.1.4f1

A UI implementation approach with Unity UI Toolkit and a custom command-based UI system.

## Overview

This project demonstrates a Unity architecture that prioritizes performance through command-based UI processing, separating concerns between game logic and UI presentation while maintaining Unity UI Toolkit integration.

## Architecture Highlights

### Command-Based UI System

The project implements a command-driven UI architecture that differs from MVVM patterns:

**Command-Based Approach:**
- **Decoupled Communication**: UI updates flow through command queues instead of direct bindings
- **Performance Optimized**: Batch processing with configurable limits (64 UI updates, 16 actions per frame)
- **Single Responsibility**: Clear separation between game logic (`GameRoot`) and UI processing (`UIManager`)

### Core Components

#### GameRoot (`Assets/Game/Scripts/GameRoot.cs:13`)
Central coordinator managing:
- **Phase-based initialization**: Start → ConfigLoaded → UILoaded → Ready
- **Custom PlayerLoop integration** for game-specific update cycles
- **Command queue processing** with performance throttling
- **Asset loading** via Unity Addressables

#### UI Command System
```csharp
// Command structure for all UI operations
public struct UIUpdateCommand
{
    public UIEventType EventType;    // UpdateScore, ChangeIcon, SetVisibility, etc.
    public UIElementId ElementId;    // PlayerScore, PlayButton, PuzzleView, etc.
    public int IntValue;             // Numeric data
    public AssetHandle AssetHandle;  // Asset references
    public byte Flags;               // Boolean flags
}
```

#### UI Manager (`Assets/Game/Scripts/UISystem/UIManager.cs`)
**Action-first processing** with array-based element lookup:
- Elements registered once at startup using `UIElementId` enum indices
- Command processing via switch statements
- No individual component controllers - unified command processing

#### Event Scheduler (`Assets/Game/Scripts/UISystem/UIEventScheduler.cs`)
**Type-safe command creation**:
- `ScheduleScoreUpdate(int newScore)`
- `ScheduleIconChange(UIElementId elementId, AssetHandle assetHandle)`
- `ScheduleVisibilityChange(UIElementId elementId, bool visible)`
- `SchedulePlayButtonClick(int difficultyLevel)`

### UI Structure

**Template-Based Composition** (`Assets/UI/UXML/PuzzleGame.uxml`):
```xml
<ui:Template name="UserScore" />      <!-- Score display with dynamic icon -->
<ui:Template name="PuzzlePicture" />  <!-- Central puzzle image area -->
<ui:Template name="PuzzlePreview" />  <!-- Difficulty selection & play button -->
```

**Hierarchical Layout**:
- **UserScore**: Top-right positioned score with coin icon
- **PuzzlePicture**: Center area (hidden until game starts)
- **PuzzlePreview**: Difficulty buttons + play button with dynamic icon

### Key Features Implementation

#### Dynamic UI Elements
- **Score Updates**: Real-time score changes every 500 frames with icon switching
- **Play Button State**: Dynamically switches between coin/ads icons based on player score
- **Visibility Control**: Hide/show UI sections via command system
- **Difficulty Selection**: Generated buttons with CSS class-based styling

#### Asset Management (`Assets/Game/Scripts/UISystem/AddressableAssetManager.cs`)
- **Lightweight handles**: `AssetHandle` struct with uint ID for memory efficiency
- **Preloaded assets**: Coin and ads icons loaded at startup
- **CSS integration**: Assets applied via Unity UI Toolkit's style system

## Implementation Details

### Command Flow Example
```csharp
// Game logic triggers score change
s_playerData.Score = newScore;

// Schedule UI update via command
UIEventScheduler.ScheduleScoreUpdate(newScore);

// Command queued in GameRoot
GameRoot.EnqueueUIUpdate(command);

// Processed in game loop with batching
UIManager.ProcessUIUpdate(command);
```

### Directory Structure
```
Assets/
├── Game/Scripts/
│   ├── GameRoot.cs                     # Main coordinator & command processing
│   ├── UISystem/
│   │   ├── UIManager.cs                # UI element registration & command handling
│   │   ├── UIEventScheduler.cs         # Type-safe command creation
│   │   └── AddressableAssetManager.cs  # Asset handle system
│   └── DataModel/
│       └── PersistentPlayerData.cs     # Player data structure
└── UI/
    ├── UXML/                           # UI layout templates
    └── USS/                            # UI styling
```

This architecture demonstrates how to build scalable, performant UI systems in Unity while maintaining clean separation of concerns.