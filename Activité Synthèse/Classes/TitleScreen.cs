using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using static SFML.Window.Mouse;

namespace Activité_Synthèse
{
    internal class TitleScreen
    {
        public static RenderWindow titleScreenWindow;
        public static Music titleScreenTheme = new Music("Sounds/Boulder Dash.wav");
        static void Main()
        {
            // new Thread(delegate () { AnimationManager.Test(0, 3); }).Start();

            titleScreenWindow = new RenderWindow(new VideoMode(Program.NbColonnes * Program.NbPixelsParCase, Program.NbLignes * Program.NbPixelsParCase), "Boulder Dash", Styles.Titlebar);

            string titleName = "Boulder Dash";

            bool animationCompleted = false;
            Text title = new Text();
            title.DisplayedString = titleName;
            Color titleColor = new Color(0, 0, 0);
            title.Color = Color.Red;

            title.CharacterSize = 24;
            title.Font = new Font("zig.ttf");
            title.Position = new Vector2f((titleScreenWindow.Size.X - title.GetGlobalBounds().Width) / 2, 200);
            title.DisplayedString = "";

            int index = 0;
            Text animTitle = new Text(title);
            animTitle.DisplayedString = titleName.Substring(index, 1);
            animTitle.Position = new Vector2f(title.Position.X, -title.CharacterSize);

            Sprite bgSprite = new Sprite(new Texture("Images/title_screen.png"));

            Sound letterHit = new Sound(new SoundBuffer("Sounds/letterHit.wav"));
            letterHit.Volume = 25;

            // Animation qui se charge des lettres du titre qui tombent
            while (!animationCompleted)
            {
                titleScreenWindow.DispatchEvents();
                titleScreenWindow.Clear();

                // Faire bouger la lettre si elle n'est pas proche du titre
                if (title.Position.Y - animTitle.Position.Y > 0)
                {
                    animTitle.Position = new Vector2f(animTitle.Position.X, animTitle.Position.Y + 20);
                }
                else
                {
                    title.DisplayedString += titleName.Substring(index, 1);

                    titleScreenWindow.SetTitle(title.DisplayedString);

                    index++;

                    if (index == titleName.Length)
                    {
                        animationCompleted = true;
                    }
                    else
                    {
                        if (titleName.Substring(index, 1) == " ")
                        {
                            title.DisplayedString += " ";
                            index++;
                        }

                        animTitle.DisplayedString = "";
                        animTitle.Position = new Vector2f(animTitle.Position.X, -10);


                        for (int i = 0; i < index; i++)
                        {
                            animTitle.DisplayedString += " ";
                        }

                        animTitle.DisplayedString += titleName.Substring(index, 1);
                    }

                    letterHit.Play();
                    Thread.Sleep(letterHit.SoundBuffer.Duration.AsMilliseconds() / 2);
                }

                titleScreenWindow.Draw(title);
                titleScreenWindow.Draw(animTitle);

                titleScreenWindow.Display();
                Thread.Sleep(10);
            }

            titleScreenTheme.Loop = true;
            titleScreenTheme.Volume = 0;
            titleScreenTheme.Play();

            // Animation qui dévoile la carte dans le fond et qui change la couleur du titre
            for (int i = 0; i < 7; i++)
            {
                titleScreenWindow.Clear();

                bgSprite.Color = ColorLerping(Color.Black, Color.White, i, 7);

                titleScreenWindow.Draw(bgSprite);

                AlternateColors(title, Color.Red, Color.Yellow, titleScreenWindow);
                titleScreenWindow.Display();
                titleScreenTheme.Volume += 50 / 7f;
                Thread.Sleep(300);
            }

            uint baseCharSize = 24;
            uint futureCharSize = 34;

            title.CharacterSize = futureCharSize;
            Vector2f bigTitlePos = new Vector2f((titleScreenWindow.Size.X - title.GetGlobalBounds().Width) / 2, title.Position.Y - 100);
            title.CharacterSize = baseCharSize;

            int deltaX = (int)((bigTitlePos.X - title.Position.X) / Math.Abs(futureCharSize - baseCharSize));
            int deltaY = (int)((bigTitlePos.Y - title.Position.Y) / Math.Abs(futureCharSize - baseCharSize));

            // Animation qui agrandit le titre et le place à sa place
            for (int i = 0; i < Math.Abs(futureCharSize - baseCharSize); i++)
            {
                Vector2f transition = title.Position;

                transition.X += deltaX;
                transition.Y += deltaY;

                title.Position = transition;
                title.CharacterSize += 1;

                titleScreenWindow.DispatchEvents();
                titleScreenWindow.Clear();
                titleScreenWindow.Draw(bgSprite);
                titleScreenWindow.Draw(title);

                titleScreenWindow.Display();
                Thread.Sleep(10);
            }

            title.Style = Text.Styles.Underlined;

            // Création des boutons
            List<Button> buttons = new List<Button>();

            Button testButton = new Button(new Vector2f(title.Position.X - 100, title.Position.Y + 100), true, "Load a map", 20, null);
            testButton.OutlineThickness = 2;
            testButton.Function = LoadMap;

            buttons.Add(testButton);

            Button levelEditorButton = new Button(new Vector2f(title.Position.X - 100, title.Position.Y + 200), true, "Enter the level editor", 20, null);
            levelEditorButton.OutlineThickness = 2;
            levelEditorButton.Function = LevelEditor.StartLevelEditor;
            buttons.Add(levelEditorButton);

            RectangleShape menuBg = new RectangleShape();
            menuBg.FillColor = new Color(0, 0, 0, 180);
            menuBg.Size = new Vector2f(480, 160);
            menuBg.Position = new Vector2f(200, 180);

            // Menu principal
            while (titleScreenWindow.IsOpen)
            {
                titleScreenWindow.DispatchEvents();
                titleScreenWindow.Clear();
                titleScreenWindow.Draw(bgSprite);
                titleScreenWindow.Draw(title);
                titleScreenWindow.Draw(menuBg);

                ButtonsTick(buttons, titleScreenWindow);

                titleScreenWindow.Display();
            }
        }

        private static void AlternateColors(Text text, Color color1, Color color2, RenderWindow window)
        {
            if (text.Color == color1)
            {
                text.Color = color2;
            }
            else
            {
                text.Color = color1;
            }

            window.Draw(text);
        }

        private static bool mouseButtonReleased = true;

        public static string AskFile(string extensions)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = extensions;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                return openFileDialog.FileName;
            }

            return null;
        }

        #region Button Section
        public class Button
        {
            // Est-ce que le bouton est actif ?
            public bool Active = true;

            // Position du bouton
            public Vector2f Position = new Vector2f(0, 0);

            // Grandeur du bouton (Largeur; Hauteur)
            public Vector2f Size { get; set; }

            public Image Sprite;
            public Vector2f Scale = new Vector2f(1, 1);

            // Fonction à appeler lors de l'appui (Voir la fonction ButtonClick pour voir/modifier les fonctions)
            public Action Function;

            // Texte à afficher pour le bouton
            public string Text = "Base Text";

            // Taille du texte à afficher
            public uint CharacterSize = 10;

            // Police d'écriture à utiliser
            public Font Font = new Font("zig.ttf");

            // Couleur du couleur des lettres
            public Color OutlineColor = Color.Black;

            // Largeur du contour des lettres
            public float OutlineThickness = 3;

            // Couleur du texte
            public Color FillColor = Color.White;

            // Constructeur d'un bouton
            public Button(Vector2f Position, bool Active, string Text, uint CharacterSize, Image Sprite)
            {
                this.Position = Position;
                this.Active = Active;
                this.Text = Text;
                this.CharacterSize = CharacterSize;
                this.Sprite = Sprite;

                this.Size = GetSize();
            }

            /// <summary>
            /// Fonction qui gère le dessinement du bouton
            /// </summary>
            /// <param name="window">Fenêtre à afficher le bouton à l'intérieur</param>
            public void Draw(RenderWindow window)
            {
                // Si le bouton est actif
                if (Active)
                {
                    // Créer le bouton étant composé d'une image de fond et d'une zone de texte
                    Sprite buttonSprite = new Sprite();

                    if (Sprite != null)
                        buttonSprite.Texture = new Texture(Sprite);

                    Text buttonText = new Text(Text, Font);
                    buttonText.Position = Position;
                    buttonText.Color = FillColor;
                    buttonText.CharacterSize = CharacterSize;

                    buttonSprite.Position = new Vector2f(buttonText.Position.X, buttonText.Position.Y - 2.5f);

                    // Adapter la taille du bouton en fonction de l'image
                    if (Sprite != null)
                    {
                        buttonSprite.Scale = Scale;
                    }

                    // Dessiner le bouton
                    window.Draw(buttonSprite);
                    window.Draw(buttonText);
                }
            }

            /// <summary>
            /// Détecte si l'utilisateur a appuyé sur le bouton
            /// </summary>
            /// <param name="point">Point où l'utilisateur a appuyé</param>
            /// <returns>L'utilisateur a-t-il appuyé sur le bouton (true) ou non (false)</returns>
            public bool IsPositionInBounds(Vector2i point, RenderWindow window)
            {
                bool result = false;

                // Si le point est sur le bouton
                if (point.X >= Position.X && point.X <= Position.X + Size.X && point.Y >= Position.Y && point.Y <= Position.Y + Size.Y)
                {
                    if (window.HasFocus())
                        result = true;
                }

                return result;
            }

            /// <summary>
            /// Que doit faire le programme quand le bouton a été appuyé
            /// </summary>
            /// <param name="Function">Fonction à exécuter</param>
            public void Click()
            {
                if (Function != null)
                {
                    new Sound(new SoundBuffer("Sounds/buttonClick.wav")).Play();
                    Function();
                }
                else
                {
                    Console.WriteLine("Aucun fonction assignée");
                }
            }

            /// <summary>
            /// Mesure et renvoie la taille du bouton
            /// </summary>
            /// <returns>Taille du bouton</returns>
            public Vector2f GetSize()
            {
                Vector2f size;

                if (Text == "")
                {
                    if (Sprite != null)
                    {
                        size = new Vector2f(Sprite.Size.X * Scale.X, Sprite.Size.Y * Scale.Y);
                    }
                    else
                    {
                        size = new Vector2f(100, 100);
                    }

                }
                else
                {
                    Text measuring = new Text(Text, Font);
                    measuring.CharacterSize = CharacterSize;

                    size = new Vector2f(measuring.GetLocalBounds().Width, measuring.GetLocalBounds().Height);
                }

                return size;
            }
        }

        /// <summary>
        /// Fonction qui gère les intéractions entre l'utilisateur et les boutons
        /// </summary>
        /// <param name="buttons">Boutons à afficher et à intéragir</param>
        /// <param name="window">Fenêtre où les boutons se trouvent</param>
        public static void ButtonsTick(List<Button> buttons, RenderWindow window)
        {
            foreach (var button in buttons)
            {
                button.Draw(window);
            }

            bool leftMouseButtonPressed = IsButtonPressed(Mouse.Button.Left);

            if (leftMouseButtonPressed && mouseButtonReleased)
            {
                Vector2i mousePos = GetPosition(window);

                for (int i = 0; i < buttons.Count; i++)
                {
                    if (buttons[i].Active)
                    {
                        if (buttons[i].IsPositionInBounds(mousePos, window))
                        {
                            mouseButtonReleased = false;

                            buttons[i].Click();
                            break;
                        }
                    }
                }
            }
            else
            {
                if (!leftMouseButtonPressed)
                    mouseButtonReleased = true;
            }
        }

        #endregion

        private static void LoadMap()
        {
            string fileName = null;
            Thread t = new Thread(() =>
            {
                fileName = AskFile("Boulder Dash Map |*.csv");
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();

            if (fileName != null)
            {
                titleScreenTheme.Stop();
                titleScreenWindow.SetVisible(false);
                Program.Load(fileName);
            }
        }

        public static void RetourTitleScreen(RenderWindow window)
        {
            window.Close();
            titleScreenWindow.SetVisible(true);
            titleScreenTheme.Play();
        }

        public static Color ColorLerping(Color initialColor, Color finalColor, int currentIteration, int maxIteration)
        {
            double pourcentage = (double)currentIteration / maxIteration;

            Color color = new Color(
                (byte)((finalColor.R - initialColor.R) * pourcentage),
                (byte)((finalColor.G - initialColor.G) * pourcentage),
                (byte)((finalColor.B - initialColor.B) * pourcentage));

            return color;
        }
    }
}