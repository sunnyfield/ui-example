namespace Game.Scripts.UISystem
{
    public enum UIEventType : byte
    {
        UpdateScore = 0,
        ChangeIcon = 1,
        SetVisibility = 2,
        DifficultySelectionUI = 3,
        
        PlayButtonClicked = 100,
    }
    
    public enum UIElementId : byte
    {
        PlayerScore = 0,
        PlayButtonIcon = 1,
        PlayButton = 2,
        PuzzlePictureView = 3,
        PuzzleStartView = 4,
        DifficultyContainer = 5,
        PlayerScoreIcon = 6
    }
    
    public struct UIUpdateCommand
    {
        public UIEventType EventType;
        public UIElementId ElementId;
        public int IntValue;
        public AssetHandle AssetHandle;
        public byte Flags;
    }
    
    public static class UIEventScheduler
    {
        public static void ScheduleScoreUpdate(int newScore)
        {
            GameRoot.EnqueueUIUpdate(new UIUpdateCommand 
            { 
                EventType = UIEventType.UpdateScore, 
                ElementId = UIElementId.PlayerScore,
                IntValue = newScore 
            });
        }
        
        public static void ScheduleIconChange(UIElementId elementId, AssetHandle assetHandle)
        {
            GameRoot.EnqueueUIUpdate(new UIUpdateCommand 
            { 
                EventType = UIEventType.ChangeIcon, 
                ElementId = elementId,
                AssetHandle = assetHandle
            });
        }
        
        public static void ScheduleVisibilityChange(UIElementId elementId, bool visible)
        {
            GameRoot.EnqueueUIUpdate(new UIUpdateCommand 
            { 
                EventType = UIEventType.SetVisibility, 
                ElementId = elementId,
                Flags = (byte)(visible ? 1 : 0)
            });
        }
        
        public static void SchedulePlayButtonClick(int difficultyLevel)
        {
            GameRoot.EnqueueUIAction(new UIUpdateCommand 
            { 
                EventType = UIEventType.PlayButtonClicked, 
                ElementId = UIElementId.PlayButton,
                IntValue = difficultyLevel
            });
        }
        
        public static void ScheduleDifficultySelectionUI(int difficultyIndex)
        {
            GameRoot.EnqueueUIUpdate(new UIUpdateCommand 
            { 
                EventType = UIEventType.DifficultySelectionUI, 
                ElementId = UIElementId.DifficultyContainer,
                IntValue = difficultyIndex
            });
        }
    }
}