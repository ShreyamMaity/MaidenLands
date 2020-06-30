using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BulletShell : MonoBehaviour
{
    [System.Serializable]
    public struct ShellSettings
    {
        public float forceMin;  // 90f
        public float forceMax;  // 120f;

        public float lifetime;  // 4;
        public float fadetime;  //2;

        public int forceDirection; //-1

        public ShellSettings(float useLessParam = 00f)
        {
            forceMin = 90f;
            forceMax = 120f;
            lifetime = 4f;
            fadetime = 2f;
            forceDirection = -1;
        }
    }

    public ShellSettings shellSettings = new ShellSettings();

    // private
    Rigidbody rigidBody;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();

        float force = Random.Range(shellSettings.forceMin, shellSettings.forceMax);
        rigidBody.AddForce(transform.right * force * shellSettings.forceDirection);
        // rigidBody.AddTorque(Random.insideUnitSphere * force);

        StartCoroutine(Fade());
    }


    IEnumerator Fade()
    {
        yield return new WaitForSeconds(shellSettings.lifetime);

        float percent = 0;
        float fadeSpeed = 1 / shellSettings.fadetime;
        Material mat = GetComponent<Renderer>().material;
        Color initialColour = mat.color;

        while (percent < 1)
        {
            percent += Time.deltaTime * fadeSpeed;
            mat.color = Color.Lerp(initialColour, Color.clear, percent);
            yield return null;
        }

        Destroy(gameObject);
    }
}
