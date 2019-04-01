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
    private GameActor.Alignment align;
    public void ShowCharacter(GameActor.Alignment charType)
    {
        Debug.Assert(Character == null, "showing already shown character");

        align = charType;
        var prefab = align == GameActor.Alignment.Player ? PlayerSprite : MobSprite;
        Character = Instantiate(prefab);

        Character.transform.SetParent(transform, false);
        Character.GetComponent<SpriteRenderer>().sortingLayerName = "Character";
    }
    public void OnPositionUpdated()
    {
        // slot moved, so we need to update the positions of UI stuff not parented by this Transform
        if (Nameplate != null)
        {
            var offset = align == GameActor.Alignment.Player ? nameplateOffsetL : nameplateOffsetR;
            var screen = WorldToScreenPoint(transform.position);
            Nameplate.transform.position = screen + offset;
        }
    }
    static readonly Vector2 nameplateOffsetL = new Vector2(-52, 20);
    static readonly Vector2 nameplateOffsetR = new Vector2(52, 20);
    public Nameplate Nameplate { get; private set; }
    public void ShowNameplate(GameActor.Alignment charType)
    {
        Debug.Assert(Nameplate == null, "showing already shown nameplate");

        GameObject nameplateGO = Instantiate(NameplateUI);
        Nameplate = nameplateGO.GetComponent<Nameplate>();
        Nameplate.transform.SetParent(UIParent.transform, false);
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
