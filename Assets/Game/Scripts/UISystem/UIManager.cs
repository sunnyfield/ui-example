using UnityEngine.UIElements;
using Unity.Collections;

namespace Game.Scripts.UISystem
{
    public static class UIManager
    {
        private static VisualElement[] s_elements;
        private static Button[] s_difficultyButtons;
        public static int s_selectedDifficultyIndex;
        
        public static void RegisterElements(VisualElement root, NativeArray<int> difficultySizes)
        {
            s_elements = new VisualElement[8];
            s_difficultyButtons = new Button[difficultySizes.Length];
            s_selectedDifficultyIndex = 0;
            
            RegisterElement(UIElementId.PlayerScore, root.Q<Label>("score-label"));
            RegisterElement(UIElementId.PlayButtonIcon, root.Q<VisualElement>("play-icon"));
            RegisterElement(UIElementId.PlayButton, root.Q<Button>("play-button"));
            RegisterElement(UIElementId.PuzzlePictureView, root.Q<VisualElement>("PuzzlePicture"));
            RegisterElement(UIElementId.PuzzleStartView, root.Q<VisualElement>("PuzzlePreview"));
            RegisterElement(UIElementId.DifficultyContainer, root.Q<VisualElement>("difficulty-container"));
            RegisterElement(UIElementId.PlayerScoreIcon, root.Q<VisualElement>("coin-icon"));

            CreateDifficultyButtons(difficultySizes);

            if (GetElement(UIElementId.PlayButton) is Button playButton)
            {
                playButton.clicked += () => {
                    int selectedDifficulty = difficultySizes[s_selectedDifficultyIndex];
                    UIEventScheduler.SchedulePlayButtonClick(selectedDifficulty);
                };
            }
        }
        
        private static void RegisterElement(UIElementId elementId, VisualElement element)
        {
            int index = (int)elementId;
            s_elements[index] = element;
        }
        
        private static VisualElement GetElement(UIElementId elementId)
        {
            int index = (int)elementId;
            return (index < s_elements.Length) ? s_elements[index] : null;
        }
        
        public static void ProcessUIUpdate(UIUpdateCommand command)
        {
            switch (command.EventType)
            {
                case UIEventType.UpdateScore:
                    HandleUpdateScore(command);
                    break;
                case UIEventType.ChangeIcon:
                    HandleChangeIcon(command);
                    break;
                case UIEventType.SetVisibility:
                    HandleSetVisibility(command);
                    break;
                case UIEventType.DifficultySelectionUI:
                    HandleDifficultySelectionUI(command);
                    break;
            }
        }
        
        private static void HandleUpdateScore(UIUpdateCommand command)
        {
            if (GetElement(command.ElementId) is Label scoreLabel)
            {
                scoreLabel.text = command.IntValue.ToString();
            }
        }
        
        private static void HandleChangeIcon(UIUpdateCommand command)
        {
            var element = GetElement(command.ElementId);
            var texture = AddressableAssetManager.GetAsset(command.AssetHandle);
            element.style.backgroundImage = new StyleBackground(texture);
        }
        
        private static void HandleSetVisibility(UIUpdateCommand command)
        {
            var element = GetElement(command.ElementId);
            if (element != null)
            {
                element.style.display = (command.Flags == 1) ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
        
        private static void HandleDifficultySelectionUI(UIUpdateCommand command)
        {
            int newIndex = command.IntValue;
            if (s_difficultyButtons != null && newIndex >= 0 && newIndex < s_difficultyButtons.Length)
            {
                if (s_selectedDifficultyIndex != newIndex)
                {
                    s_difficultyButtons[s_selectedDifficultyIndex].RemoveFromClassList("difficulty-selected");
                    s_selectedDifficultyIndex = newIndex;
                    s_difficultyButtons[s_selectedDifficultyIndex].AddToClassList("difficulty-selected");
                }
            }
        }
        
        private static void CreateDifficultyButtons(NativeArray<int> difficultySizes)
        {
            var container = GetElement(UIElementId.DifficultyContainer);
            if (container == null) return;

            for (int i = 0; i < difficultySizes.Length; i++)
            {
                var button = new Button();
                button.text = difficultySizes[i].ToString();
                button.name = $"difficulty-{difficultySizes[i].ToString()}";
                button.AddToClassList("difficulty-button");
                button.style.fontSize = new StyleLength(new Length(50, LengthUnit.Percent));
                
                if (i == 0)
                {
                    button.AddToClassList("difficulty-selected");
                }
                
                int buttonIndex = i;
                button.clicked += () => SelectDifficulty(buttonIndex);
                
                s_difficultyButtons[i] = button;
                container.Add(button);
            }
        }
        
        private static void SelectDifficulty(int index) => UIEventScheduler.ScheduleDifficultySelectionUI(index);

        public static void Cleanup()
        {
            s_elements = null;
            s_difficultyButtons = null;
        }
    }
}