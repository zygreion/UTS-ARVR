#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Scene1Builder: Tool editor otomatis untuk membangun scene Galeri Pengenalan Kampus (Sub-CPMK 1).
/// Membangun maket akurat Gedung Rektorat UPN "Veteran" Jakarta (Kampus Bela Negara) sesuai foto referensi:
/// - Fasad portal agung berundak (stepped crown) & pilar ground colonnade
/// - Dinding kaca tirai dengan tipografi "UPNVJ KAMPUS BELA NEGARA"
/// - Panel motif geometris origami segitiga khas di kiri & kanan
/// - Layar Videotron/Megatron tengah berbingkai hijau Bela Negara
/// - Tiang Bendera Merah Putih berkibar di atas fondasi berundak
/// - Marka jalan biru "TURUN PENUMPANG / DROP OFF"
/// - Badan struktural utama dibalut material WARNA_UTAMA (#511069) sesuai syarat UTS
/// </summary>
public static class Scene1Builder
{
    private const string Scene1Path = "Assets/Scenes/Scene 1_Fondasi Scene dan Jalur Input-Output.unity";
    private const string LabUtamaPath = "Assets/Scenes/Lab_Utama.unity";

    [MenuItem("Tools/ARVR UTS/1. Build Scene 1 (Fondasi Input-Output)")]
    public static void BuildScene1Only()
    {
        BuildGalleryScene(Scene1Path);
        EditorUtility.DisplayDialog("Sukses", "Scene 1 (Gedung Rektorat UPNVJ) berhasil dibangun dan disimpan!", "OK");
    }

    [MenuItem("Tools/ARVR UTS/2. Build & Sinkronkan ke Keduanya (Scene 1 & Lab_Utama)")]
    public static void BuildAndSyncBoth()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Peringatan", "Matikan mode Play (klik tombol ▶️ di atas) terlebih dahulu sebelum melakukan build scene!", "OK");
            return;
        }

        BuildAndSyncBothQuiet();
        EditorUtility.DisplayDialog("Sukses", "Kedua scene (Scene 1 & Lab_Utama) berhasil dibangun dengan Fasad Rektorat UPNVJ!", "OK");
    }

    public static void BuildAndSyncBothQuiet()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("[Scene1Builder] Lewati auto-build karena Unity sedang dalam mode Play.");
            return;
        }

        BuildGalleryScene(Scene1Path);

        if (File.Exists(Scene1Path))
        {
            AssetDatabase.CopyAsset(Scene1Path, LabUtamaPath);
            AssetDatabase.Refresh();
        }

        Debug.Log("[Scene1Builder] Sukses: Scene 1 dan Lab_Utama berhasil dibangun & disinkronkan dengan Fasad Rektorat UPNVJ!");
    }

    public static void BuildGalleryScene(string targetScenePath)
    {
        var scene = EditorSceneManager.OpenScene(targetScenePath, OpenSceneMode.Single);

        // Hapus struktur lama agar bersih
        GameObject existingRoot = GameObject.Find("[Galeri_Pengenalan_Kampus]");
        if (existingRoot != null) Object.DestroyImmediate(existingRoot);

        GameObject existingCanvas = GameObject.Find("Canvas_IOMonitor");
        if (existingCanvas != null) Object.DestroyImmediate(existingCanvas);

        GameObject existingEventSystem = GameObject.Find("EventSystem");
        if (existingEventSystem != null) Object.DestroyImmediate(existingEventSystem);

        // 1. MATERIAL PALETTE (Fasad UPNVJ + Galeri Modern)
        Material matWarnaUtama = GetOrCreateMaterial("Assets/Materials/Mat Warna Utama.mat", new Color(0.3176f, 0.0627f, 0.4117f, 1f), 0.45f); // #511069 (Syarat NIM)
        Material matFloor = GetOrCreateMaterial("Assets/Materials/Mat Floor.mat", new Color(0.12f, 0.13f, 0.15f, 1f), 0.65f); // Lantai marmer gelap galeri
        Material matWall = GetOrCreateMaterial("Assets/Materials/Mat Wall.mat", new Color(0.86f, 0.86f, 0.84f, 1f), 0.1f);   // Dinding galeri hangat
        Material matCeiling = GetOrCreateMaterial("Assets/Materials/Mat Ceiling.mat", new Color(0.08f, 0.08f, 0.10f, 1f), 0.1f); // Plafon indoor gelap
        Material matPedestal = GetOrCreateMaterial("Assets/Materials/Mat Pedestal.mat", new Color(0.10f, 0.10f, 0.12f, 1f), 0.6f); // Meja pameran hitam obsidian
        Material matWhiteTrim = GetOrCreateMaterial("Assets/Materials/Mat White Trim.mat", new Color(0.96f, 0.96f, 0.98f, 1f), 0.3f); // Marmer putih / trim
        Material matGlass = GetOrCreateMaterial("Assets/Materials/Mat Glass.mat", new Color(0.20f, 0.42f, 0.55f, 0.85f), 0.95f); // Kaca reflektif tirai UPNVJ
        Material matSilverACP = GetOrCreateMaterial("Assets/Materials/Mat Silver ACP.mat", new Color(0.82f, 0.88f, 0.92f, 1f), 0.65f); // Panel ACP portal UPNVJ
        Material matBelaNegara = GetOrCreateMaterial("Assets/Materials/Mat Bela Negara Green.mat", new Color(0.11f, 0.42f, 0.22f, 1f), 0.3f); // Hijau Bela Negara
        Material matPillarGray = GetOrCreateMaterial("Assets/Materials/Mat Pillar Gray.mat", new Color(0.25f, 0.28f, 0.33f, 1f), 0.35f); // Pilar abu-abu UPNVJ
        Material matFlagRed = GetOrCreateMaterial("Assets/Materials/Mat Flag Red.mat", new Color(0.85f, 0.05f, 0.05f, 1f), 0.2f); // Merah bendera
        Material matFlagWhite = GetOrCreateMaterial("Assets/Materials/Mat Flag White.mat", new Color(0.98f, 0.98f, 0.98f, 1f), 0.2f); // Putih bendera
        Material matBluePaving = GetOrCreateMaterial("Assets/Materials/Mat Blue Paving.mat", new Color(0.0f, 0.52f, 0.95f, 1f), 0.35f); // Marka jalan drop off biru
        Material matBrass = GetOrCreateMaterial("Assets/Materials/Mat Brass.mat", new Color(0.85f, 0.70f, 0.25f, 1f), 0.75f, true, new Color(0.3f, 0.25f, 0.05f)); // Tiang pembatas pameran
        Material matPath = GetOrCreateMaterial("Assets/Materials/Mat Path.mat", new Color(0.24f, 0.26f, 0.30f, 1f), 0.3f); // Jalur pengunjung
        Material matMarker = GetOrCreateMaterial("Assets/Materials/Mat Marker.mat", new Color(1.0f, 0.68f, 0.0f, 1f), 0.9f, true, new Color(1.0f, 0.60f, 0.0f) * 1.6f); // Kristal penanda

        // 2. ROOT GAME OBJECT
        GameObject root = new GameObject("[Galeri_Pengenalan_Kampus]");

        // 3. ARSITEKTUR RUANG GALERI INDOOR (Panjang 20m, Lebar 16m, Tinggi 5.8m)
        // Lantai Galeri
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Lantai_Galeri";
        floor.transform.parent = root.transform;
        floor.transform.position = new Vector3(0f, -0.1f, 0f);
        floor.transform.localScale = new Vector3(20f, 0.2f, 18f);
        floor.GetComponent<Renderer>().sharedMaterial = matFloor;

        // Plafon Langit-langit (Menutup skybox kartun agar suasana indoor nyata)
        GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ceiling.name = "Plafon_LangitLangit";
        ceiling.transform.parent = root.transform;
        ceiling.transform.position = new Vector3(0f, 5.6f, 0f);
        ceiling.transform.localScale = new Vector3(20f, 0.2f, 18f);
        ceiling.GetComponent<Renderer>().sharedMaterial = matCeiling;

        // Dinding Galeri
        CreateWall(root.transform, "Dinding_Utara", new Vector3(0f, 2.7f, 9f), new Vector3(20f, 5.6f, 0.4f), matWall);
        CreateWall(root.transform, "Dinding_Timur", new Vector3(10f, 2.7f, 0f), new Vector3(0.4f, 5.6f, 18f), matWall);
        CreateWall(root.transform, "Dinding_Barat", new Vector3(-10f, 2.7f, 0f), new Vector3(0.4f, 5.6f, 18f), matWall);
        CreateWall(root.transform, "Dinding_Selatan_Kiri", new Vector3(-7f, 2.7f, -9f), new Vector3(6f, 5.6f, 0.4f), matWall);
        CreateWall(root.transform, "Dinding_Selatan_Kanan", new Vector3(7f, 2.7f, -9f), new Vector3(6f, 5.6f, 0.4f), matWall);
        CreateWall(root.transform, "Dinding_Selatan_PintuAtas", new Vector3(0f, 4.6f, -9f), new Vector3(8f, 1.8f, 0.4f), matWall);

        // Baseboard / Lis Bawah Dinding
        CreateWall(root.transform, "Baseboard_Utara", new Vector3(0f, 0.12f, 8.85f), new Vector3(19.6f, 0.25f, 0.1f), matPedestal);
        CreateWall(root.transform, "Baseboard_Timur", new Vector3(9.85f, 0.12f, 0f), new Vector3(0.1f, 0.25f, 17.6f), matPedestal);
        CreateWall(root.transform, "Baseboard_Barat", new Vector3(-9.85f, 0.12f, 0f), new Vector3(0.1f, 0.25f, 17.6f), matPedestal);

        // 4. JALUR PENGUNJUNG (Visitor Walkway)
        GameObject path = GameObject.CreatePrimitive(PrimitiveType.Cube);
        path.name = "Jalur_Pengunjung";
        path.transform.parent = root.transform;
        path.transform.position = new Vector3(0f, 0.02f, -3.8f);
        path.transform.localScale = new Vector3(2.6f, 0.04f, 10.0f);
        path.GetComponent<Renderer>().sharedMaterial = matPath;

        // 5. POSTER & PANEL PAMERAN DI DINDING UTARA
        CreateWallPoster(root.transform, "Poster_ProfilUPNVJ", new Vector3(-5.2f, 3.0f, 8.78f), new Vector3(3.2f, 2.0f, 0.05f),
            "<b>UNIVERSITAS PEMBANGUNAN NASIONAL\n“VETERAN” JAKARTA</b>\n\n• Kampus Bela Negara\n• Fakultas Ilmu Komputer\n• Program Studi S1 Informatika\n• Didirikan Sejak 1958", matPedestal);

        CreateWallPoster(root.transform, "Poster_VisiMisi", new Vector3(5.2f, 3.0f, 8.78f), new Vector3(3.2f, 2.0f, 0.05f),
            "<b>VISI KAMPUS BELA NEGARA</b>\n\nMenjadi Perguruan Tinggi Negeri\nUnggul Berkarakter Bela Negara\ndan Berdaya Saing Global\npada Tingkat Internasional.", matPedestal);

        // 6. MEJA PAMERAN (Pedestal Plinth Luas untuk Memuat Fasad Rektorat)
        GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pedestal.name = "Meja_Pameran_Plinth";
        pedestal.transform.parent = root.transform;
        pedestal.transform.position = new Vector3(0f, 0.42f, 1.5f);
        pedestal.transform.localScale = new Vector3(6.4f, 0.85f, 4.2f);
        pedestal.GetComponent<Renderer>().sharedMaterial = matPedestal;

        // Lis Emas Tepian Meja Pameran
        GameObject pedestalTrim = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pedestalTrim.name = "Meja_Trim_Emas";
        pedestalTrim.transform.parent = pedestal.transform;
        pedestalTrim.transform.localPosition = new Vector3(0f, 0.48f, 0f);
        pedestalTrim.transform.localScale = new Vector3(1.01f, 0.05f, 1.01f);
        pedestalTrim.GetComponent<Renderer>().sharedMaterial = matBrass;

        // 7. TIANG PEMBATAS PAMERAN (Museum Stanchions & Velvet Ropes)
        CreateStanchion(root.transform, new Vector3(-3.5f, 0f, -0.8f), matBrass);
        CreateStanchion(root.transform, new Vector3(3.5f, 0f, -0.8f), matBrass);
        CreateStanchion(root.transform, new Vector3(-3.5f, 0f, 3.8f), matBrass);
        CreateStanchion(root.transform, new Vector3(3.5f, 0f, 3.8f), matBrass);

        // 8. OBJEK UTAMA: MAKET GEDUNG REKTORAT UPNVJ (Sesuai Foto Asli)
        GameObject maketGedung = CreateGedungRektoratUPNVJ(root.transform, matWarnaUtama, matSilverACP, matGlass, matBelaNegara, matPillarGray, matFlagRed, matFlagWhite, matBluePaving, matWhiteTrim, matPedestal);

        // 9. PENANDA LOKASI (Waypoint Hologram Berlian Emas Tepat di Atas Mahkota Gedung)
        GameObject markerRoot = new GameObject("Penanda_Lokasi_Waypoint");
        markerRoot.transform.parent = root.transform;
        markerRoot.transform.position = new Vector3(0f, 3.85f, 1.45f);

        GameObject gem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        gem.name = "Diamond_Gem";
        gem.transform.parent = markerRoot.transform;
        gem.transform.localPosition = Vector3.zero;
        gem.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
        gem.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
        gem.GetComponent<Renderer>().sharedMaterial = matMarker;

        SphereCollider markerCol = markerRoot.AddComponent<SphereCollider>();
        markerCol.radius = 0.5f;

        // 10. PAPAN INFORMASI INTERAKTIF (Kiosk Touchscreen Modern di Sisi Kanan Depan)
        GameObject kiosk = new GameObject("Papan_Informasi_Kiosk");
        kiosk.transform.parent = root.transform;
        kiosk.transform.position = new Vector3(3.2f, 0f, -1.0f);
        // Hadapkan bezel layar kiosk (sisi lokal -Z) tepat ke arah pemain di jalur pengunjung
        Vector3 dirToPlayer = (new Vector3(0f, 0f, -4.2f) - kiosk.transform.position).normalized;
        kiosk.transform.rotation = Quaternion.LookRotation(-dirToPlayer);

        // Kiosk Base & Tiang
        GameObject kioskBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        kioskBase.name = "Kiosk_Base";
        kioskBase.transform.parent = kiosk.transform;
        kioskBase.transform.localPosition = new Vector3(0f, 0.05f, 0f);
        kioskBase.transform.localScale = new Vector3(1.0f, 0.1f, 0.7f);
        kioskBase.GetComponent<Renderer>().sharedMaterial = matPedestal;

        GameObject kioskPillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        kioskPillar.name = "Kiosk_Pillar";
        kioskPillar.transform.parent = kiosk.transform;
        kioskPillar.transform.localPosition = new Vector3(0f, 0.6f, 0f);
        kioskPillar.transform.localScale = new Vector3(0.35f, 1.1f, 0.25f);
        kioskPillar.GetComponent<Renderer>().sharedMaterial = matPedestal;

        // Bezel Layar Miring 30 Derajat Mengarah ke Atas Menghadap Pemain
        GameObject kioskHead = GameObject.CreatePrimitive(PrimitiveType.Cube);
        kioskHead.name = "Kiosk_Screen_Bezel";
        kioskHead.transform.parent = kiosk.transform;
        kioskHead.transform.localPosition = new Vector3(0f, 1.25f, -0.1f);
        kioskHead.transform.localRotation = Quaternion.Euler(30f, 0f, 0f);
        kioskHead.transform.localScale = new Vector3(1.35f, 0.9f, 0.08f);
        kioskHead.GetComponent<Renderer>().sharedMaterial = matPedestal;

        // World-Space Canvas untuk Teks Kiosk
        GameObject kioskCanvasObj = new GameObject("Kiosk_World_Canvas");
        kioskCanvasObj.transform.parent = kioskHead.transform;
        kioskCanvasObj.transform.localPosition = new Vector3(0f, 0f, -0.52f);
        kioskCanvasObj.transform.localRotation = Quaternion.identity;
        kioskCanvasObj.transform.localScale = new Vector3(0.0016f, 0.0016f, 0.0016f);

        Canvas kioskCanvas = kioskCanvasObj.AddComponent<Canvas>();
        kioskCanvas.renderMode = RenderMode.WorldSpace;
        kioskCanvasObj.AddComponent<CanvasScaler>();

        RectTransform kioskRect = kioskCanvasObj.GetComponent<RectTransform>();
        kioskRect.sizeDelta = new Vector2(780f, 500f);

        GameObject screenBg = new GameObject("Screen_Background");
        screenBg.transform.SetParent(kioskCanvasObj.transform, false);
        Image bgImg = screenBg.AddComponent<Image>();
        bgImg.color = new Color(0.06f, 0.09f, 0.14f, 0.95f);
        RectTransform bgRect = screenBg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        GameObject kioskTextObj = new GameObject("Kiosk_Text_TMP");
        kioskTextObj.transform.SetParent(screenBg.transform, false);
        RectTransform kioskTextRect = kioskTextObj.AddComponent<RectTransform>();
        kioskTextRect.anchorMin = Vector2.zero;
        kioskTextRect.anchorMax = Vector2.one;
        kioskTextRect.offsetMin = new Vector2(24f, 24f);
        kioskTextRect.offsetMax = new Vector2(-24f, -24f);

        TextMeshProUGUI kioskTmp = kioskTextObj.AddComponent<TextMeshProUGUI>();
        kioskTmp.fontSize = 23f;
        kioskTmp.color = Color.white;
        kioskTmp.richText = true;
        kioskTmp.enableWordWrapping = true;
        kioskTmp.alignment = TextAlignmentOptions.TopLeft;
        kioskTmp.text = "<b><color=#FFD700><size=115%>GALERI PENGENALAN KAMPUS</size></color></b>\n" +
                        "<size=78%><color=#8FA0B5>Gedung Rektorat UPN “Veteran” Jakarta (Sub-CPMK 1)</color></size>\n" +
                        "─────────────────────────────────────\n" +
                        "<b>Objek Utama:</b> Maket Rektorat Kampus Bela Negara\n" +
                        "<b>Material Acuan:</b> WARNA_UTAMA (<color=#DDA0DD>#511069</color>)\n" +
                        "<b>Identitas NIM:</b> 2310511069\n\n" +
                        "<b>KENDALI JALUR MASUKAN (INPUT):</b>\n" +
                        "• <b>[Klik Kiri]:</b> Interaksi Maket Rektorat\n" +
                        "• <b>[Spasi]:</b> Toggle Rotasi Showcase Pameran\n" +
                        "• <b>[Tombol E]:</b> Saklar Mode Lampu Galeri\n" +
                        "• <b>[W,A,S,D]:</b> Navigasi Pengunjung Virtual";

        // 11. SISTEM PENCAHAYAAN GALERI
        Light dirLight = Object.FindObjectOfType<Light>();
        if (dirLight == null || dirLight.type != LightType.Directional)
        {
            GameObject dlObj = new GameObject("Directional Light");
            dirLight = dlObj.AddComponent<Light>();
            dirLight.type = LightType.Directional;
        }
        dirLight.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
        dirLight.intensity = 0.85f;
        dirLight.color = new Color(0.92f, 0.94f, 1.0f);
        dirLight.shadows = LightShadows.Soft;

        // Lampu Plafon Ruangan Galeri (Menyinari seluruh interior ruangan)
        GameObject roomLightObj = new GameObject("Lampu_Ruangan_Galeri");
        roomLightObj.transform.parent = root.transform;
        roomLightObj.transform.position = new Vector3(0f, 4.8f, -1.0f);
        Light roomLight = roomLightObj.AddComponent<Light>();
        roomLight.type = LightType.Point;
        roomLight.range = 28f;
        roomLight.intensity = 1.4f;
        roomLight.color = new Color(0.95f, 0.96f, 1.0f);
        roomLight.shadows = LightShadows.None;

        // Lampu Sorot Pameran Utama (Menyorot fasad depan Rektorat)
        GameObject spotObj = new GameObject("Lampu_Sorot_Maket");
        spotObj.transform.parent = root.transform;
        spotObj.transform.position = new Vector3(0f, 5.2f, 0.1f);
        spotObj.transform.rotation = Quaternion.Euler(62f, 0f, 0f);

        Light spotLight = spotObj.AddComponent<Light>();
        spotLight.type = LightType.Spot;
        spotLight.spotAngle = 72f;
        spotLight.innerSpotAngle = 50f;
        spotLight.range = 10.0f;
        spotLight.intensity = 2.0f;
        spotLight.color = new Color(1.0f, 0.97f, 0.90f);
        spotLight.shadows = LightShadows.Soft;

        // Spotlight untuk Kiosk
        GameObject kioskSpot = new GameObject("Spotlight_Kiosk");
        kioskSpot.transform.parent = root.transform;
        kioskSpot.transform.position = new Vector3(3.2f, 4.6f, -1.0f);
        kioskSpot.transform.rotation = Quaternion.Euler(80f, 0f, 0f);
        Light kSpot = kioskSpot.AddComponent<Light>();
        kSpot.type = LightType.Spot;
        kSpot.spotAngle = 45f;
        kSpot.range = 6.0f;
        kSpot.intensity = 1.2f;
        kSpot.color = new Color(0.9f, 0.95f, 1.0f);

        // 12. VIEWER / KAMERA UTAMA (Perspektif Depan Fasad)
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            mainCam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
        }

        mainCam.transform.parent = root.transform;
        mainCam.transform.position = new Vector3(0f, 1.6f, -4.2f);
        mainCam.transform.rotation = Quaternion.Euler(8f, 0f, 0f);
        mainCam.fieldOfView = 60f;
        mainCam.clearFlags = CameraClearFlags.SolidColor;
        mainCam.backgroundColor = new Color(0.08f, 0.09f, 0.11f);

        ViewerController controller = mainCam.GetComponent<ViewerController>();
        if (controller == null) controller = mainCam.gameObject.AddComponent<ViewerController>();
        controller.walkSpeed = 3.5f;
        controller.sprintSpeed = 6.0f;
        controller.boundaryMin = new Vector2(-8.5f, -8.0f);
        controller.boundaryMax = new Vector2(8.5f, 7.5f);

        // 13. CANVAS HUD IOMONITOR
        GameObject canvasObj = new GameObject("Canvas_IOMonitor");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        GameObject panelObj = new GameObject("Panel_HUD");
        panelObj.transform.SetParent(canvasObj.transform, false);
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot = new Vector2(0f, 1f);
        panelRect.anchoredPosition = new Vector2(16f, -16f);
        panelRect.sizeDelta = new Vector2(440f, 315f);

        Image panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(0.04f, 0.06f, 0.09f, 0.90f);

        GameObject textObj = new GameObject("Status_Text_TMP");
        textObj.transform.SetParent(panelObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(12f, 10f);
        textRect.offsetMax = new Vector2(-12f, -10f);

        TextMeshProUGUI tmpGui = textObj.AddComponent<TextMeshProUGUI>();
        tmpGui.fontSize = 11.2f;
        tmpGui.lineSpacing = -2f;
        tmpGui.color = Color.white;
        tmpGui.richText = true;
        tmpGui.alignment = TextAlignmentOptions.TopLeft;

        // 14. IOMONITOR MANAGER
        GameObject ioManager = new GameObject("IOMonitor_Manager");
        ioManager.transform.parent = root.transform;

        AudioSource audioSource = ioManager.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f;

        IOMonitor ioMonitor = ioManager.AddComponent<IOMonitor>();
        ioMonitor.viewerTransform = mainCam.transform;
        ioMonitor.viewerController = controller;
        ioMonitor.statusText = tmpGui;
        ioMonitor.interactiveMaket = maketGedung.transform;
        ioMonitor.infoBoardText = kioskTmp;
        ioMonitor.exhibitionSpotlight = spotLight;
        ioMonitor.galleryDirectionalLight = dirLight;
        ioMonitor.galleryRoomLight = roomLight;
        ioMonitor.spatialAudioSource = audioSource;

        // Simpan Scene
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[Scene1Builder] Maket Fasad Rektorat UPNVJ berhasil dibangun dan disimpan ke: {targetScenePath}");
    }

    /// <summary>
    /// Membangun model miniatur arsitektur Gedung Rektorat UPNVJ persis sesuai foto referensi asli.
    /// </summary>
    private static GameObject CreateGedungRektoratUPNVJ(Transform parent, Material matWarnaUtama, Material matSilverACP, Material matGlass, Material matBelaNegara, Material matPillarGray, Material matFlagRed, Material matFlagWhite, Material matBluePaving, Material matWhiteTrim, Material matPedestal)
    {
        GameObject maket = new GameObject("Maket_Gedung_Utama");
        maket.transform.parent = parent;
        maket.transform.position = new Vector3(0f, 0.85f, 1.5f);

        // 1. BASE PLINTH & TAMAN RUMPUT MINIATUR
        GameObject baseGrass = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseGrass.name = "Plinth_Taman_Miniatur";
        baseGrass.transform.parent = maket.transform;
        baseGrass.transform.localPosition = new Vector3(0f, 0.03f, 0.1f);
        baseGrass.transform.localScale = new Vector3(5.6f, 0.06f, 3.2f);
        baseGrass.GetComponent<Renderer>().sharedMaterial = matPedestal;

        // Area Paving Drop-off Biru khas UPNVJ di depan gedung
        GameObject blueDropoff = GameObject.CreatePrimitive(PrimitiveType.Cube);
        blueDropoff.name = "Paving_DropOff_Biru_UPNVJ";
        blueDropoff.transform.parent = maket.transform;
        blueDropoff.transform.localPosition = new Vector3(0f, 0.065f, -1.25f);
        blueDropoff.transform.localScale = new Vector3(2.4f, 0.02f, 0.65f);
        blueDropoff.GetComponent<Renderer>().sharedMaterial = matBluePaving;

        // Teks Marka Jalan Drop Off
        GameObject dropoffTextObj = new GameObject("Teks_DropOff");
        dropoffTextObj.transform.parent = blueDropoff.transform;
        dropoffTextObj.transform.localPosition = new Vector3(0f, 0.55f, 0f);
        dropoffTextObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        dropoffTextObj.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
        TextMeshPro dropoffTmp = dropoffTextObj.AddComponent<TextMeshPro>();
        dropoffTmp.text = "<b><color=#FFFFFF>TURUN PENUMPANG / DROP OFF</color></b>";
        dropoffTmp.fontSize = 18f;
        dropoffTmp.alignment = TextAlignmentOptions.Center;

        // 2. TIANG BENDERA MERAH PUTIH DI DEPAN GEDUNG REKTORAT (Sisi Kiri Pelataran)
        GameObject flagRoot = new GameObject("Tiang_Bendera_MerahPutih");
        flagRoot.transform.parent = maket.transform;
        flagRoot.transform.localPosition = new Vector3(-1.75f, 0.06f, -0.85f);

        GameObject flagBase1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        flagBase1.name = "Base_Tier_1";
        flagBase1.transform.parent = flagRoot.transform;
        flagBase1.transform.localPosition = new Vector3(0f, 0.03f, 0f);
        flagBase1.transform.localScale = new Vector3(0.45f, 0.06f, 0.45f);
        flagBase1.GetComponent<Renderer>().sharedMaterial = matWhiteTrim;

        GameObject flagBase2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        flagBase2.name = "Base_Tier_2";
        flagBase2.transform.parent = flagRoot.transform;
        flagBase2.transform.localPosition = new Vector3(0f, 0.08f, 0f);
        flagBase2.transform.localScale = new Vector3(0.3f, 0.05f, 0.3f);
        flagBase2.GetComponent<Renderer>().sharedMaterial = matWhiteTrim;

        GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.name = "Tiang_Stainless";
        pole.transform.parent = flagRoot.transform;
        pole.transform.localPosition = new Vector3(0f, 0.95f, 0f);
        pole.transform.localScale = new Vector3(0.025f, 0.85f, 0.025f);
        pole.GetComponent<Renderer>().sharedMaterial = matSilverACP;

        GameObject flagRed = GameObject.CreatePrimitive(PrimitiveType.Cube);
        flagRed.name = "Bendera_Merah";
        flagRed.transform.parent = flagRoot.transform;
        flagRed.transform.localPosition = new Vector3(0.18f, 1.62f, 0f);
        flagRed.transform.localRotation = Quaternion.Euler(0f, 15f, 3f);
        flagRed.transform.localScale = new Vector3(0.34f, 0.12f, 0.015f);
        flagRed.GetComponent<Renderer>().sharedMaterial = matFlagRed;

        GameObject flagWhite = GameObject.CreatePrimitive(PrimitiveType.Cube);
        flagWhite.name = "Bendera_Putih";
        flagWhite.transform.parent = flagRoot.transform;
        flagWhite.transform.localPosition = new Vector3(0.18f, 1.50f, 0f);
        flagWhite.transform.localRotation = Quaternion.Euler(0f, 15f, 3f);
        flagWhite.transform.localScale = new Vector3(0.34f, 0.12f, 0.015f);
        flagWhite.GetComponent<Renderer>().sharedMaterial = matFlagWhite;

        // 3. BADAN STRUKTURAL UTAMA GEDUNG (WARNA_UTAMA #511069)
        GameObject mainMass = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mainMass.name = "Massa_Struktural_Gedung";
        mainMass.transform.parent = maket.transform;
        mainMass.transform.localPosition = new Vector3(0f, 1.0f, 0.45f);
        mainMass.transform.localScale = new Vector3(5.2f, 1.8f, 1.6f);
        mainMass.GetComponent<Renderer>().sharedMaterial = matWarnaUtama; // Sesuai syarat Soal 1: material #511069

        // 4. PILAR DROP-OFF BAWAH (Colonnade 4 Pilar Persegi Abu-abu)
        float[] pillarX = new float[] { -1.35f, -0.45f, 0.45f, 1.35f };
        for (int i = 0; i < pillarX.Length; i++)
        {
            GameObject pilar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pilar.name = "Pilar_Ground_" + (i + 1);
            pilar.transform.parent = maket.transform;
            pilar.transform.localPosition = new Vector3(pillarX[i], 0.36f, -0.35f);
            pilar.transform.localScale = new Vector3(0.24f, 0.72f, 0.28f);
            pilar.GetComponent<Renderer>().sharedMaterial = matPillarGray;
        }

        GameObject entranceDoors = GameObject.CreatePrimitive(PrimitiveType.Cube);
        entranceDoors.name = "Pintu_Lobby_Kaca";
        entranceDoors.transform.parent = maket.transform;
        entranceDoors.transform.localPosition = new Vector3(0f, 0.35f, 0.1f);
        entranceDoors.transform.localScale = new Vector3(2.6f, 0.65f, 0.05f);
        entranceDoors.GetComponent<Renderer>().sharedMaterial = matGlass;

        // 5. BINGKAI GERBANG PORTAL TENGAH (Central Grand Portal ACP)
        GameObject portalColL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        portalColL.name = "Portal_Kolom_Kiri";
        portalColL.transform.parent = maket.transform;
        portalColL.transform.localPosition = new Vector3(-1.45f, 1.38f, -0.36f);
        portalColL.transform.localScale = new Vector3(0.22f, 1.35f, 0.15f);
        portalColL.GetComponent<Renderer>().sharedMaterial = matSilverACP;

        GameObject portalColR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        portalColR.name = "Portal_Kolom_Kanan";
        portalColR.transform.parent = maket.transform;
        portalColR.transform.localPosition = new Vector3(1.45f, 1.38f, -0.36f);
        portalColR.transform.localScale = new Vector3(0.22f, 1.35f, 0.15f);
        portalColR.GetComponent<Renderer>().sharedMaterial = matSilverACP;

        GameObject portalLintel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        portalLintel.name = "Portal_Lintel_Atas";
        portalLintel.transform.parent = maket.transform;
        portalLintel.transform.localPosition = new Vector3(0f, 2.12f, -0.36f);
        portalLintel.transform.localScale = new Vector3(3.12f, 0.22f, 0.16f);
        portalLintel.GetComponent<Renderer>().sharedMaterial = matSilverACP;

        // Mahkota Bertingkat Rektorat (Stepped Crown)
        GameObject crownTier1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crownTier1.name = "Mahkota_Stepped_Tier1";
        crownTier1.transform.parent = maket.transform;
        crownTier1.transform.localPosition = new Vector3(0f, 2.30f, -0.34f);
        crownTier1.transform.localScale = new Vector3(2.4f, 0.16f, 0.14f);
        crownTier1.GetComponent<Renderer>().sharedMaterial = matSilverACP;

        GameObject crownTier2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crownTier2.name = "Mahkota_Stepped_Tier2";
        crownTier2.transform.parent = maket.transform;
        crownTier2.transform.localPosition = new Vector3(0f, 2.45f, -0.32f);
        crownTier2.transform.localScale = new Vector3(1.8f, 0.16f, 0.12f);
        crownTier2.GetComponent<Renderer>().sharedMaterial = matWarnaUtama; // Sentuhan aksen #511069

        // 6. DINDING KACA TIRAI TENGAH (Center Glass Curtain Wall)
        GameObject centerGlass = GameObject.CreatePrimitive(PrimitiveType.Cube);
        centerGlass.name = "Kaca_Tirai_Tengah";
        centerGlass.transform.parent = maket.transform;
        centerGlass.transform.localPosition = new Vector3(0f, 1.45f, -0.30f);
        centerGlass.transform.localScale = new Vector3(2.7f, 1.25f, 0.05f);
        centerGlass.GetComponent<Renderer>().sharedMaterial = matGlass;

        // Teks Ikonik "UPNVJ" & "KAMPUS BELA NEGARA"
        GameObject upnvjTextObj = new GameObject("Teks_UPNVJ_BelaNegara");
        upnvjTextObj.transform.parent = maket.transform;
        upnvjTextObj.transform.localPosition = new Vector3(0f, 1.88f, -0.34f);
        upnvjTextObj.transform.localRotation = Quaternion.identity;
        upnvjTextObj.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);

        TextMeshPro upnvjTmp = upnvjTextObj.AddComponent<TextMeshPro>();
        upnvjTmp.text = "<b><color=#FFFFFF><size=130%>UPNVJ</size></color></b>\n<size=55%><color=#E0EBF5>KAMPUS BELA NEGARA</color></size>";
        upnvjTmp.fontSize = 24f;
        upnvjTmp.alignment = TextAlignmentOptions.Center;
        upnvjTmp.rectTransform.sizeDelta = new Vector2(180f, 60f);

        // 7. PANEL GEOMETRIS ORIGAMI IKONIK UPNVJ (Sisi Kiri & Kanan Kaca Tengah)
        CreateGeometricFacadePanel(maket.transform, new Vector3(-0.95f, 1.42f, -0.33f), matBelaNegara, matSilverACP, matWarnaUtama, true);
        CreateGeometricFacadePanel(maket.transform, new Vector3(0.95f, 1.42f, -0.33f), matBelaNegara, matSilverACP, matWarnaUtama, false);

        // 8. LAYAR MEGATRON / VIDEOTRON TENGAH BESAR
        GameObject videotronFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
        videotronFrame.name = "Videotron_Frame_Hijau";
        videotronFrame.transform.parent = maket.transform;
        videotronFrame.transform.localPosition = new Vector3(0f, 1.08f, -0.35f);
        videotronFrame.transform.localScale = new Vector3(1.72f, 0.72f, 0.08f);
        videotronFrame.GetComponent<Renderer>().sharedMaterial = matBelaNegara; // Bingkai hijau khas foto

        GameObject videotronScreen = GameObject.CreatePrimitive(PrimitiveType.Cube);
        videotronScreen.name = "Videotron_Display_Screen";
        videotronScreen.transform.parent = maket.transform;
        videotronScreen.transform.localPosition = new Vector3(0f, 1.08f, -0.40f);
        videotronScreen.transform.localScale = new Vector3(1.58f, 0.58f, 0.04f);
        videotronScreen.GetComponent<Renderer>().sharedMaterial = matPedestal;

        // World-Space Canvas untuk Layar Videotron Tengah (Teks Besar & Proporsional Sesuai Border Hijau)
        GameObject videoCanvasObj = new GameObject("Videotron_World_Canvas");
        videoCanvasObj.transform.parent = maket.transform;
        videoCanvasObj.transform.localPosition = new Vector3(0f, 1.08f, -0.425f);
        videoCanvasObj.transform.localRotation = Quaternion.identity;
        videoCanvasObj.transform.localScale = new Vector3(0.0018f, 0.0018f, 0.0018f);

        Canvas vCanvas = videoCanvasObj.AddComponent<Canvas>();
        vCanvas.renderMode = RenderMode.WorldSpace;
        videoCanvasObj.AddComponent<CanvasScaler>();

        RectTransform vRect = videoCanvasObj.GetComponent<RectTransform>();
        vRect.sizeDelta = new Vector2(860f, 310f);

        GameObject vTextObj = new GameObject("Videotron_TMP");
        vTextObj.transform.SetParent(videoCanvasObj.transform, false);
        RectTransform vTextRect = vTextObj.AddComponent<RectTransform>();
        vTextRect.anchorMin = Vector2.zero;
        vTextRect.anchorMax = Vector2.one;
        vTextRect.offsetMin = new Vector2(24f, 16f);
        vTextRect.offsetMax = new Vector2(-24f, -16f);

        TextMeshProUGUI vTmp = vTextObj.AddComponent<TextMeshProUGUI>();
        vTmp.alignment = TextAlignmentOptions.Center;
        vTmp.enableWordWrapping = true;
        vTmp.richText = true;
        vTmp.text = "<b><color=#00FF66><size=135%>GEDUNG REKTORAT UPNVJ</size></color></b>\n" +
                    "<b><size=88%><color=#FFD700>KAMPUS BELA NEGARA</color></size></b>\n" +
                    "<size=64%><color=#E2EDF8>Fakultas Ilmu Komputer • S1 Informatika</color></size>";
        vTmp.fontSize = 44f;

        // 9. SAYAP HORIZONTAL SIMETRIS KIRI & KANAN (Wings with Curtain Glass)
        GameObject wingL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wingL.name = "Sayap_Kiri_GlassCurtain";
        wingL.transform.parent = maket.transform;
        wingL.transform.localPosition = new Vector3(-2.05f, 1.45f, -0.15f);
        wingL.transform.localScale = new Vector3(1.1f, 1.15f, 0.05f);
        wingL.GetComponent<Renderer>().sharedMaterial = matGlass;

        GameObject wingR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wingR.name = "Sayap_Kanan_GlassCurtain";
        wingR.transform.parent = maket.transform;
        wingR.transform.localPosition = new Vector3(2.05f, 1.45f, -0.15f);
        wingR.transform.localScale = new Vector3(1.1f, 1.15f, 0.05f);
        wingR.GetComponent<Renderer>().sharedMaterial = matGlass;

        GameObject roofFasciaL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roofFasciaL.name = "Fascia_Atap_Kiri";
        roofFasciaL.transform.parent = maket.transform;
        roofFasciaL.transform.localPosition = new Vector3(-2.05f, 2.06f, -0.15f);
        roofFasciaL.transform.localScale = new Vector3(1.15f, 0.16f, 0.12f);
        roofFasciaL.GetComponent<Renderer>().sharedMaterial = matSilverACP;

        GameObject roofFasciaR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roofFasciaR.name = "Fascia_Atap_Kanan";
        roofFasciaR.transform.parent = maket.transform;
        roofFasciaR.transform.localPosition = new Vector3(2.05f, 2.06f, -0.15f);
        roofFasciaR.transform.localScale = new Vector3(1.15f, 0.16f, 0.12f);
        roofFasciaR.GetComponent<Renderer>().sharedMaterial = matSilverACP;

        // 10. COLLIDER RAYCAST INTERAKTIF
        BoxCollider maketCollider = maket.AddComponent<BoxCollider>();
        maketCollider.center = new Vector3(0f, 1.25f, 0.1f);
        maketCollider.size = new Vector3(5.6f, 2.5f, 2.4f);

        return maket;
    }

    /// <summary>
    /// Membangun panel motif geometris origami segitiga khas fasad Gedung Rektorat UPNVJ.
    /// </summary>
    private static void CreateGeometricFacadePanel(Transform parent, Vector3 localPos, Material matGreen, Material matSilver, Material matPurple, bool flip)
    {
        GameObject panel = new GameObject(flip ? "Panel_Geometris_Kiri" : "Panel_Geometris_Kanan");
        panel.transform.parent = parent;
        panel.transform.localPosition = localPos;

        GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bg.name = "Panel_Backdrop";
        bg.transform.parent = panel.transform;
        bg.transform.localPosition = Vector3.zero;
        bg.transform.localScale = new Vector3(0.58f, 1.15f, 0.04f);
        bg.GetComponent<Renderer>().sharedMaterial = matSilver;

        int rows = 4;
        for (int r = 0; r < rows; r++)
        {
            float y = -0.42f + r * 0.28f;

            GameObject facet1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            facet1.name = $"Facet_{r}_1";
            facet1.transform.parent = panel.transform;
            facet1.transform.localPosition = new Vector3(-0.13f, y, -0.035f);
            facet1.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            facet1.transform.localScale = new Vector3(0.18f, 0.18f, 0.03f);
            facet1.GetComponent<Renderer>().sharedMaterial = (r % 2 == 0) ? matGreen : matPurple;

            GameObject facet2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            facet2.name = $"Facet_{r}_2";
            facet2.transform.parent = panel.transform;
            facet2.transform.localPosition = new Vector3(0.13f, y, -0.035f);
            facet2.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            facet2.transform.localScale = new Vector3(0.18f, 0.18f, 0.03f);
            facet2.GetComponent<Renderer>().sharedMaterial = (r % 2 == 0) ? matSilver : matGreen;
        }
    }

    private static void CreateWall(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.parent = parent;
        wall.transform.position = pos;
        wall.transform.localScale = scale;
        wall.GetComponent<Renderer>().sharedMaterial = mat;
    }

    private static void CreateWallPoster(Transform parent, string name, Vector3 pos, Vector3 scale, string content, Material frameMat)
    {
        GameObject poster = GameObject.CreatePrimitive(PrimitiveType.Cube);
        poster.name = name;
        poster.transform.parent = parent;
        poster.transform.position = pos;
        poster.transform.localScale = scale;
        poster.GetComponent<Renderer>().sharedMaterial = frameMat;

        GameObject canvasObj = new GameObject("Canvas_" + name);
        canvasObj.transform.parent = poster.transform;
        canvasObj.transform.localPosition = new Vector3(0f, 0f, -0.52f);
        canvasObj.transform.localRotation = Quaternion.identity;
        canvasObj.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);

        Canvas cv = canvasObj.AddComponent<Canvas>();
        cv.renderMode = RenderMode.WorldSpace;
        canvasObj.GetComponent<RectTransform>().sizeDelta = new Vector2(500f, 350f);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = 22f;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.rectTransform.anchorMin = Vector2.zero;
        tmp.rectTransform.anchorMax = Vector2.one;
        tmp.rectTransform.offsetMin = new Vector2(10f, 10f);
        tmp.rectTransform.offsetMax = new Vector2(-10f, -10f);
    }

    private static void CreateStanchion(Transform parent, Vector3 pos, Material mat)
    {
        GameObject stanchion = new GameObject("Stanchion_Pole");
        stanchion.transform.parent = parent;
        stanchion.transform.position = pos;

        GameObject baseCyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        baseCyl.transform.parent = stanchion.transform;
        baseCyl.transform.localPosition = new Vector3(0f, 0.03f, 0f);
        baseCyl.transform.localScale = new Vector3(0.3f, 0.03f, 0.3f);
        baseCyl.GetComponent<Renderer>().sharedMaterial = mat;

        GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.transform.parent = stanchion.transform;
        pole.transform.localPosition = new Vector3(0f, 0.45f, 0f);
        pole.transform.localScale = new Vector3(0.06f, 0.45f, 0.06f);
        pole.GetComponent<Renderer>().sharedMaterial = mat;

        GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cap.transform.parent = stanchion.transform;
        cap.transform.localPosition = new Vector3(0f, 0.92f, 0f);
        cap.transform.localScale = new Vector3(0.14f, 0.14f, 0.14f);
        cap.GetComponent<Renderer>().sharedMaterial = mat;
    }

    private static Material GetOrCreateMaterial(string path, Color color, float smoothness = 0.5f, bool isEmissive = false, Color? emissionColor = null)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader standardShader = Shader.Find("Standard");
            mat = new Material(standardShader);
            mat.color = color;
            mat.SetFloat("_Glossiness", smoothness);

            if (isEmissive && emissionColor.HasValue)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emissionColor.Value);
            }

            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            mat.color = color;
            mat.SetFloat("_Glossiness", smoothness);
            if (isEmissive && emissionColor.HasValue)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emissionColor.Value);
            }
            EditorUtility.SetDirty(mat);
        }
        return mat;
    }
}

/// <summary>
/// Otomatis memicu build fasad UPNVJ saat script selesai di-compile oleh Unity Editor.
/// </summary>
[InitializeOnLoad]
public static class Scene1AutoRunner
{
    static Scene1AutoRunner()
    {
        EditorApplication.delayCall += () =>
        {
            const string RunKey = "Scene1Builder_AutoBuilt_UPNVJ_v5";
            if (!SessionState.GetBool(RunKey, false))
            {
                SessionState.SetBool(RunKey, true);
                Debug.Log("[Scene1AutoRunner] Membangun fasad Rektorat UPNVJ secara otomatis...");
                Scene1Builder.BuildAndSyncBothQuiet();
            }
        };
    }
}
#endif
