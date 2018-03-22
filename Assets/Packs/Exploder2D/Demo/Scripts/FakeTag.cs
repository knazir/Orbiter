using UnityEngine;
using System.Collections;

public class FakeTag
{
    public static bool IsTag(Collider2D c, string tag)
    {
        var name = c.gameObject.name;

        switch (tag)
        {
            case "Enemy":
                return name.Contains("enemy");

            case "Player":
                return name == "hero";

            case "Obstacle":
                return name == "env_TowerFull" || name == "env_TowerFull";

            case "ground":
                return name == "env_PlatformBridge" || name == "env_PlatformBridge" || name == "env_PlatformTop" ||
                       name == "env_PlatformTop" || name == "env_PlatformUfo";

            case "BombPickup":
                return name == "bombCrate";

            default: return false;
        }
    }
}
