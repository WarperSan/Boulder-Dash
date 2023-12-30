using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Threading;
using static Activité_Synthèse.Program;
using Button = Activité_Synthèse.TitleScreen.Button;

namespace Activité_Synthèse
{
    // Comment ajouter un matériel ?
    // 1. Ajouter le nouveau matériel dans l'énumaration Matériel
    // 2. Ajouter la case pour le nouveau matériel dans DessinerCarte()
    // 3. Ajouter le sprite dans la liste des Sprites dans PlacerBoutonsTerrain()
    // 4. Manuellement assigner la fonction au nouveau bouton
    // 5. Ajouter le nouveau matériel dans SaveMap()
    // 6. Ajouter le nouveau matériel dans LoadMap()

    internal class LevelEditor
    {
        // Nombre de pixels autour du cadre
        public const int Contour = 200;

        // Énumération des objets pouvant être placés sur la carte
        // (Différents que celle correspondant aux valeurs possibles des cases d'une carte)
        public enum Matériel { Vide, Terre, Mur, Diamant, Roche, Papillon, Fygar, Ghastly, Joueur, Rien }

        private static bool closeLevelEditor = false;

        // Quel matériel l'utilisateur est présentement en train d'utiliser 
        // Si le matériel est Rien, aucun matériel est utilisé (Différent de Vide)
        public static Matériel récentMatériel = Matériel.Rien;

        private static Image tempCarte;

        // Carte qui est en modification
        public static Matériel[,] carte;

        // Cases à modifier
        private static List<KeyValuePair<Vector2i, Matériel>> CasesModifiées = new List<KeyValuePair<Vector2i, Matériel>>();

        private static Text selectionText = new Text("", new Font("zig.ttf"));
        private static Text saveText = new Text("", new Font("zig.ttf"));

        // Fenêtre de l'éditeur de niveau
        public static RenderWindow levelEditorWindow;

        public static Music levelEditorTheme = new Music("Sounds/Boulder Dash Extra.wav");

        /// <summary>
        /// Démarrer l'éditeur de niveau
        /// </summary>
        public static void StartLevelEditor()
        {
            TitleScreen.titleScreenTheme.Stop();
            TitleScreen.titleScreenWindow.SetVisible(false);

            levelEditorTheme.Volume = 50;
            levelEditorTheme.Loop = true;
            levelEditorTheme.Play();

            levelEditorWindow = new RenderWindow(new VideoMode(Program.NbColonnes * Program.NbPixelsParCase + Contour, Program.NbLignes * Program.NbPixelsParCase + Contour), "Level Editor", Styles.Titlebar);

            // Liste des boutons dans l'éditeur
            List<Button> buttons = new List<Button>();
            InitialiserBoutonsUI(buttons);

            // Fond du cadre
            RectangleShape carteBG = new RectangleShape(new Vector2f(Program.NbColonnes, Program.NbLignes) * Program.NbPixelsParCase);
            carteBG.FillColor = Color.Black;
            carteBG.Position = new Vector2f(Contour / 2 + 30, Contour / 2);

            tempCarte = levelEditorWindow.Capture();

            // Initialisation du texte montrant la sélection actuelle
            selectionText.Position = new Vector2f(10, levelEditorWindow.Size.Y - 20);
            selectionText.CharacterSize = 15;
            selectionText.Color = new Color(150, 150, 150);

            saveText.CharacterSize = 15;
            saveText.Position = new Vector2f(levelEditorWindow.Size.X - 250, levelEditorWindow.Size.Y - 50);

            // Initialiser Carte
            carte = new Matériel[Program.NbLignes, Program.NbColonnes];

            while (!closeLevelEditor)
            {
                levelEditorWindow.DispatchEvents();

                // Mettre un fond bleu foncé
                levelEditorWindow.Clear(new Color(0, 0, 139));

                TitleScreen.ButtonsTick(buttons, levelEditorWindow);

                levelEditorWindow.Draw(carteBG);
                if (Dessiner(carteBG))
                {
                    saveText.DisplayedString = "";
                }

                // Si la carte a été modifiée
                if (CasesModifiées.Count != 0)
                {
                    levelEditorWindow.Draw(new Sprite(new Texture(tempCarte)));

                    DessinerCarte(carteBG);
                }

                levelEditorWindow.Draw(new Sprite(new Texture(tempCarte)));

                // Show Material Buttons
                TitleScreen.ButtonsTick(materialCatalog[currentPageIndex], levelEditorWindow);

                levelEditorWindow.Draw(selectionText);
                levelEditorWindow.Draw(saveText);

                levelEditorWindow.Display();

                // Ralentir le jeu
                Thread.Sleep(10);

                // À noter que l'utilisateur peut sauter une ou plusieurs cases si la souris va plus vite que le programme
            }

            ChangeBrush(Matériel.Rien);
            saveText.DisplayedString = "";
            path = "Map.csv";

            TitleScreen.RetourTitleScreen(levelEditorWindow);
            closeLevelEditor = false;
            levelEditorTheme.Stop();
        }

        /// <summary>
        /// Fonction qui gère l'actualisation de la carte
        /// </summary>
        private static void DessinerCarte(RectangleShape canevas)
        {
            Sprite tile = new Sprite();

            foreach (var item in CasesModifiées)
            {
                // La texture n'est pas forcément la même que celle sur le bouton
                switch (item.Value)
                {
                    case Matériel.Vide:
                        tile.Texture = new Texture("Images/vide24.bmp");
                        break;
                    case Matériel.Terre:
                        tile.Texture = new Texture("Images/terre24.bmp");
                        break;
                    case Matériel.Diamant:
                        tile.Texture = new Texture("Images/diamant24.bmp");
                        break;
                    case Matériel.Roche:
                        tile.Texture = new Texture("Images/roche24.bmp");
                        break;
                    case Matériel.Mur:
                        tile.Texture = new Texture("Images/mur24.bmp");
                        break;
                    case Matériel.Papillon:
                        tile.Texture = new Texture("Images/papillon24.bmp");
                        break;
                    case Matériel.Joueur:
                        tile.Texture = new Texture("Images/heros24.bmp");
                        break;
                    case Matériel.Fygar:
                        tile.Texture = new Texture("Images/Fygar/fygar-droite24.bmp");
                        break;
                    case Matériel.Ghastly:
                        tile.Texture = new Texture("Images/Ghastly/ghastly-droite24.bmp");
                        break;
                    case Matériel.Rien:
                    default:
                        Console.WriteLine($"Mauvais type: {item.Key}");
                        break;
                }

                tile.Position = new Vector2f(item.Key.X * Program.NbPixelsParCase, item.Key.Y * Program.NbPixelsParCase) + canevas.Position;
                levelEditorWindow.Draw(tile);

                carte[item.Key.Y, item.Key.X] = item.Value;
            }

            CasesModifiées.Clear();

            tempCarte = levelEditorWindow.Capture();
        }

        /// <summary>
        /// Fonction qui gère le dessinement par l'utilisateur
        /// </summary>
        public static bool Dessiner(RectangleShape canevas)
        {
            bool drew = false;

            // Si l'utilisateur est en train de dessiner
            if (Mouse.IsButtonPressed(Mouse.Button.Left))
            {
                // Si l'utilisateur 
                if (récentMatériel != Matériel.Rien)
                {
                    // Position de la souris par rapport au cadre
                    Vector2i mousePos = Mouse.GetPosition(levelEditorWindow) - (Vector2i)canevas.Position;

                    // Position de la souris en case
                    Vector2i mousePosInCases = new Vector2i(mousePos.X / Program.NbPixelsParCase, mousePos.Y / Program.NbPixelsParCase);

                    // Si la souris en dans le cadre
                    if (mousePosInCases.X >= 0 && mousePosInCases.X < Program.NbColonnes && mousePosInCases.Y >= 0 && mousePosInCases.Y < Program.NbLignes)
                    {
                        // Modifier la case selon le matériel choisi
                        CasesModifiées.Add(new KeyValuePair<Vector2i, Matériel>(mousePosInCases, récentMatériel));
                        drew = true;
                    }
                }
            }

            return drew;
        }

        /// <summary>
        /// Fonction qui gère les échanges de pinceaux (de matériel)
        /// </summary>
        /// <param name="nouveauMatériel">Matériel à appliquer</param>
        public static void ChangeBrush(Matériel nouveauMatériel)
        {
            // Si le matériel à appliquer est le même que celui déjà choisi
            if (nouveauMatériel == récentMatériel)
            {
                // Retirer la capacité de dessiner puisque l'utilisateur a désélectionné le matériel
                récentMatériel = Matériel.Rien;

                selectionText.DisplayedString = "";
            }
            else
            {
                // Remplacer le matériel utilisé
                récentMatériel = nouveauMatériel;
                selectionText.DisplayedString = $"Current selection: {nouveauMatériel}";
            }
        }

        /// <summary>
        /// Fonction qui place des murs sur tout le contour du cadre
        /// </summary>
        public static void PlacerGénériquesMurs()
        {
            // Contours horizontaux
            for (int i = 0; i < Program.NbColonnes; i++)
            {
                CasesModifiées.Add(new KeyValuePair<Vector2i, Matériel>(new Vector2i(i, 0), Matériel.Mur));
                CasesModifiées.Add(new KeyValuePair<Vector2i, Matériel>(new Vector2i(i, Program.NbLignes - 1), Matériel.Mur));
            }

            // Contours verticaux
            for (int i = 1; i < Program.NbLignes - 1; i++)
            {
                CasesModifiées.Add(new KeyValuePair<Vector2i, Matériel>(new Vector2i(0, i), Matériel.Mur));
                CasesModifiées.Add(new KeyValuePair<Vector2i, Matériel>(new Vector2i(Program.NbColonnes - 1, i), Matériel.Mur));
            }
        }

        /// <summary>
        /// Fonction qui remplit la carte avec le matériel sélectionné. Engendre forcément un lag
        /// </summary>
        public static void RemplirCarte()
        {
            if (récentMatériel != Matériel.Rien)
            {
                for (int i = 0; i < Program.NbColonnes; i++)
                {
                    for (int j = 0; j < Program.NbLignes; j++)
                    {
                        if (carte[j, i] != récentMatériel)
                            CasesModifiées.Add(new KeyValuePair<Vector2i, Matériel>(new Vector2i(i, j), récentMatériel));
                    }
                }
            }
        }

        /// <summary>
        /// Fonction qui vide la carte. La vitesse du nettoyage dépendant de la quantité de cases à nettoyer
        /// </summary>
        public static void NettoyerCarte()
        {
            for (int i = 0; i < Program.NbColonnes; i++)
            {
                for (int j = 0; j < Program.NbLignes; j++)
                {
                    // Optimisation: Si la case n'est pas vide, la vider.
                    if (carte[j, i] != Matériel.Vide)
                        CasesModifiées.Add(new KeyValuePair<Vector2i, Matériel>(new Vector2i(i, j), Matériel.Vide));
                }
            }
        }

        /// <summary>
        /// Fonction qui crée les boutons donnant accès aux différents matériaux
        /// </summary>
        /// <returns>Liste des boutons des matériaux</returns>
        private static List<Button> PlacerBoutonsTerrain(int pageSize)
        {
            // Comme les boutons sont similaires (à part leur matériel), il est plus simple de répéter les propriétés semblables
            List<Button> boutons = new List<Button>();

            string[] Sprites = new string[]
            {
                "Images/terre24.bmp",
                "Images/mur24.bmp",
                "Images/roche24.bmp",
                "Images/diamant24.bmp",
                "Images/videIcon24.bmp",
                "Images/papillon24.bmp",
                "Images/Fygar/fygar-droite24.bmp",
                "Images/Ghastly/ghastly-droite24.bmp",
                "Images/heros24.bmp"
            };

            for (int i = 0; i < Sprites.Length; i++)
            {
                Button matérielBoutonTemplate = new Button(new Vector2f(10, 300 / pageSize * (i % 5 + 1)), true, "", 20, new Image(Sprites[i]));
                matérielBoutonTemplate.OutlineThickness = 2;
                boutons.Add(matérielBoutonTemplate);
            }

            boutons[0].Function = delegate () { ChangeBrush(Matériel.Terre); };
            boutons[1].Function = delegate () { ChangeBrush(Matériel.Mur); };
            boutons[2].Function = delegate () { ChangeBrush(Matériel.Roche); };
            boutons[3].Function = delegate () { ChangeBrush(Matériel.Diamant); };
            boutons[4].Function = delegate () { ChangeBrush(Matériel.Vide); };
            boutons[5].Function = delegate () { ChangeBrush(Matériel.Papillon); };
            boutons[6].Function = delegate () { ChangeBrush(Matériel.Fygar); };
            boutons[7].Function = delegate () { ChangeBrush(Matériel.Ghastly); };
            boutons[8].Function = delegate () { ChangeBrush(Matériel.Joueur); };

            return boutons;
        }

        private static Dictionary<int, List<Button>> InitialiserCatalog(int pageSize)
        {
            List<Button> allButtons = PlacerBoutonsTerrain(pageSize);

            Dictionary<int, List<Button>> catalog = new Dictionary<int, List<Button>>();

            if (allButtons.Count <= pageSize)
            {
                catalog.Add(0, allButtons);
            }
            else
            {
                int remainingSize = allButtons.Count;
                int index = 0;

                while (remainingSize - pageSize > 0)
                {
                    catalog.Add(index, allButtons.GetRange(pageSize * index, pageSize));

                    remainingSize -= pageSize;
                    index++;
                }

                if (remainingSize > 0)
                {
                    catalog.Add(index, allButtons.GetRange(pageSize * index, allButtons.Count - pageSize * index));
                }
            }

            return catalog;
        }

        private static int currentPageIndex = 0;
        private static Dictionary<int, List<Button>> materialCatalog = InitialiserCatalog(5);
        public static void ChangeIndexCatalog(int modification)
        {
            currentPageIndex += modification;

            if (currentPageIndex >= materialCatalog.Count)
                currentPageIndex = -1 + modification;

            if (currentPageIndex < 0)
                currentPageIndex += materialCatalog.Count;
        }

        private static string path = "Map.csv";
        private static bool mapLoaded = false;
        /// <summary>
        /// Fonction qui enregistre la carte dans un fichier .csv
        /// </summary>
        private static void SaveMap()
        {
            saveText.Color = Color.Yellow;
            saveText.DisplayedString = "Saving...";

            if (File.Exists(path) && !mapLoaded)
            {
                int index = 1;

                while (File.Exists($"Map{index}.csv"))
                {
                    index++;
                }

                path = $"Map{index}.csv";

            }

            File.Create(path).Close();
            string content = "";

            // Verify Map
            VerifyMap();

            for (int i = 0; i < carte.GetLength(0); i++)
            {
                for (int j = 0; j < carte.GetLength(1); j++)
                {
                    switch (carte[i, j])
                    {
                        case Matériel.Vide:
                            content += "V";
                            break;
                        case Matériel.Terre:
                            content += "T";
                            break;
                        case Matériel.Mur:
                            content += "M";
                            break;
                        case Matériel.Diamant:
                            content += "D";
                            break;
                        case Matériel.Roche:
                            content += "R";
                            break;
                        case Matériel.Papillon:
                            content += "Papillon";
                            break;
                        case Matériel.Fygar:
                            content += "Fygar";
                            break;
                        case Matériel.Ghastly:
                            content += "Ghastly";
                            break;
                        case Matériel.Joueur:
                            content += "Joueur";
                            break;
                        default:
                            content += "???";
                            Console.WriteLine($"La case [{i};{j}] est invalide.");
                            break;
                    }

                    if (j + 1 != carte.GetLength(1))
                        content += ",";
                }

                content += "\n";
            }

            File.WriteAllText(path, content);

            saveText.Color = Color.Green;


            if (mapLoaded)
            {
                mapLoaded = false;
                saveText.DisplayedString = "Map updated";
            }
            else
            {
                saveText.DisplayedString = "Saved";
            }
        }

        private static void LoadMap()
        {
            string fileName = null;
            Thread t = new Thread(() =>
            {
                fileName = TitleScreen.AskFile("Boulder Dash Map |*.csv");
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();

            if (fileName != null)
            {
                saveText.Color = Color.Yellow;
                saveText.DisplayedString = "Loading...";

                string[] lines = LoadFile(fileName, NbLignes);

                if (lines != null)
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string[] values = lines[i].Split(',');

                        for (int j = 0; j < NbColonnes; j++)
                        {
                            Matériel matériel = Matériel.Vide;

                            switch (values[j].Replace(" ", ""))
                            {
                                case "Joueur":
                                    matériel = Matériel.Joueur;
                                    break;
                                case "Papillon":
                                    matériel = Matériel.Papillon;
                                    break;
                                case "Fygar":
                                    matériel = Matériel.Fygar;
                                    break;
                                case "Ghastly":
                                    matériel = Matériel.Ghastly;
                                    break;
                                case "M":
                                    matériel = Matériel.Mur;
                                    break;
                                case "D":
                                    matériel = Matériel.Diamant;
                                    break;
                                case "T":
                                    matériel = Matériel.Terre;
                                    break;
                                case "R":
                                    matériel = Matériel.Roche;
                                    break;
                                case "V":
                                    matériel = Matériel.Vide;
                                    break;
                                default:
                                    Console.WriteLine($"Couldn't load properly the tile [{j},{i}] that is \"{values[j]}\"");
                                    break;
                            }

                            if (carte[i, j] != matériel)
                            {
                                CasesModifiées.Add(new KeyValuePair<Vector2i, Matériel>(new Vector2i(j, i), matériel));
                            }
                        }
                    }

                    path = fileName;
                    mapLoaded = true;

                    if (CasesModifiées.Count == 0)
                    {
                        CasesModifiées.Add(new KeyValuePair<Vector2i, Matériel>(new Vector2i(0, 0), Matériel.Mur));
                    }

                    saveText.Color = Color.Green;
                    saveText.DisplayedString = "Loaded !";
                }
                else
                {
                    saveText.Color = Color.Red;
                    saveText.DisplayedString = "Error";
                }
            }
        }

        public static string[] LoadFile(string path, int SizeY)
        {
            string[] lines = new string[SizeY];

            try
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    for (int i = 0; i < SizeY; i++)
                    {
                        lines[i] = reader.ReadLine();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occured with {path}. Please retry later");
                Console.WriteLine(e);
                return null;
            }

            return lines;
        }

        /// <summary>
        /// Fonction qui vérifie si la carte a certaines caractéristiques nécessaires
        /// </summary>
        private static void VerifyMap()
        {
            // Vérifier murs génériques
            for (int i = 0; i < Program.NbColonnes; i++)
            {
                if (carte[0, i] != Matériel.Mur)
                    carte[0, i] = Matériel.Mur;

                if (carte[Program.NbLignes - 1, i] != Matériel.Mur)
                    carte[Program.NbLignes - 1, i] = Matériel.Mur;
            }

            for (int i = 1; i < Program.NbLignes - 1; i++)
            {
                if (carte[i, 0] != Matériel.Mur)
                    carte[i, 0] = Matériel.Mur;

                if (carte[i, Program.NbColonnes - 1] != Matériel.Mur)
                    carte[i, Program.NbColonnes - 1] = Matériel.Mur;
            }

            // Vérifier si la carte a au moins un endroit où le joueur peut apparaître
            if (!DoesMapHasMaterial(Matériel.Joueur))
            {
                carte[1, 1] = Matériel.Joueur;
            }

            // Vérifier si la carte a au moins un diamant
            if (!DoesMapHasMaterial(Matériel.Diamant))
            {
                carte[2, 2] = Matériel.Diamant;
            }

            // Vérification des rochers
            for (int i = Program.NbColonnes - 2; i >= 0; i--)
            {
                for (int j = Program.NbLignes - 2; j >= 0; j--)
                {
                    if (carte[j, i] == Matériel.Roche)
                    {
                        if (carte[j + 1, i] == Matériel.Vide)
                        {
                            int index = j;

                            do
                            {
                                carte[index, i] = Matériel.Vide;
                                carte[index + 1, i] = Matériel.Roche;

                                index++;
                            } while (carte[index + 1, i] == Matériel.Vide);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Fonction qui cherche dans la carte si le matériel donnée y est présent au moins une fois
        /// </summary>
        /// <param name="matériel">Matériel à chercher</param>
        /// <returns>Est-ce que le matériel donné est présent au moins une fois ?</returns>
        private static bool DoesMapHasMaterial(Matériel matériel)
        {
            // Les break sont utilisés pour éviter de chercher tout le tableau
            // À noter que si la dernière case est la seule case avec le matériel X, le programme devra toujours le chercher au complet

            bool result = false;

            for (int i = 1; i < Program.NbColonnes - 1; i++)
            {
                for (int j = 1; j < Program.NbLignes; j++)
                {
                    if (carte[j, i] == matériel)
                    {
                        result = true;
                        break;
                    }
                }

                if (result)
                    break;
            }

            return result;
        }

        private static void InitialiserBoutonsUI(List<Button> buttons)
        {
            // Bouton pour placer des murs sur tout le contour du cadre
            Button addGenericWalls = new Button(new Vector2f(150, 10), true, "Place the border", 15, null);
            addGenericWalls.Function = PlacerGénériquesMurs;
            buttons.Add(addGenericWalls);

            // Bouton pour remplir la carte avec le matériel sélectionné
            Button fillMap = new Button(new Vector2f(150, 30), true, "Fill the map", 15, null);
            fillMap.Function = RemplirCarte;
            buttons.Add(fillMap);

            // Bouton pour vider la carte
            Button clearMap = new Button(new Vector2f(150, 50), true, "Empty the map", 15, null);
            clearMap.Function = NettoyerCarte;
            buttons.Add(clearMap);

            // Bouton pour sauvegarder la carte
            Button saveMap = new Button(new Vector2f(10, levelEditorWindow.Size.Y - 50), true, "Save the map", 15, null);
            saveMap.Function = SaveMap;
            buttons.Add(saveMap);

            // Bouton pour revenir au menu principal
            Button backTitle = new Button(new Vector2f(levelEditorWindow.Size.X - 400, 10), true, "Go back to main menu", 15, null);
            backTitle.Function = delegate () { closeLevelEditor = true; };
            buttons.Add(backTitle);

            // Bouton pour charger une carte
            Button loadCarte = new Button(new Vector2f(levelEditorWindow.Size.X - 250, levelEditorWindow.Size.Y - 20), true, "Load map", 15, null);
            loadCarte.Function = LoadMap;
            buttons.Add(loadCarte);

            Button nextPage = new Button(new Vector2f(10, 350), true, "Next", 15, null);
            nextPage.Function = delegate () { ChangeIndexCatalog(1); };
            buttons.Add(nextPage);

            Button prevPage = new Button(new Vector2f(10, 50), true, "Previous", 15, null);
            prevPage.Function = delegate () { ChangeIndexCatalog(-1); };
            prevPage.Position = new Vector2f(prevPage.Position.X, prevPage.Size.Y);
            buttons.Add(prevPage);
        }
    }
}