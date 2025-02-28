using UnityEngine;

public static class Rigidbody2DExt
{
    public static void AddExplosionForce(this Rigidbody2D body, float explosionForce, Vector3 explosionPosition, float explosionRadius, ForceMode2D mode = ForceMode2D.Force)

    {

        var dir = (body.transform.position - explosionPosition);

        float wearoff = 1 - (dir.magnitude / explosionRadius);

        // Debug.Log(dir.normalized * (wearoff <= 0f ? 0f : explosionForce) * wearoff);

        body.AddForce(dir.normalized * (wearoff <= 0f ? 0f : explosionForce) * wearoff, mode);

    }


    public static void AddExplosionForce(this Rigidbody2D body, float explosionForce, Vector3 explosionPosition, float explosionRadius, float upliftModifier, ForceMode2D mode = ForceMode2D.Force)

    {

        var dir = (body.transform.position - explosionPosition);

        float wearoff = 1 - (dir.magnitude / explosionRadius);

        Vector3 baseForce = dir.normalized * (wearoff <= 0f ? 0f : explosionForce) * wearoff;

        body.AddForce(baseForce);


        float upliftWearoff = 1 - upliftModifier / explosionRadius;

        Vector3 upliftForce = Vector2.up * explosionForce * upliftWearoff;

        body.AddForce(upliftForce, mode);

    }
}