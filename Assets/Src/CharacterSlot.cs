using UnityEngine;
using UnityEngine.UI;

using ScryptTheCrypt;

public class CharacterSlot : MonoBehaviour
{
    [SerializeField] private GameObject PlayerSprite = null;
    [SerializeField] private GameObject MobSprite = null;
    [SerializeField] private GameObject TurnIndicator = null;
    [SerializeField] private GameObject TargetIndicator = null;
    [SerializeField] private GameObject DeathIndicator = null;
    [SerializeField] private GameObject DamageText = null;
    [SerializeField] private GameObject CallToAttentionIndicator = null;
    [SerializeField] private GameObject Floor = null;
    [SerializeField] private GameObject NameplateUI = null;

    GameObject UIParent = null;
    static private Vector2 WorldToScreenPoint(Vector2 pos)
    {
        return RectTransformUtility.WorldToScreenPoint(Camera.main, pos);
    }
    void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("CharacterLayer");
        UIParent = GameObject.Find("UI");

        Debug.Assert(UIParent != null, "couldn't find UI parent");
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
        Nameplate.transform.SetParent(UIParent.transform, false);

        var screen = WorldToScreenPoint(transform.position);
        screen.y -= 65;
        Nameplate.transform.position = screen;
    }
    private void ToggleIndicator(bool show, Transform parent, GameObject prefab, ref GameObject existing)
    {
        if (show && existing == null)
        {
            existing = Instantiate(prefab);
            existing.transform.SetParent(parent, false);
            existing.transform.localPosition = Vector2.zero;
        }
        else if (!show && existing != null)
        {
            Destroy(existing.gameObject);
            existing = null;
        }
    }
    private GameObject turnIndicator;
    public void ToggleTurnIndicator(bool show)
    {
        ToggleIndicator(show, Floor.transform, TurnIndicator, ref turnIndicator);
    }
    private GameObject targetIndicator;
    public void ToggleTargetIndicator(bool show)
    {
        ToggleIndicator(show, Floor.transform, TargetIndicator, ref targetIndicator);
    }
    private GameObject deathIndicator;
    public void ToggleDeathIndicator(bool show)
    {
        ToggleIndicator(show, transform, DeathIndicator, ref deathIndicator);
    }
    public void ShowDamageText(string damageText)
    {
        GameObject obj = Instantiate(DamageText);
        obj.transform.SetParent(UIParent.transform, false);
        obj.transform.position = WorldToScreenPoint(transform.position);

        obj.GetComponentInChildren<Text>().text = damageText;
    }
}
