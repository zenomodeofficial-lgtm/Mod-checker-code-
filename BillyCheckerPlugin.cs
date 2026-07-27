using System;
using BepInEx;
using UnityEngine;

namespace BillyChecker
{
    [BepInPlugin("com.billychecker.tool", "Billy Checker", "1.0.0")]
    public class BillyCheckerPlugin : BaseUnityPlugin
    {
        private static bool menuOpen = false;
        private Rect windowRect = new Rect(40, 40, 480, 520);
        private int selectedTab = 0;
        private readonly string[] tabs = { "Players", "Inspect", "Settings" };

        private GUIStyle windowStyle;
        private GUIStyle buttonStyle;
        private GUIStyle toggleStyle;
        private GUIStyle labelStyle;
        private GUIStyle titleStyle;
        private bool stylesInitialized = false;

        private Photon.Realtime.Player selectedPlayer = null;
        private string playerSearchFilter = "";

        private void Update()
        {
            try
            {
                if (UnityInput.Current.GetKeyDown(KeyCode.Insert) || (ControllerInputPoller.instance != null && ControllerInputPoller.instance.leftControllerSecondaryButton))
                {
                    menuOpen = !menuOpen;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BillyChecker Error]: {ex.Message}");
            }
        }

        private void OnGUI()
        {
            if (!menuOpen) return;

            InitStyles();
            windowRect = GUI.Window(999, windowRect, DrawWindowContent, "");
        }

        private void InitStyles()
        {
            if (stylesInitialized) return;

            Color fillColor = new Color(0.12f, 0.14f, 0.18f, 0.95f);
            Color borderColor = new Color(0.35f, 0.45f, 0.6f, 0.9f);

            windowStyle = new GUIStyle(GUI.skin.window)
            {
                normal = { background = MakeSolidTex(480, 520, fillColor) },
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                normal = { background = MakeSolidTex(180, 30, new Color(0.2f, 0.25f, 0.35f, 1f)), textColor = Color.white },
                hover = { background = MakeSolidTex(180, 30, new Color(0.3f, 0.4f, 0.55f, 1f)), textColor = Color.white },
                active = { background = MakeSolidTex(180, 30, new Color(0.15f, 0.2f, 0.28f, 1f)), textColor = Color.white },
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter
            };

            toggleStyle = new GUIStyle(GUI.skin.toggle)
            {
                normal = { textColor = Color.white },
                onNormal = { textColor = new Color(0.6f, 0.8f, 1f, 1f) },
                fontStyle = FontStyle.Normal
            };

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = new Color(0.85f, 0.88f, 0.92f, 1f) },
                fontSize = 12
            };

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = new Color(0.9f, 0.95f, 1f, 1f) },
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            stylesInitialized = true;
        }

        private void DrawWindowContent(int windowID)
        {
            stylesInitialized = false;

            GUILayout.Space(2);
            GUILayout.Label("Billy Checker", titleStyle);
            GUILayout.Space(6);

            selectedTab = GUI.Toolbar(new Rect(12, 32, 456, 25), selectedTab, tabs);
            GUILayout.Space(32);

            GUILayout.BeginVertical();
            switch (selectedTab)
            {
                case 0: // Players Tab
                    GUILayout.Label("Current Room Player Directory:", labelStyle);
                    GUILayout.Space(3);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Filter:", labelStyle, GUILayout.Width(45));
                    playerSearchFilter = GUILayout.TextField(playerSearchFilter);
                    GUILayout.EndHorizontal();
                    GUILayout.Space(4);

                    if (Photon.Pun.PhotonNetwork.InRoom)
                    {
                        foreach (Photon.Realtime.Player player in Photon.Pun.PhotonNetwork.PlayerList)
                        {
                            if (player == null) continue;

                            string displayString = $"{player.NickName} (ID: {player.ActorNumber})";
                            if (!string.IsNullOrEmpty(playerSearchFilter) && !displayString.ToLower().Contains(playerSearchFilter.ToLower()))
                                continue;

                            if (GUILayout.Button(displayString, buttonStyle))
                            {
                                selectedPlayer = player;
                                selectedTab = 1;
                            }
                        }
                    }
                    else
                    {
                        GUILayout.Label("Not currently connected to a room.", labelStyle);
                    }
                    break;

                case 1: // Inspect Tab
                    GUILayout.Label("Player Mod Inspector", labelStyle);
                    GUILayout.Space(5);

                    if (selectedPlayer != null)
                    {
                        GUILayout.Label($"Selected: {selectedPlayer.NickName}", labelStyle);
                        GUILayout.Space(5);

                        bool hasFly = selectedPlayer.CustomProperties.ContainsKey("Mod_Fly") && (bool)selectedPlayer.CustomProperties["Mod_Fly"];
                        bool hasSpeed = selectedPlayer.CustomProperties.ContainsKey("Mod_Speed") && (bool)selectedPlayer.CustomProperties["Mod_Speed"];

                        GUILayout.Label($"Fly Status: {(hasFly ? "Active ⚠️" : "None")}", labelStyle);
                        GUILayout.Label($"Speed Status: {(hasSpeed ? "Active ⚠️" : "None")}", labelStyle);
                    }
                    else
                    {
                        GUILayout.Label("Select a player from the Players tab to inspect.", labelStyle);
                    }
                    break;

                case 2: // Settings Tab
                    GUILayout.Label("Billy Checker Tool", labelStyle);
                    GUILayout.Space(10);
                    if (GUILayout.Button("Close Menu", buttonStyle))
                    {
                        menuOpen = false;
                    }
                    break;
            }
            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        private Texture2D MakeSolidTex(int width, int height, Color col)
        {
            Texture2D tex = new Texture2D(width, height);
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++) pix[i] = col;
            tex.SetPixels(pix);
            tex.Apply();
            return tex;
        }
    }
}
