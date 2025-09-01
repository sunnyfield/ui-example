using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Scriptable Objects/GameConfig")]
public class GameConfig : ScriptableObject
{
    public int[] DifficultySizes = { 36, 64, 100, 144, 256, 400 };

    public int InitialPlayerScore = 10;
}
