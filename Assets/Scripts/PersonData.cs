using UnityEngine;

[CreateAssetMenu(fileName = "New Person", menuName = "Game/Person")]
public class PersonData : ScriptableObject
{
    public string personName;
    public string description;
}