using UnityEngine;

public class WeaponIconManager : MonoBehaviour
{
    public static WeaponIconManager Instance { get; private set; }

    [SerializeField] public Sprite mp5Icon;
    [SerializeField] public Sprite shotgunIcon;
    [SerializeField] public Sprite smgIcon;
    [SerializeField] public Sprite uziIcon;
    [SerializeField] public Sprite m16Icon;
    [SerializeField] public Sprite sniperIcon;
    [SerializeField] public Sprite magnumIcon;
    [SerializeField] public Sprite ak47Icon;
    [SerializeField] public Sprite lmgIcon;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}