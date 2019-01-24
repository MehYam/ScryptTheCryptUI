using UnityEngine;

using ScryptTheCrypt;

public class CharacterSlot : MonoBehaviour
{
    #pragma warning disable 0649 // A fix to Unity is in the works: https://github.com/dotnet/roslyn/issues/30172
    [SerializeField] GameObject PlayerSprite;
    [SerializeField] GameObject MobSprite;
    [SerializeField] GameObject TurnIndicator;
    [SerializeField] GameObject TargetIndicator;
    [SerializeField] GameObject CallToAttentionIndicator;
    [SerializeField] GameObject Floor;
    [SerializeField] GameObject NameplateUI;

    private void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("CharacterLayer");
    }
    public GameObject Character { get; private set; }
    public void ShowCharacter(Game.ActorAlignment charType)
    {
        Debug.Assert(Character == null, "showing already shown character");

        var prefab = charType == Game.ActorAlignment.Player ? PlayerSprite : MobSprite;
        Character = Instantiate(prefab);

        Character.transform.SetParent(transform, false);
        Character.GetComponent<SpriteRenderer>().sortingLayerName = "Character";
    }
    public Nameplate Nameplate { get; private set; }
    public void ShowNameplate()
    {
        Debug.Assert(Nameplate == null, "showing already shown nameplate");

        GameObject nameplateGO = Instantiate(NameplateUI);
        Nameplate = nameplateGO.GetComponent<Nameplate>();

        var UIParent = GameObject.Find("UI");
        Nameplate.transform.SetParent(UIParent.transform, false);

        var screen = RectTransformUtility.WorldToScreenPoint(Camera.main, transform.position);
        screen.y -= 65;
        Nameplate.transform.position = screen;
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
