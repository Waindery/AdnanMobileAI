using UnityEngine;

public static class BattleBackground
{
    public static void Create()
    {
        if (GameObject.Find("BattleArena") != null)
            return;

        GameObject arena = new GameObject("BattleArena");

        CreatePlane(arena.transform, "Floor", new Vector3(0f, BattleGround.FloorY, 0f), new Vector3(16f, 1f, 11f),
            new Color(0.08f, 0.1f, 0.16f));

        CreatePlane(arena.transform, "BackWall", new Vector3(0f, 2.5f, 5.5f), new Vector3(18f, 5f, 0.2f),
            new Color(0.05f, 0.08f, 0.14f));

        CreatePlane(arena.transform, "SideGlow", new Vector3(0f, 1.2f, 3f), new Vector3(14f, 3f, 0.1f),
            new Color(0.12f, 0.18f, 0.32f));

        CreateAccentStripes(arena.transform);
    }

    private static void CreatePlane(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
    {
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.name = name;
        plane.transform.SetParent(parent);
        plane.transform.position = position;
        plane.transform.localScale = scale;
        Object.Destroy(plane.GetComponent<Collider>());

        Renderer renderer = plane.GetComponent<Renderer>();
        if (renderer != null)
            renderer.sharedMaterial = CreateSafeMaterial(name, color);
    }

    private static void CreateAccentStripes(Transform parent)
    {
        for (int i = 0; i < 3; i++)
        {
            float x = -3f + i * 3f;
            GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stripe.name = $"AccentStripe{i}";
            stripe.transform.SetParent(parent);
            stripe.transform.position = new Vector3(x, 0.05f, 1.5f);
            stripe.transform.localScale = new Vector3(0.15f, 0.02f, 6f);
            Object.Destroy(stripe.GetComponent<Collider>());

            Renderer renderer = stripe.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = CreateSafeMaterial(stripe.name, new Color(0.15f, 0.35f, 0.55f, 1f));
        }
    }

    private static Material CreateSafeMaterial(string name, Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
            shader = Shader.Find("Sprites/Default");
        if (shader == null)
            shader = Shader.Find("Unlit/Color");
        if (shader == null)
            shader = Shader.Find("Standard");

        Material material = new Material(shader)
        {
            name = $"Battle_{name}_Material"
        };

        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color"))
            material.SetColor("_Color", color);

        return material;
    }
}
