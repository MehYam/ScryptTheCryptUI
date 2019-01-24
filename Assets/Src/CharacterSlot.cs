using UnityEngine;

using ScryptTheCrypt;

public class CharacterSlot : MonoBehaviour
{
    public GameObject PlayerSprite;
    public GameObject MobSprite;
    public GameObject TurnIndicator;
    public GameObject TargetIndicator;
    public GameObject CallToAttentionIndicator;

    public GameObject Floor;

    private void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("CharacterLayer");
    }
    public void ShowCharacter(Game.ActorAlignment charType)
    {
        var prefab = charType == Game.ActorAlignment.Player ? PlayerSprite : MobSprite;
        var character = Instantiate(prefab);

        character.transform.SetParent(transform, false);
        character.GetComponent<SpriteRenderer>().sortingLayerName = "Character";
    }
    private GameObject turnIndicator;
    public void ShowTurnIndicator(bool show)
    {
        if (show && turnIndicator == null)
        {
            turnIndicator = Instantiate(TurnIndicator);
            turnIndicator.transform.SetParent(Floor.transform, false);
            turnIndicator.transform.localPosition = Vector2.zero;
        }
        else if (!show && turnIndicator != null)
        {
            Destroy(turnIndicator.gameObject);
        }
    }
    private GameObject targetIndicator;
    public void ShowTargetIndicator(bool show)
    {
        if (show && targetIndicator == null)
        {
            targetIndicator = Instantiate(TargetIndicator);
            targetIndicator.transform.SetParent(Floor.transform, false);
            targetIndicator.transform.localPosition = Vector2.zero;
        }
        else if (!show && targetIndicator != null)
        {
            Destroy(targetIndicator.gameObject);
        }
    }
}
