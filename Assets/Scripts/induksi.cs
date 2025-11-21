using UnityEngine;
using TMPro;

[System.Serializable]
public class KumparanData
{
    public Transform coil;
    public int lilitan = 1;
    public float area = 1.0f;


    [HideInInspector]
    public float lastFlux;

    [HideInInspector]
    public bool aktif = true; // ditambahkan
}




public class induksi : MonoBehaviour
{
    [Header("GameObject Kumparan")]
    public GameObject Kumparan;
    public GameObject Kumparan2;

    [Header("Magnet dan Kutub")]
    public Transform magnet;
    public Transform sPole;
    public Transform nPole;

    [Header("Data Kumparan")]
    public KumparanData[] kumparanArray;

    [Header("Fluks Sampling")]
    public float coilWidth = 1.0f; // bisa disesuaikan dengan ukuran sprite kumparan


    [Header("UI dan Visual")]
    public TextMeshProUGUI outputText;
    public SpriteRenderer lampuRenderer;
    public SpriteRenderer glowRenderer;
    public Transform jarumGalvanometer;
    public Transform panahArus;

    [Header("Parameter")]
    public float fieldStrength = 50f;
    public float smoothing = 5f;
    public float maxSudut = 90f;
    public float sensitivity = 30f;
    public Color matiColor = Color.gray;
    public Color nyalaColor = Color.yellow;

    [Header("Garis Medan Magnet")]
    public GameObject fieldLinesObject;


    private float displayVoltage;
    private int modeAktif = 3;
    private bool flippedX = false;
    private bool flippedY = false;
    private bool arahMedanTerbalik = false;
    private Vector3 posisiMagnetTerakhir;




    void Start()
    {
        float totalVoltage = 0f;
        posisiMagnetTerakhir = magnet.position;

        foreach (var k in kumparanArray)
        {
            float currentFlux = HitungFluks(k);
            float deltaFlux = currentFlux - k.lastFlux;
            float epsilon = -k.lilitan * deltaFlux / Time.deltaTime;
            totalVoltage += epsilon;

            k.lastFlux = HitungFluks(k);
            k.lastFlux = currentFlux;
            k.aktif = false;
        }
        SetMode(1);
    }

    void Update()
    {
        float totalVoltage = 0f;

        for (int i = 0; i < kumparanArray.Length; i++)
        {
            var k = kumparanArray[i];
            if (!k.aktif) continue;

            float currentFlux = HitungFluks(k);
            float deltaFlux = currentFlux - k.lastFlux;
            float epsilon = -k.lilitan * deltaFlux / Time.deltaTime;



            Debug.Log($"Kumparan {i + 1} | Flux: {currentFlux:F3} | dFlux: {deltaFlux:F3} | ε: {epsilon:F3}");

            totalVoltage += epsilon;
            k.lastFlux = currentFlux;
        }


        displayVoltage = Mathf.Lerp(displayVoltage, totalVoltage, 1 - Mathf.Exp(-smoothing * Time.deltaTime));

        float intensitas = Mathf.Clamp01(Mathf.Abs(displayVoltage) / 5f);

        if (lampuRenderer != null)
            lampuRenderer.color = Color.Lerp(matiColor, nyalaColor, intensitas);
        SetGlowAlpha(intensitas);

        // Jarum galvanometer
        if (jarumGalvanometer != null)
        {
            float sudut = Mathf.Clamp(displayVoltage * sensitivity, -maxSudut, maxSudut);
            Quaternion targetRot = Quaternion.Euler(0, 0, sudut);
            jarumGalvanometer.rotation = Quaternion.Lerp(jarumGalvanometer.rotation, targetRot, 5f * Time.deltaTime);
        }

        // Panah arah arus
        if (panahArus != null)
        {
            float arah = Mathf.Sign(displayVoltage);
            panahArus.localScale = new Vector3(arah, 1, 1);
            panahArus.gameObject.SetActive(Mathf.Abs(displayVoltage) > 0.01f);
        }

    }

    float HitungFluks(KumparanData k)
    {
        Vector2 posisiMagnet = magnet.position;
        Vector2 posisiKumparan = k.coil.position;

        float jarak = Vector2.Distance(posisiMagnet, posisiKumparan);

        // Hindari pembagian oleh nol atau jarak sangat kecil
        float jarakAman = Mathf.Max(jarak, 0.5f);

        // Alignment magnet terhadap arah ke kumparan
        Vector2 arahMagnet = (sPole.position - nPole.position).normalized;
        Vector2 arahKeKumparan = (posisiKumparan - posisiMagnet).normalized;
        float alignment = Vector2.Dot(arahMagnet, arahKeKumparan);

        // Gunakan hukum kuadrat terbalik untuk realistis penurunan intensitas
        float B = (fieldStrength * alignment) / (jarakAman * jarakAman);

        float flux = B * k.area;
        return flux;
    }




    void SetGlowAlpha(float alpha)
    {
        if (glowRenderer != null)
        {
            Color c = glowRenderer.color;
            c.a = alpha;
            glowRenderer.color = c;
        }
    }
    public void SetMode(int mode)
    {
        modeAktif = mode;

        for (int i = 0; i < kumparanArray.Length; i++)
        {
            bool aktifSekarang = false;
            if (i == 0)
                aktifSekarang = mode == 1 || mode == 3;
            else if (i == 1)
                aktifSekarang = mode == 2 || mode == 3;


            kumparanArray[i].aktif = aktifSekarang;

            // Aktifkan GameObject visual
            if (i == 0 && Kumparan != null)
                Kumparan.SetActive(aktifSekarang);
            if (i == 1 && Kumparan2 != null)
                Kumparan2.SetActive(aktifSekarang);

            // Reset flux jika tidak aktif
            if (aktifSekarang)
                kumparanArray[i].lastFlux = HitungFluks(kumparanArray[i]);
            else
                kumparanArray[i].lastFlux = 0f;

            kumparanArray[i].lastFlux = aktifSekarang ? HitungFluks(kumparanArray[i]) : 0f;
        }
    }


    public void BalikkanMagnet()
    {
        arahMedanTerbalik = !arahMedanTerbalik;

        // Flip visual tampilan (boleh tetap pakai)
        flippedX = !flippedX;
        flippedY = !flippedY;

        Vector3 scale = magnet.localScale;
        scale.x = flippedX ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        scale.y = flippedY ? -Mathf.Abs(scale.y) : Mathf.Abs(scale.y);
        magnet.localScale = scale;
    }

    public void ToggleFieldLines(bool aktif)
    {
        Debug.Log("Toggle garis medan: " + aktif);

        if (fieldLinesObject != null)
            fieldLinesObject.SetActive(aktif);
    }





}
