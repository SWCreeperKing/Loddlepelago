using Loddlepelago.CopyPastas;
using UnityEngine;

namespace Loddlepelago;

public static class PrivateFixes
{
    public static GameObject RaycastAgainstUI_PUBLIC(this CentralGameMenu menu, Vector2 position)
        => menu.CallPrivateMethod<GameObject>("RaycastAgainstUI", position);

    public static Vector2 CanvasToScreenSpace_PUBLIC(this CentralGameMenu menu, Vector2 canvasPosition)
        => menu.CallPrivateMethod<Vector2>("CanvasToScreenSpace", canvasPosition);
}