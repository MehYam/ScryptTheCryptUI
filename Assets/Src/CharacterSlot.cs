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
    private void OnDestroy()
    {
        if (Nameplate != null)
        {
            GameObject.Destroy(Nameplate.gameObject);
        }
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
    static readonly Vector2 nameplateOffsetL = new Vector2(-52, 20);
    static readonly Vector2 nameplateOffsetR = new Vector2(52, 20);
    public Nameplate Nameplate { get; private set; }
    public void ShowNameplate(Game.ActorAlignment charType)
    {
        Debug.Assert(Nameplate == null, "showing already shown nameplate");

        GameObject nameplateGO = Instantiate(NameplateUI);
        Nameplate = nameplateGO.GetComponent<Nameplate>();
        Nameplate.transform.SetParent(UIParent.transform, false);

        var offset = charType == Game.ActorAlignment.Player ? nameplateOffsetL : nameplateOffsetR;
        var screen = WorldToScreenPoint(transform.position);
        Nameplate.transform.position = screen + offset;
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
