using Tuto;

public struct OnDoorOpen_TUTO
{
    public int action;
    public int indexDoor;
}

public struct OnDialogue_TUTO
{
    public string dialogue;
    public float duration;
    public Speaker speaker;
}

public struct OnNotification_TUTO
{
    public string notificationText;
    public NotificationTarget speaker;
    public bool activated;
    public NotificationDisableAction disableAction;
    public float duration;
}

public struct OnFireModeChanged_TUTO
{ }

public struct OnLaserFired_TUTO
{ }

public struct OnDroneUsed_TUTO
{ }

public struct OnHealUsed_TUTO
{ }

public struct OnFillAmount_TUTO
{
    public bool activated;
    public float maxPercentage;
    public float speed;
    public AnimationBar type;
}

public struct OnStartSpawner_TUTO
{
    public int spawnIndex;
}