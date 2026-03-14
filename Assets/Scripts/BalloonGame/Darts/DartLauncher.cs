using UnityEngine;

[DisallowMultipleComponent]
public class DartLauncher : MonoBehaviour
{
    private static Material _bodyMaterial;
    private static Material _tipMaterial;
    private ObjectPool _dartPool;
    private GameObject _dartPrefab;
    private PerkManager _perkManager;

    public DartController SpawnAndLaunch(Vector3 velocity)
    {
        EnsurePool();
        GameObject dart = _dartPool.Get();
        if (dart == null)
        {
            return null;
        }

        dart.transform.position = GameConstants.LAUNCH_POSITION;
        dart.transform.rotation = Quaternion.identity;

        var controller = dart.GetComponent<DartController>();
        controller.Initialize(
            _dartPool,
            _perkManager != null ? _perkManager.AdditionalPierce : 0,
            _perkManager != null ? _perkManager.AdditionalRicochet : 0);
        controller.Launch(velocity);
        return controller;
    }

    public void SetPerkManager(PerkManager perkManager)
    {
        _perkManager = perkManager;
    }

    private void EnsurePool()
    {
        if (_dartPool != null)
        {
            return;
        }

        _dartPrefab = CreateDartObject();
        _dartPrefab.SetActive(false);
        _dartPool = gameObject.AddComponent<ObjectPool>();
        _dartPool.Configure(_dartPrefab, GameConstants.DEFAULT_POOL_SIZE);
    }

    private GameObject CreateDartObject()
    {
        var root = new GameObject("DartPrefab");
        root.layer = GameConstants.LAYER_PROJECTILES;

        var body = new GameObject("Body");
        body.name = "Body";
        body.transform.SetParent(root.transform, false);
        body.layer = GameConstants.LAYER_PROJECTILES;
        body.AddComponent<MeshFilter>().sharedMesh = DartMeshGenerator.GetBodyMesh();
        body.AddComponent<MeshRenderer>().material = GetBodyMaterial();

        var tip = new GameObject("Tip");
        tip.name = "Tip";
        tip.transform.SetParent(root.transform, false);
        tip.transform.localPosition = new Vector3(0.36f, 0f, 0f);
        tip.layer = GameConstants.LAYER_PROJECTILES;
        tip.AddComponent<MeshFilter>().sharedMesh = DartMeshGenerator.GetTipMesh();
        tip.AddComponent<MeshRenderer>().material = GetTipMaterial();

        var collider = root.AddComponent<CapsuleCollider>();
        collider.direction = 0;
        collider.center = Vector3.zero;
        collider.radius = 0.06f;
        collider.height = 0.6f;

        var ricochetAnchor = new GameObject("RicochetAnchor");
        ricochetAnchor.transform.SetParent(root.transform, false);
        ricochetAnchor.layer = GameConstants.LAYER_RICOCHET;
        var ricochetCollider = ricochetAnchor.AddComponent<SphereCollider>();
        ricochetCollider.isTrigger = true;
        ricochetCollider.radius = 0.22f;
        ricochetCollider.enabled = false;

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
        root.AddComponent<DartTrailVFX>();
        return root;
    }

    private static Material GetBodyMaterial()
    {
        if (_bodyMaterial != null)
        {
            return _bodyMaterial;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        Color chromeBody = new(0.78f, 0.80f, 0.84f);
        _bodyMaterial = new Material(shader) { color = chromeBody };

        if (_bodyMaterial.HasProperty("_BaseColor"))
            _bodyMaterial.SetColor("_BaseColor", chromeBody);
        if (_bodyMaterial.HasProperty("_Glossiness"))
            _bodyMaterial.SetFloat("_Glossiness", 0.92f);
        if (_bodyMaterial.HasProperty("_Smoothness"))
            _bodyMaterial.SetFloat("_Smoothness", 0.92f);
        if (_bodyMaterial.HasProperty("_Metallic"))
            _bodyMaterial.SetFloat("_Metallic", 0.85f);

        return _bodyMaterial;
    }

    private static Material GetTipMaterial()
    {
        if (_tipMaterial != null)
        {
            return _tipMaterial;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        Color chromeTip = new(0.92f, 0.93f, 0.95f);
        _tipMaterial = new Material(shader) { color = chromeTip };

        if (_tipMaterial.HasProperty("_BaseColor"))
            _tipMaterial.SetColor("_BaseColor", chromeTip);
        if (_tipMaterial.HasProperty("_Glossiness"))
            _tipMaterial.SetFloat("_Glossiness", 0.95f);
        if (_tipMaterial.HasProperty("_Smoothness"))
            _tipMaterial.SetFloat("_Smoothness", 0.95f);
        if (_tipMaterial.HasProperty("_Metallic"))
            _tipMaterial.SetFloat("_Metallic", 0.9f);

        return _tipMaterial;
    }
}
