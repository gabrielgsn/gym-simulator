using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class SceneBuilder
{
    const string GameScenePath = "Assets/Scenes/Game.unity";
    const string MenuScenePath = "Assets/Scenes/Menu.unity";
    const string PrefabPath = "Assets/Prefabs/Coletavel.prefab";
    const string CollectibleTag = "Coletavel";
    const float ArenaHalfW = 13f;
    const float ArenaHalfH = 7.5f;

    static readonly Color Accent = new Color(1f, 0.8f, 0.2f);
    static readonly Color Muted = new Color(0.75f, 0.75f, 0.8f);

    static Font font;
    static Sprite circle, square, ring, floor, wall, triangle;

    [MenuItem("Tools/Build Game Scene")]
    public static void Build()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        circle = LoadSprite("Circle", 128, false);
        square = LoadSprite("Square", 128, false);
        ring = LoadSprite("Ring", 128, false);
        floor = LoadSprite("Floor", 16, true, true);
        wall = LoadSprite("Wall", 16, true, true);
        triangle = LoadSprite("Triangle", 128, false);

        AddTag(CollectibleTag);
        BuildMenu();
        BuildGame();

        foreach (var path in new[] { "Assets/Scenes/SampleScene.unity", "Assets/Scenes/Tutorial.unity" })
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null)
                AssetDatabase.DeleteAsset(path);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MenuScenePath, true),
            new EditorBuildSettingsScene(GameScenePath, true),
        };
        AssetDatabase.SaveAssets();
        Debug.Log("Scenes built");
    }

    static void BuildMenu()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera(new Vector3(0, 0, -10));
        CreateGlobalLight();

        CreateTiled("Floor", floor, Vector2.zero, new Vector2(20, 12), -10);
        CreateDecor("Bench", new Vector2(-5.5f, -1.5f), 2);
        CreateDecor("SquatRack", new Vector2(5.5f, -1.5f), 2);
        CreateDecor("GymBro", new Vector2(-2.6f, -3.3f), 2.5f);
        CreateDecor("Player", new Vector2(0, -3.3f), 2.5f);
        CreateDecor("Chato", new Vector2(2.6f, -3.3f), 2.5f);

        var music = new GameObject("Music").AddComponent<AudioSource>();
        music.clip = LoadClip("MenuMusic");
        music.loop = true;
        music.volume = 0.4f;
        music.playOnAwake = true;

        CreateEventSystem();
        var canvas = CreateCanvas("Canvas");
        var menu = canvas.gameObject.AddComponent<MainMenu>();

        var main = CreatePanel(canvas, "Main Panel", Color.clear);
        CreateText(main, "Title", "GYM SIMULATOR", 72, Accent, new Vector2(0, 170), new Vector2(900, 100));
        CreateText(main, "Subtitle", "Complete a ficha de treino antes da academia fechar!", 22, Color.white, new Vector2(0, 105), new Vector2(900, 40));
        var play = CreateButton(main, "Play Button", "TREINAR", new Vector2(0, 20), menu.Play);
        CreateButton(main, "How To Button", "COMO JOGAR", new Vector2(0, -50), menu.ShowHowTo);
        var record = CreateText(main, "Record", "", 18, Muted, new Vector2(0, -115), new Vector2(800, 30));
        CreateText(main, "Credits", "Projeto Introdução ao Unity  |  Insper 2026.2", 14, Muted, new Vector2(0, -280), new Vector2(800, 24));

        var howTo = CreatePanel(canvas, "How To Panel", new Color(0.05f, 0.05f, 0.07f, 0.94f));
        CreateText(howTo, "Title", "COMO JOGAR", 40, Accent, new Vector2(0, 240), new Vector2(800, 60));
        CreateText(howTo, "Body",
            "Complete os 8 exercícios da ficha antes da academia fechar (3:00).\n\n" +
            "Andar: WASD / setas / analógico esquerdo\n" +
            "Usar máquina: E / Enter / A\n" +
            "Pausar: Esc / P / Start\n\n" +
            "Supino: aperte Espaço / Z / X / Clique / A quando o anel fechar\n" +
            "Agachamento: segure pra descer e solte na zona verde\n" +
            "Desenvolvimento e Terra: aperte várias vezes pra subir a barra (no Terra, cuidado com a lombar)\n" +
            "Tríceps: segure e solte pra manter o cursor na faixa verde\n" +
            "Leg Curl: digite a sequência de setas antes do tempo acabar\n" +
            "Rosca: A / D no ritmo, alternando   |   Panturrilha: ESPAÇO pra pular as anilhas\n\n" +
            "Cada série gasta energia. Pegue shakes pra recuperar.\n" +
            "Cuidado com o marombeiro (tira energia) e com o chato do \"vai usar?\" (rouba tempo).\n" +
            "Sobrou tempo? Vira bônus de score!",
            16, Color.white, new Vector2(0, 10), new Vector2(900, 420));
        var back = CreateButton(howTo, "Back Button", "VOLTAR", new Vector2(0, -250), menu.HideHowTo);

        menu.mainPanel = main.gameObject;
        menu.howToPanel = howTo.gameObject;
        menu.firstButton = play.gameObject;
        menu.howToBackButton = back.gameObject;
        menu.recordText = record;
        howTo.gameObject.SetActive(false);

        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), MenuScenePath);
    }

    static void BuildGame()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cam = CreateCamera(new Vector3(0, -2.5f, -10));
        CreateGlobalLight();

        CreateTiled("Floor", floor, Vector2.zero, new Vector2(ArenaHalfW * 2, ArenaHalfH * 2), -10);
        CreateMat(new Vector2(-9.25f, 0), new Vector2(7.5f, 15f), new Color(0.4f, 0.55f, 1f, 0.035f));
        CreateMat(new Vector2(0, 0), new Vector2(11f, 15f), new Color(1f, 0.45f, 0.45f, 0.025f));
        CreateMat(new Vector2(9.25f, 0), new Vector2(7.5f, 15f), new Color(0.45f, 1f, 0.55f, 0.03f));

        var walls = new GameObject("Walls").transform;
        CreateWall(walls, "Wall Top", new Vector2(0, ArenaHalfH + 0.25f), new Vector2(ArenaHalfW * 2 + 1, 0.5f));
        CreateWall(walls, "Wall Bottom", new Vector2(0, -ArenaHalfH - 0.25f), new Vector2(ArenaHalfW * 2 + 1, 0.5f));
        CreateWall(walls, "Wall Left", new Vector2(-ArenaHalfW - 0.25f, 0), new Vector2(0.5f, ArenaHalfH * 2 + 1));
        CreateWall(walls, "Wall Right", new Vector2(ArenaHalfW + 0.25f, 0), new Vector2(0.5f, ArenaHalfH * 2 + 1));
        CreateWall(walls, "Partition Left", new Vector2(-5.5f, 4.75f), new Vector2(0.5f, 5.5f));
        CreateWall(walls, "Partition Right", new Vector2(5.5f, 4.75f), new Vector2(0.5f, 5.5f));

        CreateZoneLabel("PERNAS", new Vector2(-9.25f, 6.9f));
        CreateZoneLabel("PEITO & OMBRO", new Vector2(0, 6.9f));
        CreateZoneLabel("BRAÇOS & PANTURRILHA", new Vector2(9.25f, 6.9f));

        var player = CreatePlayer(new Vector2(0, -5.5f));
        var follow = cam.gameObject.AddComponent<CameraFollow>();
        follow.target = player.transform;
        follow.boundsMin = new Vector2(-ArenaHalfW - 0.5f, -ArenaHalfH - 0.5f);
        follow.boundsMax = new Vector2(ArenaHalfW + 0.5f, ArenaHalfH + 0.5f);

        var shakePrefab = CreateCollectiblePrefab();
        var shakes = new GameObject("Shakes");
        foreach (var spot in new Vector2[] { new(-4, -5.5f), new(4, -5.5f), new(0, 3) })
        {
            var s = (GameObject)PrefabUtility.InstantiatePrefab(shakePrefab, shakes.transform);
            s.transform.position = spot;
        }
        var spawner = shakes.AddComponent<ShakeSpawner>();
        spawner.shakePrefab = shakePrefab;
        spawner.spawnPoints = new Vector2[]
        {
            new(0, 0), new(-4, -5.5f), new(4, -5.5f), new(-7.5f, -6), new(7.5f, -6), new(0, 3),
            new(-12, -6.5f), new(12, -6.5f), new(-7.5f, 3.5f), new(7.5f, 3.5f),
        };

        CreatePatrol("Marombeiro", new Vector2(-4, 1.5f), 2.2f, new Vector2[] { new(-4, 1.5f), new(4, 1.5f), new(4, -3), new(-4, -3) }, player.transform, Color.white);
        CreatePatrol("Marombeiro 2", new Vector2(-7.3f, 6), 2.6f, new Vector2[] { new(-7.3f, 6), new(-7.3f, -6) }, player.transform, new Color(1f, 0.85f, 0.85f));
        CreateChaser("Chato", new Vector2(7.5f, -1), player.transform);

        CreateEventSystem();
        var canvas = CreateCanvas("Canvas");

        var hud = BuildHud(canvas);
        var prompt = CreateText(canvas, "Prompt", "", 26, Color.white, new Vector2(0, 50), new Vector2(700, 40));
        Anchor(prompt.rectTransform, new Vector2(0.5f, 0));
        prompt.gameObject.SetActive(false);

        int reps = EnvInt("GYM_REPS", 0);
        var machines = new List<GymMachine>
        {
            CreateMachine("Supino", "Bench", new Vector2(-2.5f, 5), Timing(canvas, "Supino", reps, 1.2f, 0.6f, new Color(1f, 0.4f, 0.65f)), prompt),
            CreateMachine("Desenvolvimento", "ShoulderPress", new Vector2(2.5f, 5), Mash(canvas, "Desenvolvimento", reps, false, false, 0.2f, 0.12f), prompt),
            CreateMachine("Tríceps", "CableStation", new Vector2(10, 1.5f), Tracking(canvas, "Tríceps", reps), prompt),
            CreateMachine("Agachamento", "SquatRack", new Vector2(-10, 5), PowerBar(canvas, "Agachamento", reps, 0.5f, 0.8f, 1f, 0.13f, 0.08f), prompt),
            CreateMachine("Terra", "Deadlift", new Vector2(-10, 1), Mash(canvas, "Levantamento Terra", reps, true, true, 0.18f, 0.13f), prompt),
            CreateMachine("Leg Curl", "LegCurl", new Vector2(-10, -3.5f), Qte(canvas, "Leg Curl", reps), prompt),
            CreateMachine("Rosca", "DumbbellRack", new Vector2(10, 5.5f), Rhythm(canvas, "Rosca Bíceps", reps == 0 ? 8 : reps, 0.8f, 0.5f), prompt),
            CreateMachine("Panturrilha", "CalfMachine", new Vector2(10, -3), Jump(canvas, "Panturrilha", reps == 0 ? 8 : reps), prompt),
        };

        var gmGo = new GameObject("Game Manager");
        var gm = gmGo.AddComponent<GameManager>();
        gm.timeLimit = EnvInt("GYM_TIME", 180);
        gm.machines = machines;
        gm.hud = hud;
        gm.playerGrowth = player.GetComponent<PlayerGrowth>();
        gm.musicSource = gmGo.AddComponent<AudioSource>();
        gm.musicSource.clip = LoadClip("GameMusic");
        gm.musicSource.loop = true;
        gm.musicSource.volume = 0.35f;
        gm.musicSource.playOnAwake = true;
        gm.sfxSource = gmGo.AddComponent<AudioSource>();
        gm.sfxSource.playOnAwake = false;
        gm.winClip = LoadClip("Win");
        gm.loseClip = LoadClip("Lose");
        gm.deniedClip = LoadClip("Denied");
        gm.shakeClip = LoadClip("Shake");

        var pause = CreatePanel(canvas, "Pause Panel", new Color(0.05f, 0.05f, 0.07f, 0.9f));
        CreateText(pause, "Title", "PAUSADO", 48, Accent, new Vector2(0, 150), new Vector2(600, 70));
        gm.pauseFirstButton = CreateButton(pause, "Resume Button", "CONTINUAR", new Vector2(0, 40), gm.Resume).gameObject;
        CreateButton(pause, "Restart Button", "REINICIAR", new Vector2(0, -30), gm.Restart);
        CreateButton(pause, "Menu Button", "MENU", new Vector2(0, -100), gm.GoToMenu);
        gm.pausePanel = pause.gameObject;

        var end = CreatePanel(canvas, "End Panel", new Color(0.05f, 0.05f, 0.07f, 0.94f));
        gm.endTitle = CreateText(end, "Title", "", 52, Accent, new Vector2(0, 200), new Vector2(900, 70));
        gm.endStats = CreateText(end, "Stats", "", 24, Color.white, new Vector2(0, 50), new Vector2(700, 200));
        gm.endFirstButton = CreateButton(end, "Restart Button", "JOGAR DE NOVO", new Vector2(0, -110), gm.Restart).gameObject;
        CreateButton(end, "Menu Button", "MENU", new Vector2(0, -180), gm.GoToMenu);
        gm.endPanel = end.gameObject;

        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), GameScenePath);
    }

    static Hud BuildHud(Transform canvas)
    {
        var hud = canvas.gameObject.AddComponent<Hud>();
        var topLeft = new Vector2(0, 1);
        var topRight = new Vector2(1, 1);
        var top = new Vector2(0.5f, 1);

        hud.scoreText = CreateText(canvas, "Score", "", 24, Accent, new Vector2(120, -26), new Vector2(220, 32), TextAnchor.MiddleLeft);
        Anchor(hud.scoreText.rectTransform, topLeft);

        var listBg = CreateImage(canvas, "Checklist Bg", null, new Color(0, 0, 0, 0.45f), new Vector2(105, -150), new Vector2(190, 200));
        Anchor(listBg.rectTransform, topLeft);
        hud.checklistText = CreateText(canvas, "Checklist", "", 15, Color.white, new Vector2(110, -152), new Vector2(180, 192), TextAnchor.UpperLeft);
        Anchor(hud.checklistText.rectTransform, topLeft);

        hud.timerText = CreateText(canvas, "Timer", "", 38, Color.white, new Vector2(0, -30), new Vector2(200, 50));
        Anchor(hud.timerText.rectTransform, top);
        hud.toastText = CreateText(canvas, "Toast", "", 22, Color.white, new Vector2(0, -80), new Vector2(800, 34));
        Anchor(hud.toastText.rectTransform, top);

        hud.staminaText = CreateText(canvas, "Stamina Label", "", 16, Color.white, new Vector2(-125, -20), new Vector2(210, 24), TextAnchor.MiddleLeft);
        Anchor(hud.staminaText.rectTransform, topRight);
        var barBg = CreateImage(canvas, "Stamina Bar", square, new Color(0.15f, 0.15f, 0.18f), new Vector2(-125, -44), new Vector2(210, 18));
        Anchor(barBg.rectTransform, topRight);
        hud.staminaFillImage = CreateImage(barBg.transform, "Fill", square, Accent, Vector2.zero, Vector2.zero);
        hud.staminaFill = hud.staminaFillImage.rectTransform;
        hud.staminaFill.anchorMin = Vector2.zero;
        hud.staminaFill.anchorMax = Vector2.one;
        hud.staminaFill.sizeDelta = Vector2.zero;
        return hud;
    }

    static Camera CreateCamera(Vector3 pos)
    {
        var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(UniversalAdditionalCameraData));
        camGo.tag = "MainCamera";
        camGo.transform.position = pos;
        var cam = camGo.GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.08f, 0.1f);
        return cam;
    }

    static void CreateGlobalLight()
    {
        var light = new GameObject("Global Light 2D").AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Global;
    }

    static GameObject CreatePlayer(Vector2 pos)
    {
        var player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = pos;
        var visual = AddVisual(player, LoadSprite("PlayerBulk0", 16, true), 2);
        var growth = player.AddComponent<PlayerGrowth>();
        growth.stages = new Sprite[5];
        for (int i = 0; i < growth.stages.Length; i++)
            growth.stages[i] = LoadSprite("PlayerBulk" + i, 16, true);
        growth.spriteRenderer = visual.GetComponent<SpriteRenderer>();
        growth.visual = visual.GetComponent<CharacterVisual>();
        var rb = AddBody(player, 1f);
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        var col = player.AddComponent<CircleCollider2D>();
        col.radius = 0.35f;
        col.offset = new Vector2(0, -0.15f);
        var audio = player.AddComponent<AudioSource>();
        audio.clip = LoadClip("Collect");
        audio.playOnAwake = false;
        audio.loop = false;
        player.AddComponent<PlayerMovement>();
        return player;
    }

    static void CreatePatrol(string name, Vector2 pos, float speed, Vector2[] waypoints, Transform player, Color tint)
    {
        var go = CreateEnemy(name, "GymBro", pos, tint);
        var e = go.AddComponent<EnemyPatrol>();
        e.speed = speed;
        e.waypoints = waypoints;
        e.player = player;
        e.hitClip = LoadClip("Bump");
    }

    static void CreateChaser(string name, Vector2 pos, Transform player)
    {
        var go = CreateEnemy(name, "Chato", pos, Color.white);
        var e = go.AddComponent<EnemyChaser>();
        e.speed = 2.6f;
        e.player = player;
        e.hitClip = LoadClip("Talk");
        e.hitCooldown = 3f;
    }

    static GameObject CreateEnemy(string name, string sprite, Vector2 pos, Color tint)
    {
        var go = new GameObject(name);
        go.transform.position = pos;
        AddVisual(go, LoadSprite(sprite, 16, true), 2).GetComponent<SpriteRenderer>().color = tint;
        AddBody(go, 20f);
        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.4f;
        col.offset = new Vector2(0, -0.15f);
        go.AddComponent<AudioSource>().playOnAwake = false;
        return go;
    }

    static GameObject AddVisual(GameObject parent, Sprite sprite, int order)
    {
        var visual = new GameObject("Visual");
        visual.transform.SetParent(parent.transform, false);
        var sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = order;
        visual.AddComponent<CharacterVisual>();
        return visual;
    }

    static Rigidbody2D AddBody(GameObject go, float mass)
    {
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.mass = mass;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        return rb;
    }

    static GymMachine CreateMachine(string exercise, string sprite, Vector2 pos, MinigameBase minigame, Text prompt)
    {
        var go = new GameObject(exercise);
        go.transform.position = pos;
        go.AddComponent<SpriteRenderer>().sprite = LoadSprite(sprite, 16, true);
        var solid = go.AddComponent<BoxCollider2D>();
        var zone = go.AddComponent<BoxCollider2D>();
        zone.isTrigger = true;
        zone.size = solid.size + new Vector2(1.2f, 1.2f);
        var machine = go.AddComponent<GymMachine>();
        machine.exerciseName = exercise;
        machine.minigame = minigame;
        machine.promptText = prompt;
        return machine;
    }

    static T CreateMinigame<T>(Transform canvas, string title, string hint, int reps, Vector2 barbellPos, float barbellWidth) where T : MinigameBase
    {
        var panel = CreatePanel(canvas, title + " Minigame", new Color(0.05f, 0.05f, 0.07f, 0.93f));

        CreateText(panel, "Title", title.ToUpper(), 40, Color.white, new Vector2(0, 255), new Vector2(700, 50));
        var repsText = CreateText(panel, "Reps", "", 22, new Color(0.85f, 0.85f, 0.85f), new Vector2(0, 215), new Vector2(800, 30));
        var feedback = CreateText(panel, "Feedback", "", 34, Color.white, new Vector2(0, 160), new Vector2(600, 50));
        CreateText(panel, "Hint", hint, 22, Color.white, new Vector2(0, -262), new Vector2(940, 30));
        CreateText(panel, "Exit Hint", "Esc / B: sair", 14, new Color(0.6f, 0.6f, 0.65f), new Vector2(0, -288), new Vector2(300, 20));

        RectTransform barbell = null;
        if (barbellWidth > 0)
        {
            barbell = CreateImage(panel, "Barbell", null, Color.clear, barbellPos, new Vector2(barbellWidth, 80)).rectTransform;
            CreateImage(barbell, "Bar", square, new Color(0.8f, 0.82f, 0.86f), Vector2.zero, new Vector2(barbellWidth, 10));
            CreateImage(barbell, "Plate L", square, new Color(0.8f, 0.15f, 0.15f), new Vector2(-barbellWidth / 2 + 30, 0), new Vector2(22, 80));
            CreateImage(barbell, "Plate R", square, new Color(0.8f, 0.15f, 0.15f), new Vector2(barbellWidth / 2 - 30, 0), new Vector2(22, 80));
        }

        panel.gameObject.AddComponent<AudioSource>().playOnAwake = false;

        var game = panel.gameObject.AddComponent<T>();
        game.barbell = barbell;
        game.repsText = repsText;
        game.feedbackText = feedback;
        game.hitClip = LoadClip("Hit");
        game.missClip = LoadClip("Miss");
        game.completeClip = LoadClip("SetComplete");
        if (reps > 0)
            game.repsRequired = reps;
        panel.gameObject.SetActive(false);
        return game;
    }

    static TimingMinigame Timing(Transform canvas, string title, int reps, float start, float end, Color targetColor)
    {
        var game = CreateMinigame<TimingMinigame>(canvas, title, "ESPAÇO / A quando o anel fechar", reps, new Vector2(0, -150), 460);
        game.target = CreateImage(game.transform, "Target", circle, targetColor, Vector2.zero, new Vector2(90, 90)).rectTransform;
        game.approachRing = CreateImage(game.transform, "Approach Ring", ring, Color.white, Vector2.zero, new Vector2(90, 90)).rectTransform;
        game.startApproachTime = start;
        game.endApproachTime = end;
        game.targetArea = new Vector2(240, 70);
        game.barbellLow = -235f;
        game.barbellHigh = -110f;
        return game;
    }

    static PowerBarMinigame PowerBar(Transform canvas, string title, int reps, float zoneMin, float zoneMax, float accel, float startHalf, float endHalf)
    {
        var game = CreateMinigame<PowerBarMinigame>(canvas, title, "SEGURE ESPAÇO / A pra descer, SOLTE no verde", reps, new Vector2(-110, 0), 360);
        var meterPos = new Vector2(230, -60);
        var meter = CreateImage(game.transform, "Meter", square, new Color(0.22f, 0.22f, 0.26f), meterPos, new Vector2(40, 300)).rectTransform;
        game.meter = meter;
        game.zone = CreateImage(meter, "Zone", square, new Color(0.3f, 0.9f, 0.35f, 0.75f), Vector2.zero, new Vector2(40, 40)).rectTransform;
        game.pointer = CreateImage(meter, "Pointer", square, Color.white, new Vector2(0, 150), new Vector2(70, 6)).rectTransform;
        CreateText(game.transform, "Top Label", "EM CIMA", 16, Muted, meterPos + new Vector2(0, 168), new Vector2(120, 24));
        CreateText(game.transform, "Bottom Label", "FUNDO", 16, Muted, meterPos + new Vector2(0, -168), new Vector2(120, 24));
        game.zoneCenterMin = zoneMin;
        game.zoneCenterMax = zoneMax;
        game.acceleration = accel;
        game.zoneStartHalf = startHalf;
        game.zoneEndHalf = endHalf;
        game.barbellLow = -200f;
        game.barbellHigh = 60f;
        return game;
    }

    static RhythmMinigame Rhythm(Transform canvas, string title, int reps, float startBeat, float endBeat)
    {
        var game = CreateMinigame<RhythmMinigame>(canvas, title, "A e D alternando no ritmo", reps, Vector2.zero, 0);
        game.leftTarget = CreateImage(game.transform, "Left Target", circle, game.idleColor, new Vector2(-160, 10), new Vector2(110, 110)).rectTransform;
        game.rightTarget = CreateImage(game.transform, "Right Target", circle, game.idleColor, new Vector2(160, 10), new Vector2(110, 110)).rectTransform;
        CreateText(game.transform, "Left Label", "A / ← / LB", 18, Muted, new Vector2(-160, -70), new Vector2(200, 26));
        CreateText(game.transform, "Right Label", "D / → / RB", 18, Muted, new Vector2(160, -70), new Vector2(200, 26));
        game.approachRing = CreateImage(game.transform, "Approach Ring", ring, Color.white, Vector2.zero, new Vector2(110, 110)).rectTransform;
        game.leftWeight = CreateDumbbell(game.transform, "Left Dumbbell", new Vector2(-160, -170));
        game.rightWeight = CreateDumbbell(game.transform, "Right Dumbbell", new Vector2(160, -170));
        game.startBeat = startBeat;
        game.endBeat = endBeat;
        return game;
    }

    static MashMinigame Mash(Transform canvas, string title, int reps, bool grip, bool strain, float gravity, float push)
    {
        var game = CreateMinigame<MashMinigame>(canvas, title, grip ? "SEGURE, depois APERTE RÁPIDO sem encher a lombar" : "APERTE RÁPIDO ESPAÇO / A até o topo", reps, new Vector2(-110, 0), 360);
        var meterPos = new Vector2(230, -40);
        var meter = CreateImage(game.transform, "Meter", square, new Color(0.22f, 0.22f, 0.26f), meterPos, new Vector2(40, 300)).rectTransform;
        game.heightFill = CreateImage(meter, "Fill", square, new Color(1f, 0.8f, 0.2f), Vector2.zero, Vector2.zero).rectTransform;
        game.stickZone = CreateImage(meter, "Stick Zone", square, new Color(1f, 0.35f, 0.2f, 0.45f), Vector2.zero, new Vector2(54, 40)).rectTransform;
        CreateText(game.transform, "Top Label", "TOPO", 16, Muted, meterPos + new Vector2(0, 168), new Vector2(120, 24));
        CreateText(game.transform, "Stick Label", "TRAVA", 14, new Color(1f, 0.5f, 0.35f), meterPos + new Vector2(62, 0), new Vector2(80, 20));
        var strainBar = CreateBar(game.transform, "Strain", "LOMBAR", new Vector2(-110, -235), new Vector2(300, 16), new Color(1f, 0.35f, 0.3f));
        game.strainRoot = strainBar.parent.gameObject;
        game.strainFill = strainBar;
        var gripBar = CreateBar(game.transform, "Grip", "PEGADA", new Vector2(-110, -205), new Vector2(300, 16), new Color(0.4f, 0.8f, 1f));
        game.gripRoot = gripBar.parent.gameObject;
        game.gripFill = gripBar;
        game.useGrip = grip;
        game.useStrain = strain;
        game.gravity = gravity;
        game.pushPerPress = push;
        game.barbellLow = -170f;
        game.barbellHigh = 100f;
        return game;
    }

    static TrackingMinigame Tracking(Transform canvas, string title, int reps)
    {
        var game = CreateMinigame<TrackingMinigame>(canvas, title, "SEGURE ESPAÇO / A pra subir, SOLTE pra descer", reps, Vector2.zero, 0);
        var track = CreateImage(game.transform, "Track", square, new Color(0.22f, 0.22f, 0.26f), new Vector2(0, -20), new Vector2(60, 320)).rectTransform;
        game.track = track;
        game.band = CreateImage(track, "Band", square, new Color(0.3f, 0.9f, 0.35f, 0.6f), Vector2.zero, new Vector2(60, 80)).rectTransform;
        game.cursor = CreateImage(track, "Cursor", square, Color.white, Vector2.zero, new Vector2(84, 8)).rectTransform;
        CreateText(game.transform, "Rope Label", "CORDA", 16, Muted, new Vector2(0, 158), new Vector2(120, 24));
        game.progressFill = CreateBar(game.transform, "Progress", "REP", new Vector2(0, -235), new Vector2(360, 16), new Color(0.45f, 1f, 0.45f));
        return game;
    }

    static QteMinigame Qte(Transform canvas, string title, int reps)
    {
        var game = CreateMinigame<QteMinigame>(canvas, title, "Digite as setas: WASD ou setas", reps, Vector2.zero, 0);
        var arrows = new Image[6];
        for (int i = 0; i < arrows.Length; i++)
            arrows[i] = CreateImage(game.transform, "Arrow " + i, triangle, Color.white, new Vector2((i - 2.5f) * 95f, 0), new Vector2(64, 64));
        game.arrows = arrows;
        game.timerFill = CreateBar(game.transform, "Timer", "TEMPO", new Vector2(0, -120), new Vector2(500, 16), new Color(1f, 0.8f, 0.2f));
        return game;
    }

    static JumpMinigame Jump(Transform canvas, string title, int reps)
    {
        var game = CreateMinigame<JumpMinigame>(canvas, title, "ESPAÇO / A pra pular as anilhas (segure pra pular mais alto)", reps, Vector2.zero, 0);
        CreateImage(game.transform, "Floor", square, new Color(0.18f, 0.18f, 0.22f), new Vector2(0, game.groundY - 40), new Vector2(880, 80));
        CreateImage(game.transform, "Ground Line", square, new Color(0.6f, 0.6f, 0.68f), new Vector2(0, game.groundY - 2), new Vector2(880, 4));
        var runner = CreateImage(game.transform, "Runner", LoadSprite("PlayerBulk0", 16, true), Color.white, Vector2.zero, new Vector2(66, 55));
        runner.preserveAspect = true;
        game.player = runner.rectTransform;
        game.plates = new[] { CreatePlate(game.transform, "Plate A"), CreatePlate(game.transform, "Plate B") };
        return game;
    }

    static RectTransform CreatePlate(Transform parent, string name)
    {
        var plate = CreateImage(parent, name, circle, new Color(0.85f, 0.18f, 0.18f), Vector2.zero, new Vector2(50, 50)).rectTransform;
        CreateImage(plate, "Rim", ring, new Color(0.55f, 0.1f, 0.1f), Vector2.zero, new Vector2(50, 50));
        CreateImage(plate, "Hole", circle, new Color(0.15f, 0.15f, 0.18f), Vector2.zero, new Vector2(14, 14));
        CreateImage(plate, "Mark", square, new Color(1f, 0.85f, 0.85f), new Vector2(0, 15), new Vector2(4, 10));
        plate.gameObject.SetActive(false);
        return plate;
    }

    static RectTransform CreateBar(Transform parent, string name, string label, Vector2 pos, Vector2 size, Color fillColor)
    {
        var bg = CreateImage(parent, name, square, new Color(0.15f, 0.15f, 0.18f), pos, size);
        var fill = CreateImage(bg.transform, "Fill", square, fillColor, Vector2.zero, Vector2.zero).rectTransform;
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = new Vector2(0, 1);
        CreateText(bg.transform, "Label", label, 14, Muted, new Vector2(-size.x / 2 - 50, 0), new Vector2(100, 20), TextAnchor.MiddleRight);
        return fill;
    }

    static RectTransform CreateDumbbell(Transform parent, string name, Vector2 pos)
    {
        var root = CreateImage(parent, name, null, Color.clear, pos, new Vector2(120, 40)).rectTransform;
        CreateImage(root, "Handle", square, new Color(0.8f, 0.82f, 0.86f), Vector2.zero, new Vector2(110, 8));
        CreateImage(root, "Plate L", square, new Color(0.25f, 0.45f, 0.9f), new Vector2(-45, 0), new Vector2(18, 40));
        CreateImage(root, "Plate R", square, new Color(0.25f, 0.45f, 0.9f), new Vector2(45, 0), new Vector2(18, 40));
        return root;
    }

    static void CreateEventSystem()
    {
        var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule), typeof(UISelectionKeeper));
        go.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
    }

    static Transform CreateCanvas(string name)
    {
        var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(960, 600);
        scaler.matchWidthOrHeight = 0.5f;
        return go.transform;
    }

    static Transform CreatePanel(Transform parent, string name, Color color)
    {
        var panel = CreateImage(parent, name, null, color, Vector2.zero, Vector2.zero);
        panel.rectTransform.anchorMin = Vector2.zero;
        panel.rectTransform.anchorMax = Vector2.one;
        panel.raycastTarget = color.a > 0f;
        return panel.transform;
    }

    static Button CreateButton(Transform parent, string name, string label, Vector2 pos, UnityAction action)
    {
        var img = CreateImage(parent, name, square, Color.white, pos, new Vector2(280, 54));
        img.raycastTarget = true;
        var button = img.gameObject.AddComponent<Button>();
        var colors = button.colors;
        colors.normalColor = new Color(0.22f, 0.22f, 0.27f);
        colors.highlightedColor = new Color(0.95f, 0.72f, 0.15f);
        colors.selectedColor = new Color(0.95f, 0.72f, 0.15f);
        colors.pressedColor = new Color(0.75f, 0.55f, 0.1f);
        button.colors = colors;
        CreateText(img.transform, "Label", label, 24, Color.white, Vector2.zero, new Vector2(280, 54));
        UnityEventTools.AddPersistentListener(button.onClick, action);
        return button;
    }

    static Image CreateImage(Transform parent, string name, Sprite sprite, Color color, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;
        img.rectTransform.anchoredPosition = pos;
        img.rectTransform.sizeDelta = size;
        return img;
    }

    static Text CreateText(Transform parent, string name, string content, int size, Color color, Vector2 pos, Vector2 box, TextAnchor align = TextAnchor.MiddleCenter)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(Outline));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<Text>();
        text.font = font;
        text.text = content;
        text.fontSize = size;
        text.fontStyle = FontStyle.Bold;
        text.color = color;
        text.alignment = align;
        text.raycastTarget = false;
        text.supportRichText = true;
        text.rectTransform.anchoredPosition = pos;
        text.rectTransform.sizeDelta = box;
        go.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0.85f);
        return text;
    }

    static void Anchor(RectTransform rt, Vector2 anchor)
    {
        var pos = rt.anchoredPosition;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.anchoredPosition = pos;
    }

    static void CreateZoneLabel(string label, Vector2 pos)
    {
        var go = new GameObject("Label " + label, typeof(Canvas));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = -5;
        var rt = (RectTransform)go.transform;
        rt.position = pos;
        rt.sizeDelta = new Vector2(800, 100);
        rt.localScale = Vector3.one * 0.01f;
        CreateText(go.transform, "Text", label, 50, new Color(1f, 1f, 1f, 0.35f), Vector2.zero, new Vector2(800, 100));
    }

    static SpriteRenderer CreateTiled(string name, Sprite sprite, Vector2 pos, Vector2 size, int order)
    {
        var go = new GameObject(name);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = size;
        sr.sortingOrder = order;
        return sr;
    }

    static void CreateMat(Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject("Mat");
        go.transform.position = pos;
        go.transform.localScale = new Vector3(size.x, size.y, 1);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = square;
        sr.color = color;
        sr.sortingOrder = -9;
    }

    static void CreateWall(Transform parent, string name, Vector2 pos, Vector2 size)
    {
        var sr = CreateTiled(name, wall, pos, size, 0);
        sr.transform.SetParent(parent);
        sr.gameObject.AddComponent<BoxCollider2D>().size = size;
    }

    static GameObject CreateDecor(string sprite, Vector2 pos, float scale)
    {
        var go = new GameObject(sprite);
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * scale;
        go.AddComponent<SpriteRenderer>().sprite = LoadSprite(sprite, 16, true);
        return go;
    }

    static GameObject CreateCollectiblePrefab()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        var go = new GameObject("Coletavel");
        go.tag = CollectibleTag;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite("Shake", 16, true);
        sr.sortingOrder = 1;
        go.AddComponent<PolygonCollider2D>().isTrigger = true;
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
        Object.DestroyImmediate(go);
        return prefab;
    }

    static int EnvInt(string name, int fallback)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return int.TryParse(value, out int result) ? result : fallback;
    }

    static AudioClip LoadClip(string name) => AssetDatabase.LoadAssetAtPath<AudioClip>($"Assets/Audio/{name}.wav");

    static Sprite LoadSprite(string name, float ppu, bool point, bool tiled = false)
    {
        string path = $"Assets/Sprites/{name}.png";
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        var filter = point ? FilterMode.Point : FilterMode.Bilinear;
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        bool meshOk = !tiled || settings.spriteMeshType == SpriteMeshType.FullRect;
        bool wrapOk = !tiled || importer.wrapMode == TextureWrapMode.Repeat;
        if (importer.textureType != TextureImporterType.Sprite || importer.spritePixelsPerUnit != ppu || importer.filterMode != filter || !meshOk || !wrapOk)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = ppu;
            importer.filterMode = filter;
            if (point)
                importer.textureCompression = TextureImporterCompression.Uncompressed;
            if (tiled)
            {
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings);
                importer.wrapMode = TextureWrapMode.Repeat;
            }
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static void AddTag(string tag)
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tags = tagManager.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == tag)
                return;
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedPropertiesWithoutUndo();
    }
}
