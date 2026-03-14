using UnityEngine;

public class DartLauncher : MonoBehaviour
{
    private static Material _bodyMaterial;
    private static Material _tipMaterial;

    public DartController SpawnAndLaunch(Vector3 velocity)
    {
        GameObject dart = CreateDartObject();
        dart.transform.position = GameConstants.LAUNCH_POSITION;

        var controller = dart.GetComponent<DartController>();
        controller.Launch(velocity);
        return controller;
    }

    private GameObject CreateDartObject()
    {
        var root = new GameObject("Dart");
        root.layer = GameConstants.LAYER_PROJECTILES;

        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(root.transform, false);
        body.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        body.transform.localScale = new Vector3(0.12f, 0.3f, 0.12f);
        body.layer = GameConstants.LAYER_PROJECTILES;
        Object.Destroy(body.GetComponent<CapsuleCollider>());
        body.GetComponent<Renderer>().material = GetBodyMaterial();

        var tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tip.name = "Tip";
        tip.transform.SetParent(root.transform, false);
        tip.transform.localPosition = new Vector3(0.28f, 0f, 0f);
        tip.transform.localScale = new Vector3(0.08f, 0.08f, 0.15f);
        tip.layer = GameConstants.LAYER_PROJECTILES;
        Object.Destroy(tip.GetComponent<SphereCollider>());
        tip.GetComponent<Renderer>().material = GetTipMaterial();

        var collider = root.AddComponent<CapsuleCollider>();
        collider.direction = 0;
        collider.center = Vector3.zero;
        collider.radius = 0.06f;
        collider.height = 0.6f;

        var rigidbody = root.AddComponent<Rigidbody>();
        rigidbody.mass = 0.5f;
        rigidbody.linearDamping = 0.1f;
        rigidbody.angularDamping = 0.5f;
        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;
        rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rigidbody.constraints = RigidbodyConstraints.FreezePositionZ |
                                 RigidbodyConstraints.FreezeRotationX |
                                 RigidbodyConstraints.FreezeRotationY;

        root.AddComponent<DartController>();
        return root;
    }

    private static Material GetBodyMaterial()
    {
        if (_bodyMaterial != null)
        {
            return _bodyMaterial;
        }

        Shader shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit");
        _bodyMaterial = new Material(shader)
        {
            color = new Color(0.7f, 0.72f, 0.75f)
        };

        if (_bodyMaterial.HasProperty("_Glossiness"))
        {
            _bodyMaterial.SetFloat("_Glossiness", 0.8f);
        }

        if (_bodyMaterial.HasProperty("_Smoothness"))
        {
            _bodyMaterial.SetFloat("_Smoothness", 0.8f);
        }

        if (_bodyMaterial.HasProperty("_Metallic"))
        {
            _bodyMaterial.SetFloat("_Metallic", 0.6f);
        }

        return _bodyMaterial;
    }

    private static Material GetTipMaterial()
    {
        if (_tipMaterial != null)
        {
            return _tipMaterial;
        }

        Shader shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit");
        _tipMaterial = new Material(shader)
        {
            color = new Color(0.85f, 0.85f, 0.85f)
        };

        if (_tipMaterial.HasProperty("_Glossiness"))
        {
            _tipMaterial.SetFloat("_Glossiness", 0.9f);
        }

        if (_tipMaterial.HasProperty("_Smoothness"))
        {
            _tipMaterial.SetFloat("_Smoothness", 0.9f);
        }

        if (_tipMaterial.HasProperty("_Metallic"))
        {
            _tipMaterial.SetFloat("_Metallic", 0.7f);
        }

        return _tipMaterial;
    }
}
