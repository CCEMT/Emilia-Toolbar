using UnityEditor;
using UnityEngine;

namespace Emilia.Toolbar.Editor
{
    public partial class ConsoleCommandWindow
    {
        private class Style
        {
            public static Style instance { get; private set; }

            public const string CommandSearchLabel = "请输入命令名称";

            public const int ItemHeight = 30;

            public Color WindowColor;
            public Color SearchFieldColor;
            public Color SearchFieldTextColor;
            public Color SearchResultTextColor;
            public Color SelectedResultFieldColor;
            public Color ResultFieldColor;
            public Color CommandHelpTextColor;
            public Color CommandHelpHighlightTextColor;
            public Color HighlightOnSelectedTextColor;
            public Color QuickNameTextColor;

            public Texture2D TopPanelGradientTexture;
            public Texture2D TitleTexture;
            public Texture2D SearchFieldBackgroundTexture;
            public Texture2D SearchIcon;
            public Texture2D FolderIcon;
            public Texture2D LeftIcon;
            public Texture2D WindowBackgroundTex;
            public Texture2D SideTexture;
            public Texture2D SideTextureSecond;
            public Texture2D RunIcon;

            public Texture2D SelectedResultFieldTex;
            public Texture2D ResultFieldTex;

            public GUIStyle SearchStyle;
            public GUIStyle SearchLabelGroupStyle;
            public GUIStyle SearchLabelStyle;
            public GUIStyle SearchLabelHelpStyle;
            public GUIStyle SearchIconBackgroundStyle;
            public GUIStyle SearchIconStyle;
            public GUIStyle SearchLabelStyleEmpty;

            public GUIStyle TitleStyle;
            public GUIStyle TitleLabelStyle;

            public GUISkin DefaultSkin;
            public GUISkin ScrollBarStyle;

            public GUIStyle CommandResultLayoutForcedHighlightStyle;
            public GUIStyle CommandResultLayoutNoHighlightStyle;
            public GUIStyle CommandResultInsideLayoutStyle;
            public GUIStyle CommandResultGroupStyle;
            public GUIStyle CommandNameStyle;
            public GUIStyle ParamIconGroupStyle;
            public GUIStyle CategoryIconStyle;
            public GUIStyle RunIconStyle;

            public GUIStyle CommandHelpStyle;
            public GUIStyle CommandHelpStyleSelected;

            public static void Initialize()
            {
                if (instance != null) return;
                instance = new Style();
                instance.OnInitialize();
            }

            private void OnInitialize()
            {
                InitializeColor();
                InitializeTexture();
                InitializeStyles();
            }

            private void InitializeColor()
            {
                ColorUtility.TryParseHtmlString("#313131", out WindowColor);
                ColorUtility.TryParseHtmlString("#979797", out SearchFieldColor);
                this.SearchFieldTextColor = Color.black;
                this.SearchResultTextColor = Color.white;

                ColorUtility.TryParseHtmlString("#262626", out SelectedResultFieldColor);
                ColorUtility.TryParseHtmlString("#434343", out ResultFieldColor);
                ColorUtility.TryParseHtmlString("#8d8d8d", out CommandHelpTextColor);
                ColorUtility.TryParseHtmlString("#a0a0a0", out CommandHelpHighlightTextColor);

                ColorUtility.TryParseHtmlString("#db7f1a", out HighlightOnSelectedTextColor);
                ColorUtility.TryParseHtmlString("#ffc75a", out QuickNameTextColor);
            }

            private void InitializeTexture()
            {
                this.TopPanelGradientTexture = ColorTexture(1, 1, WindowColor);

                this.SearchFieldBackgroundTexture = ColorTexture(1, 1, SearchFieldColor);

                this.TitleTexture = ColorVerticalOutlineTexture(25, WindowColor, SearchFieldTextColor, 1);

                this.SearchIcon = EditorGUIUtility.FindTexture("Search Icon");
                this.FolderIcon = EditorGUIUtility.FindTexture("Folder Icon");
                this.LeftIcon = EditorGUIUtility.FindTexture("back");
                this.RunIcon = EditorGUIUtility.FindTexture("CollabMoved Icon");

                this.WindowBackgroundTex = ColorTexture(1, 1, WindowColor);
                SelectedResultFieldTex = ColorTexture(1, 1, SelectedResultFieldColor);
                ResultFieldTex = ColorTexture(1, 1, ResultFieldColor);
            }

            private void InitializeStyles()
            {
                this.DefaultSkin = GUI.skin;
                ScrollBarStyle = CreateInstance<GUISkin>();
                ScrollBarStyle.hideFlags = HideFlags.HideAndDontSave;
                ScrollBarStyle.verticalScrollbar.stretchWidth = true;
                ScrollBarStyle.verticalScrollbar.stretchHeight = true;
                ScrollBarStyle.verticalScrollbar.normal = new GUIStyleState {background = WindowBackgroundTex};

                SearchStyle = new GUIStyle {
                    margin = new RectOffset(0, 0, 0, 0),
                    stretchWidth = true,
                    fixedHeight = 75,
                    normal = {background = TopPanelGradientTexture}
                };

                SearchLabelGroupStyle = new GUIStyle {
                    margin = new RectOffset(20, 20, 20, 20),
                    stretchWidth = true,
                    stretchHeight = true,
                };

                SearchLabelStyle = new GUIStyle {
                    richText = true,
                    fontSize = 15,
                    alignment = TextAnchor.UpperLeft,
                    stretchWidth = true,
                    padding = new RectOffset(0, 0, 1, 1),
                    normal = {
                        textColor = SearchFieldTextColor,
                        background = this.SearchFieldBackgroundTexture,
                    }
                };

                SearchLabelHelpStyle = new GUIStyle {
                    richText = true,
                    fontSize = 15,
                    alignment = TextAnchor.UpperLeft,
                    padding = new RectOffset(0, 0, 1, 1),
                    stretchWidth = true,
                    normal = {
                        textColor = WindowColor,
                        background = this.SearchFieldBackgroundTexture,
                    },
                };

                SearchIconBackgroundStyle = new GUIStyle {
                    fixedWidth = 30,
                    padding = new RectOffset(10, 5, 4, 2),
                    normal = {background = this.SearchFieldBackgroundTexture}
                };

                SearchIconStyle = new GUIStyle {
                    fixedWidth = 14,
                    fixedHeight = 14,
                    stretchWidth = true,
                    stretchHeight = true,
                    normal = {background = SearchIcon}
                };

                SearchLabelStyleEmpty = new GUIStyle {
                    richText = true,
                    fontSize = 15,
                    fixedWidth = 0,
                    stretchWidth = false,
                    alignment = TextAnchor.UpperLeft,
                    padding = new RectOffset(0, 0, 1, 1),
                    normal = {
                        textColor = SearchFieldTextColor,
                        background = this.SearchFieldBackgroundTexture,
                    },
                };

                TitleStyle = new GUIStyle {
                    fixedHeight = 25f,
                    stretchWidth = true,
                    normal = {background = this.TitleTexture}
                };

                TitleLabelStyle = new GUIStyle {
                    richText = true,
                    fontSize = 13,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    normal = {textColor = Color.white}
                };

                CommandResultLayoutForcedHighlightStyle = new GUIStyle {
                    richText = true,
                    stretchWidth = true,
                    normal = {background = SelectedResultFieldTex, textColor = SearchResultTextColor},
                };

                CommandResultLayoutNoHighlightStyle = new GUIStyle {
                    richText = true,
                    stretchWidth = true,
                    normal = {textColor = SearchResultTextColor, background = ResultFieldTex},
                };

                CommandResultInsideLayoutStyle = new GUIStyle {
                    richText = true,
                    stretchWidth = true,
                    margin = new RectOffset(0, 0, 2, 2)
                };

                CommandResultGroupStyle = new GUIStyle {
                    margin = new RectOffset(10, 10, 5, 5),
                };

                CommandNameStyle = new GUIStyle {
                    richText = true,
                    fontSize = 14,
                    alignment = TextAnchor.MiddleLeft,
                    margin = new RectOffset(0, 0, 0, 0),
                    wordWrap = true
                };

                ParamIconGroupStyle = new GUIStyle {
                    fixedWidth = 21,
                    stretchHeight = true,
                    margin = new RectOffset(0, 10, 0, 0)
                };

                CategoryIconStyle = new GUIStyle {
                    fixedWidth = 18,
                    fixedHeight = 18,
                    alignment = TextAnchor.MiddleCenter,
                    normal = {background = this.FolderIcon}
                };

                RunIconStyle = new GUIStyle {
                    fixedWidth = 18,
                    fixedHeight = 18,
                    alignment = TextAnchor.MiddleCenter,
                    normal = {background = this.RunIcon}
                };

                CommandHelpStyle = new GUIStyle {
                    richText = true,
                    fontSize = 11,
                    margin = new RectOffset(0, 0, 4, 0),
                    stretchWidth = true,
                    normal = {textColor = CommandHelpTextColor},
                    wordWrap = true
                };

                CommandHelpStyleSelected = new GUIStyle {
                    richText = true,
                    fontSize = 11,
                    margin = new RectOffset(0, 0, 4, 0),
                    normal = {
                        textColor = CommandHelpHighlightTextColor
                    },
                    wordWrap = true
                };
            }

            public static Texture2D ColorTexture(int width, int height, Color color)
            {
                Color[] pix = new Color[width * height];
                for (int i = 0; i < pix.Length; i++)
                {
                    pix[i] = QualitySettings.activeColorSpace == ColorSpace.Linear ? color.linear : color;
                }
                Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
                result.hideFlags = HideFlags.HideAndDontSave;
                result.SetPixels(pix);
                result.Apply();

                return result;
            }

            private static Texture2D ColorVerticalOutlineTexture(int high, Color color, Color lineColor, int linePixel)
            {
                Color[] pix = new Color[high];
                color = QualitySettings.activeColorSpace == ColorSpace.Linear ? color.linear : color;
                lineColor = QualitySettings.activeColorSpace == ColorSpace.Linear ? lineColor.linear : lineColor;
                for (int i = 0; i < pix.Length; i++)
                {
                    if (i < linePixel || i >= high - linePixel)
                    {
                        pix[i] = lineColor;
                    }
                    else
                    {
                        pix[i] = color;
                    }
                }

                Texture2D result = new Texture2D(1, high, TextureFormat.RGBA32, false, true);
                result.hideFlags = HideFlags.HideAndDontSave;
                result.wrapMode = TextureWrapMode.Clamp;
                result.SetPixels(pix);
                result.Apply();

                return result;
            }
        }
    }
}