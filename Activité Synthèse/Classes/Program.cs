// Activité Synthèse 420-KB1-LG
// Version 1.0: X novembre 2019 | Par ???
// - Traduit du C++ au C# 

// Version 1.1: 2 décembre 2019 | Par ???
// - Modification du namespace
// - Modification de la rotation quand Rockford ne fait rien 

// Version 1.2: 3 décembre 2019 | Par ???
// - Spécification du Framework .Net 4.7.2
// - Changement du nom de l'assembly 

// Version 1.3: 22 novembre 2022 | Par ???
// - Élimination de la partie "matrice de transformation"

// Version 1.4: 1 décembre 2022 | Par WarperSan
// - Modification de la méthode d'affichage de la carte
// - Introduction d'une fonction pour les Inputs
// - Introduction d'un système de déplacement pour le joueur
// - Introduction d'un système pour les roches (déplacements horizontaux, verticaux et diagonals)
// - Introduction d'un système d'ennemis
// - Restructure du code

// Version 1.5: 2 décembre 2022 | Par WarperSan
// - Correction des rochers suspendus
// - Changement des éboulements (Miner à côté de deux roches superposées => Miner à côté de deux roches superposées avec une case vide au-dessus)
//   Ceci peut créer des situations où une roche est sur une autre, mais ne tombe pas. (Ce n'est pas un bug, c'est un feature)
// - Ajout de la mort par un ennemi

// Version 1.6: 3 décembre 2022 | Par WarperSan
// - Ajout d'un menu de sélection des niveaux
// - Ajout d'un éditeur de niveau
// - Modification du fonctionnement des boutons (Les fonctions sont maintenant une Action au lieu d'un string qu'on met dans un switch case)

// Version 1.6.1: 4 décembre 2022 | Par WarperSan
// - Création du fond pour l'écran principal
// - Ajout d'un nouveau ennemi: Fygar
// - Ajout du comportement "Projectile"
// - Amélioration du système de dégât

// Version 1.6.2: 5 décembre 2022 | Par WarperSan
// - Finalisation de l'ennemi Fygar
// - Correction de bugs
// - Changement de l'apparence de Fygar (Déssaturation des couleurs)
// - Ajout de l'ennemi Fygar dans l'éditeur de niveau
// - Ajout de la mort des ennemis par les rochers
// - Processus de traduction des variables/fonctions/commentaires

// Version 1.6.3: 6 décembre 2022 | Par WarperSan
// - Ajout des intéractions entre les ennemis (Ils ne peuvent plus se foncer dedans)
// - Séparation de la classe Enemy de la classe Program
// - Utilisation de la fonction RaycastInDirection pour faire bouger les rochers vers le bas
// - Optimisation du système des rochers
// - Correction du chargement de la carte

// Version 1.6.4: 7 décembre 2022 | Par WarperSan
// - Ajout de musiques et de sfx

// Version 1.6.5: 8 décembre 2022 | Par WarperSan
// - Correction du feu du Fygar
// - Correction d'un bug avec les rochers tombants
// - Ajout de l'ennemi Ghastly
// - Correction du système de poussement des rochers
// - Ajout de décès des ennemis par les rochers poussés


// Version 1.6.6: 12 décembre 2022 | Par WarperSan
// - Correction du chargement de la carte dans l'éditeur de niveau
// - Ajout d'une protection lorsque le joueur essaye de charger une carte ouverte
// - Ajout d'une vérification pour les rochers dans l'éditeur de niveau
// - Correction du bug où les murs pouvaient devenir des rochers et pouvaient être poussés

// Version 1.7: 13 décembre 2022 | Par WarperSan
// - Ajout du thème de victoire
// - Ajout de sons pour les boutons
// - Importation de SFML à SFML Net 2
// - Changement des apparences des textes (Changement forcé par l'importation)
// - Ajout des éboulements par le dessous
// - Ajout des éboulements en chaine
// - Correction d'un bug où les bouttons s'activaient même si la fenêtre n'était pas active

using Activité_Synthèse.Classes;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Activité_Synthèse
{
    internal class Program
    {
        // Nombre de cases par ligne
        public const int NbColonnes = 40;

        // Nombre de lignes
        public const int NbLignes = 22;

        // Nombre de pixels d'une case
        public const int NbPixelsParCase = 24;
        const int VitessePapillon = 5;

        private static bool isPlayerImmortal = false;

        // Représente le contenu d'une case du tableau
        // Vide, Mur, Terre, Rocher, RocherTombant, Diamant
        public enum Objet { V, M, T, R, D };

        public const float VolumeLevel = 50;
        public static Random rdm = new Random();

        // L'état RocherTomber (RT) a été supprimé, car il est inutile.
        // La fonctionnalité d'éboulement des roches est gérée par la fonction MapTick & MoveRock.

        /// <summary>
        /// La classe Personnage sert à créer Boulderdash et son ennemi
        /// </summary>
        public class Personnage
        {
            /// <summary>
            /// Constructeur du personnage: pour créer un personnage, utilisez l'opérateur new
            /// </summary>
            /// <param name="fichierImage">Nom du fichier image</param>
            /// <param name="x">Position initiale en x</param>
            /// <param name="y">Position initiale en y</param>
            /// <param name="dir">Direction intiale</param>
            public Personnage(string fichierImage, int x, int y)
            {
                Position = new Vector2i(x, y);
                Texture = new Texture(fichierImage);
                Sprite = new Sprite(Texture);
            }

            /// <summary>
            /// Utilisez la méthode Afficher du personnage pour afficher le personnage
            /// dans une fenêtre
            /// </summary>
            /// <param name="fenetre">Objet représentant la fenêtre</param>
            /// <param name="echelle">Facteur multiplicatif permettant de passer de la position
            /// dans le tableau 2D à la position dans l'image</param>
            public void Afficher(RenderWindow fenetre, float nbPixelsParCase)
            {
                Vector2f position = new Vector2f(Position.X, Position.Y) * nbPixelsParCase;
                Sprite.Position = position;

                fenetre.Draw(Sprite);
            }

            // Les variables X et Y ont été réunies dans la variable Position, car ça facilite l'accès et l'unification
            public Vector2i Position { get; set; }

            // La propriété Direction a été déplacée dans la classe Enemy, puisqu'elle est utilisé que par les ennemis

            #region UI Section
            // Pour les ennemis qui doivent changer leur sprite selon leur direction, il est important que la texture soit accessible
            public Texture Texture
            {
                get
                {
                    return Sprite.Texture;
                }
                set
                {
                    if (Sprite == null)
                        Sprite = new Sprite();

                    Sprite.Texture = value;
                }
            }

            private Sprite Sprite { get; set; }
            #endregion
        }

        public static RenderWindow fenêtreDeJeu;
        static Image photoCarte;

        static Personnage joueur;
        static bool joueurEstMort;
        static bool joueurAGagné;

        public static List<Enemy> enemies = new List<Enemy>();

        static int diamantsRestants = 0;

        static bool carteModifiée = false;
        static List<Vector2i> casesModifiées = new List<Vector2i>();
        static List<Vector2i> fallingRocks = new List<Vector2i>();
        static Vector2i rocheSuspendue = new Vector2i(-1, -1);

        public static void Load(string path)
        {
            joueurEstMort = false;
            joueurAGagné = false;

            // Charger la carte depuis le fichier donné
            // La fonction a été légèrement modifiée pour permette de facilement charger un fichier donné
            Objet[,] carte = ChargerCarteJeu(path);

            fenêtreDeJeu = new RenderWindow(new VideoMode(NbColonnes * NbPixelsParCase, NbLignes * NbPixelsParCase), "Boulder Dash", Styles.Titlebar);

            if (carte != null)
            {
                Music theme = new Music("Sounds/Boulder Theme.wav");
                theme.Loop = true;
                theme.Volume = VolumeLevel;
                theme.Play();

                AfficherCarteJeu(carte, fenêtreDeJeu);

                // Rouler tant que le joueur n'est pas mort, n'a pas gagné ou n'a pas appuyé Escape
                while (!Keyboard.IsKeyPressed(Keyboard.Key.Escape) && (!joueurEstMort || isPlayerImmortal) && !joueurAGagné)
                {
                    //FPSTick(); // Fonction pour mesurer les performances START

                    fenêtreDeJeu.DispatchEvents();

                    // Nettoyer la fenêtre
                    fenêtreDeJeu.Clear();

                    // Dessiner la carte
                    fenêtreDeJeu.Draw(new Sprite(new Texture(photoCarte)));

                    // Obtenir les inputs
                    Vector2f inputs = GetInputs();

                    // Fonctions de Tick
                    // Regarder individuellement chaque fonction pour avoir plus d'informations sur chacune
                    MapTick(carte);
                    RockSuspensionTick(carte, inputs);

                    // Les ennemis sont affichés en ordre décroissant, car la modification de la liste enemies n'affecte pas la loop
                    for (int i = enemies.Count - 1; i >= 0; i--)
                    {
                        EnemyTick(enemies[i], carte);
                    }

                    PlayerTick(carte, inputs);

                    // Actualiser la fenêtre
                    fenêtreDeJeu.Display();

                    //FPSTick(); // Fonction pour mesurer les performances END

                    // Ralentir le jeu
                    Thread.Sleep(75);
                }

                theme.Stop();

                Sprite message = new Sprite();
                message.Position = new Vector2f(0, 0);
                Sound sound = new Sound(new SoundBuffer("Sounds/death.wav"));
                sound.Volume = 50;

                // Menus de victoire
                if (joueurEstMort)
                {
                    message.Texture = new Texture("Images/perdu24.bmp");
                }
                else if (joueurAGagné)
                {
                    message.Texture = new Texture("Images/gagne24.bmp");
                    sound = new Sound(new SoundBuffer("Sounds/Boulder Winning.wav"));
                }

                sound.Play();

                fenêtreDeJeu.Draw(new Sprite(new Texture(fenêtreDeJeu.Capture())));
                fenêtreDeJeu.Display();

                fenêtreDeJeu.Draw(message);
                fenêtreDeJeu.Display();

                Thread.Sleep(sound.SoundBuffer.Duration.AsMilliseconds());
            }

            TitleScreen.RetourTitleScreen(fenêtreDeJeu);
        }

        #region Tick Functions
        /// <summary>
        /// Fonction qui gère les rochers en suspension
        /// </summary>
        /// <param name="carte">Carte à analyser</param>
        /// <param name="inputs">Inputs du joueurs</param>
        static void RockSuspensionTick(Objet[,] carte, Vector2f inputs)
        {
            // S'il y a un rocher en suspension
            if (rocheSuspendue != new Vector2i(-1, -1))
            {
                // Si le joueur n'est plus en dessous du rocher
                if (joueur.Position.X != rocheSuspendue.X)
                {
                    // Faire bouger le rocher vers le bas
                    MoveRock(rocheSuspendue, carte, true, 1);

                    // Mettre à jour la carte graphique
                    photoCarte = fenêtreDeJeu.Capture();
                    fenêtreDeJeu.Draw(new Sprite(new Texture(photoCarte)));

                    int initY = rocheSuspendue.Y;

                    // Si la recherche se passe dans la carte
                    if (rocheSuspendue.Y - 1 >= 0)
                    {
                        // S'il y a un rocher sur le rocher qui était suspendu
                        if (carte[rocheSuspendue.Y - 1, rocheSuspendue.X] == Objet.R)
                        {
                            // Faire tomber le rocher
                            rocheSuspendue.Y--;
                        }
                    }

                    // Si aucun nouveau rocher n'a été mit
                    if (initY == rocheSuspendue.Y)
                    {
                        // Retirer le rocher en suspension
                        rocheSuspendue = new Vector2i(-1, -1);
                    }
                }
                // Si le joueur creuse vers le bas
                else if (inputs.Y == 1 && !MoveInteractions(inputs, joueur, carte))
                {
                    // Ajouter le rocher dans la liste des rochers à actualiser
                    fallingRocks.Add(rocheSuspendue);

                    // Signaler du changement de la carte
                    carteModifiée = true;
                }
            }
        }

        /// <summary>
        /// Fonction qui gère le joueur
        /// </summary>
        /// <param name="carte">Carte à analyser</param>
        /// <param name="inputs">Inputs du joueur</param>
        static void PlayerTick(Objet[,] carte, Vector2f inputs)
        {
            // Si le joueur doit bouger
            if (inputs.X != 0 || inputs.Y != 0)
            {
                // Si le joueur ne va pas être bloqué
                if (!MoveInteractions(inputs, joueur, carte))
                {
                    // Faire le déplacement
                    Vector2i nextPos = joueur.Position;

                    nextPos.X += (int)inputs.X;
                    nextPos.Y += (int)inputs.Y;

                    joueur.Position = nextPos;
                }
            }

            foreach (var enemy in enemies)
            {
                // Si une partie du projectile est sur la même case que le joueur
                if (enemy.behavior == Enemy.EnemiesBehavior.Projectile)
                {
                    Vector2i nextPos = enemy.Position;

                    for (int i = 0; i < enemy.TailleProjectile; i++)
                    {
                        if (nextPos == joueur.Position)
                        {
                            joueurEstMort = true;
                            break;
                        }

                        nextPos = Enemy.AvancerDansDirection(enemy.DirectionActuelle, nextPos, 1);
                    }
                }
                // Si l'ennemi est sur la même case que le joueur
                else if (enemy.Position == joueur.Position)
                {
                    joueurEstMort = true;
                }

                if (joueurEstMort)
                    break;
            }

            if (!joueurEstMort || isPlayerImmortal)
                joueur.Afficher(fenêtreDeJeu, NbPixelsParCase);
        }

        /// <summary>
        /// Fonction qui gère les événements liés à la carte
        /// </summary>
        /// <param name="carte">Carte à analyser</param>
        static void MapTick(Objet[,] carte)
        {
            // Si la carte a besoin d'être rafraichie
            if (carteModifiée)
            {
                carteModifiée = false;

                RectangleShape voidTile = new RectangleShape(new Vector2f(NbPixelsParCase, NbPixelsParCase));
                voidTile.FillColor = Color.Black;

                // Effacer chaque case nécessitant une modification
                foreach (var tile in casesModifiées)
                {
                    voidTile.Position = (Vector2f)tile * NbPixelsParCase;
                    fenêtreDeJeu.Draw(voidTile);
                }

                casesModifiées.Clear();

                for (int i = 0; i < fallingRocks.Count; i++)
                {
                    Vector2i rock = fallingRocks[i];
                    Vector2i nextPos = rock;

                    // Si le joueur est sous le rocher
                    if (joueur.Position.X == nextPos.X && joueur.Position.Y == nextPos.Y + 1)
                    {
                        // Si le joueur se déplace de sous une roche à sous une autre roche
                        if (rocheSuspendue != new Vector2i(-1, -1))
                        {
                            MoveRock(rocheSuspendue, carte, true, 1);
                        }

                        rocheSuspendue = rock;
                    }
                    else
                    {
                        MoveRock(nextPos, carte, true, 1);
                    }
                }

                // Reset la liste des rochers qui tombent
                fallingRocks.Clear();

                photoCarte = fenêtreDeJeu.Capture();
                fenêtreDeJeu.Draw(new Sprite(new Texture(photoCarte)));
            }
        }

        /// <summary>
        /// Fonction qui gère les comportements des ennemis
        /// </summary>
        /// <param name="enemy">Ennemi à analyser</param>
        /// <param name="carte">Carte à analyser</param>
        static void EnemyTick(Enemy enemy, Objet[,] carte)
        {
            // Retirer un tick
            enemy.UpdateCount--;

            // Si l'ennemi doit réagir
            if (enemy.UpdateCount == 0)
            {
                enemy.UpdateCount = enemy.UpdateCountMax;

                // if (enemy.Tick != null)
                //    enemy.Tick();

                switch (enemy.behavior)
                {
                    case Enemy.EnemiesBehavior.Projectile:
                        // Tuer le projectile s'il doit est mort ou s'il touche un mur
                        if (enemy.Lifespan < 0 || Enemy.ColliderInFront(enemy.DirectionActuelle, enemy.Position, carte) && enemy.Speed != 0)
                        {
                            enemies.Remove(enemy);
                            enemy = null;
                        }
                        else
                        {
                            enemy.Lifespan--;
                        }
                        break;
                    case Enemy.EnemiesBehavior.FollowWalls:
                        enemy.Position = Enemy.FollowWalls(enemy, carte);
                        break;
                    case Enemy.EnemiesBehavior.BreathFire:
                        if (rdm.NextDouble() < 0.3)
                        {
                            Enemy.BreathFire(enemy, carte);
                        }
                        else
                        {
                            enemy.Position = Enemy.RandomlyMove(enemy, carte);
                        }
                        break;
                    case Enemy.EnemiesBehavior.MoveBackAndForward:

                        Enemy.MoveBackAndForward(enemy, carte);
                        break;
                    default:
                        break;
                }
            }

            if (enemy != null)
            {
                enemy.Afficher(fenêtreDeJeu, NbPixelsParCase);
            }
        }
        #endregion

        #region Carte
        private static List<KeyValuePair<Vector2i, string>> SpecialCases = new List<KeyValuePair<Vector2i, string>>();

        /// <summary>
        /// Fonction qui gère le chargement de la carte
        /// </summary>
        /// <param name="path">Carte à charger</param>
        /// <returns>Carte chargée</returns>
        static Objet[,] ChargerCarteJeu(string path)
        {
            diamantsRestants = 0;
            SpecialCases.Clear();

            Objet[,] carte = new Objet[NbLignes, NbColonnes];

            string[] lines = LevelEditor.LoadFile(path, NbLignes);

            if (lines != null)
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    string[] values = lines[i].Split(',');

                    for (int j = 0; j < NbColonnes; j++)
                    {
                        try
                        {
                            carte[i, j] = (Objet)Enum.Parse(typeof(Objet), values[j]);

                            if (carte[i, j] == Objet.D)
                                diamantsRestants++;
                        }
                        catch (Exception)
                        {
                            // Si une case n'est pas valide (Comme "Joueur", elle sera placée dans la liste des cases spéciales et sera un espace vide)
                            SpecialCases.Add(new KeyValuePair<Vector2i, string>(new Vector2i(j, i), values[j]));

                            carte[i, j] = Objet.V;
                        }
                    }
                }

                if (diamantsRestants <= 0)
                    joueurAGagné = true;

                casesModifiées.Clear();
                fallingRocks.Clear();
                rocheSuspendue = new Vector2i(-1, -1);
                carteModifiée = false;

                // Charger les ennemis
                enemies.Clear();
                Enemy enemy;

                for (int i = SpecialCases.Count - 1; i >= 0; i--)
                {
                    enemy = null;

                    switch (SpecialCases[i].Value)
                    {
                        case "Papillon":
                            enemy = new Enemy("Images/papillon24.bmp", SpecialCases[i].Key.X, SpecialCases[i].Key.Y, Enemy.EnemiesBehavior.FollowWalls, VitessePapillon);
                            break;
                        case "Fygar":
                            enemy = new Enemy("Images/Fygar/fygar-droite24.bmp", SpecialCases[i].Key.X, SpecialCases[i].Key.Y, Enemy.EnemiesBehavior.BreathFire, 10);

                            enemy.SpriteDroite = "Images/Fygar/fygar-droite24.bmp";
                            enemy.SpriteGauche = "Images/Fygar/fygar-gauche24.bmp";
                            break;
                        case "Ghastly":
                            enemy = new Enemy("Images/Ghastly/ghastly-droite24.bmp", SpecialCases[i].Key.X, SpecialCases[i].Key.Y, Enemy.EnemiesBehavior.MoveBackAndForward, 1);

                            //if (rdm.Next(0, 2) == 0)
                            //{
                            enemy.DirectionActuelle = Enemy.Direction.Droite;
                            //}
                            break;
                        default:
                            break;
                    }

                    if (enemy != null)
                    {
                        enemies.Add(enemy);
                    }
                }

                // Le joueur pourrait apparaître à plusieurs endroits.
                // Le joueur va donc apparaitre à un des points alétoirement

                List<Vector2i> playerSpawns = new List<Vector2i>();

                foreach (var item in SpecialCases)
                {
                    if (item.Value == "Joueur")
                        playerSpawns.Add(item.Key);
                }

                Vector2i playerSpawn;

                if (playerSpawns.Count != 0)
                    playerSpawn = playerSpawns[new Random().Next(0, playerSpawns.Count)];
                else
                    playerSpawn = new Vector2i(1, 1);

                joueur = new Personnage("Images/heros24.bmp", playerSpawn.X, playerSpawn.Y);
            }
            else
            {
                carte = null;
            }
            return carte;
        }

        /// <summary>
        /// Afficher la carte donnée dans la fenêtre donnée
        /// </summary>
        /// <param name="carte">Carte à afficher</param>
        /// <param name="fenêtreDeJeu">Fenêtre à afficher la carte dedans</param>
        static void AfficherCarteJeu(Objet[,] carte, RenderWindow fenêtreDeJeu)
        {
            Sprite tile = new Sprite();

            for (int i = 0; i < NbLignes; i++)
            {
                for (int j = 0; j < NbColonnes; j++)
                {
                    switch (carte[i, j])
                    {
                        case Objet.V:
                            break;
                        case Objet.M:
                            tile.Texture = new Texture("Images/mur24.bmp");
                            break;
                        case Objet.T:
                            tile.Texture = new Texture("Images/terre24.bmp");
                            break;
                        case Objet.R:
                            tile.Texture = new Texture("Images/roche24.bmp");
                            break;
                        case Objet.D:
                            tile.Texture = new Texture("Images/diamant24.bmp");
                            break;
                        default:
                            Console.WriteLine($"ERROR: Tile Type Invalid {{{0};{1}}}", i, j);
                            break;
                    }

                    if (carte[i, j] != Objet.V)
                    {
                        tile.Position = new Vector2f(j * NbPixelsParCase, i * NbPixelsParCase);

                        fenêtreDeJeu.Draw(tile);
                    }
                }
            }

            photoCarte = fenêtreDeJeu.Capture();

            fenêtreDeJeu.Display();


        }
        #endregion

        /// <summary>
        /// Retourne les inputs directionnelles entrées par le joueur
        /// </summary>
        /// <returns>Mouvement du joueur</returns>
        static Vector2f GetInputs()
        {
            Vector2f movement = new Vector2f(0, 0);

            if (Keyboard.IsKeyPressed(Keyboard.Key.W) || Keyboard.IsKeyPressed(Keyboard.Key.Up))
                movement.Y -= 1;

            if (Keyboard.IsKeyPressed(Keyboard.Key.S) || Keyboard.IsKeyPressed(Keyboard.Key.Down))
                movement.Y += 1;

            // S'il y a aucun mouvement vertical (Mouvement vertical override mouvement horizontal)
            if (movement.Y == 0)
            {
                if (Keyboard.IsKeyPressed(Keyboard.Key.A) || Keyboard.IsKeyPressed(Keyboard.Key.Left))
                    movement.X -= 1;

                if (Keyboard.IsKeyPressed(Keyboard.Key.D) || Keyboard.IsKeyPressed(Keyboard.Key.Right))
                    movement.X += 1;
            }

            return movement;
        }

        /// <summary>
        /// Fonction qui gère les différentes interactions entre la carte et le joueur
        /// </summary>
        /// <param name="movement">Mouvement à appliquer</param>
        /// <param name="character">Joueur</param>
        /// <param name="carte">Carte à nalyser</param>
        /// <returns>Est-ce que le joueur va foncer dans un mur ?</returns>
        static bool MoveInteractions(Vector2f movement, Personnage character, Objet[,] carte)
        {
            bool result = false;

            Vector2i playerNextPos = new Vector2i((int)(character.Position.X + movement.X), (int)(character.Position.Y + movement.Y));

            Objet prochaineCase = carte[playerNextPos.Y, playerNextPos.X];

            // Si la prochaine case peut être traversée (Terre ou Diamant)
            if (prochaineCase == Objet.T || prochaineCase == Objet.D)
            {
                carte[playerNextPos.Y, playerNextPos.X] = Objet.V;

                if (prochaineCase == Objet.D)
                {
                    diamantsRestants--;

                    Sound diamantCollected = new Sound(new SoundBuffer("Sounds/diamant.wav"));
                    diamantCollected.Volume = VolumeLevel;
                    diamantCollected.Play();

                    if (diamantsRestants <= 0)
                    {
                        joueurAGagné = true;
                    }
                }
                // Si deux éboulement se peuvent dans la même situation, un seul doit arriver
                bool éboulementComplet = false;

                // Éboulement quand le joueur creuse vers le bas
                // Répéter 2 fois la vérification avec -1 (Haut Gauche) et 1 (Haut Droit)
                for (int i = -1; i < 2; i += 2)
                {
                    if (!éboulementComplet)
                    {
                        // Si la case à vérifier et la case sous celle à vérifier sont un rocher
                        if (carte[playerNextPos.Y, playerNextPos.X + i] == Objet.R && carte[playerNextPos.Y - 1, playerNextPos.X + i] == Objet.R)
                        {
                            // Si la case au-dessus du joueur est une case vide 
                            if (carte[playerNextPos.Y - 1, playerNextPos.X] == Objet.V)
                            {
                                Éboulement(playerNextPos, carte, i);

                                joueurEstMort = true;
                                éboulementComplet = true;
                            }
                        }
                    }
                }

                for (int i = -1; i < 2; i += 2)
                {
                    if (!éboulementComplet)
                    {
                        if (carte[playerNextPos.Y, playerNextPos.X + i] == Objet.R && carte[playerNextPos.Y + 1, playerNextPos.X + i] == Objet.R)
                        {
                            if (carte[playerNextPos.Y + 1, playerNextPos.X] == Objet.V)
                            {
                                Éboulement(playerNextPos, carte, i);

                                joueurEstMort = true;
                                éboulementComplet = true;
                            }
                        }
                    }
                }

                casesModifiées.Add(playerNextPos);

                carteModifiée = true;

                if (carte[playerNextPos.Y - 1, playerNextPos.X] == Objet.R)
                {
                    fallingRocks.Add(new Vector2i(playerNextPos.X, playerNextPos.Y - 1));
                }
            }
            // Si la prochaine case est rocher ou un mur
            else if (prochaineCase == Objet.R || prochaineCase == Objet.M)
            {
                // Bloquer le joueur
                result = true;

                if (prochaineCase == Objet.R)
                {
                    // Si le joueur n'essaye pas de poser un rocher hors du jeu
                    if (playerNextPos.X + movement.X >= 0 && playerNextPos.X + movement.X < NbColonnes && movement.X != 0)
                    {
                        if (RaycastInDirection((Enemy.Direction)(movement.X + 2), playerNextPos, carte, 1) == 1)
                        {

                            // Si la case au-dessus du joueur est un rocher
                            if (carte[playerNextPos.Y - 1, playerNextPos.X] == Objet.R)
                            {
                                // Ajouter le rocher dans la liste des rochers qui tombent
                                fallingRocks.Add(new Vector2i(playerNextPos.X, playerNextPos.Y - 1));

                                carteModifiée = true;
                            }

                            RectangleShape voidTile = new RectangleShape(new Vector2f(NbPixelsParCase, NbPixelsParCase));
                            voidTile.FillColor = Color.Black;

                            // Effacer graphiquement les ennemis
                            // Nous devons enlever les ennemis, puisqu'ils sont affichés avant l'appel de cette fonction.
                            // Ne pas les effacer graphiquement va créer une copie de l'ennemi qui est comme une case
                            foreach (var enemy in enemies)
                            {
                                for (int i = 0; i < enemy.TailleProjectile; i++)
                                {
                                    voidTile.Position = new Vector2f(enemy.Position.X + i * ((int)enemy.DirectionActuelle - 2), enemy.Position.Y) * NbPixelsParCase;

                                    fenêtreDeJeu.Draw(voidTile);
                                }
                            }

                            MoveRock(playerNextPos, carte, false, (int)movement.X);

                            // Mettre à jour la carte graphique
                            photoCarte = fenêtreDeJeu.Capture();
                            fenêtreDeJeu.Draw(new Sprite(new Texture(photoCarte)));

                            // Remettre les ennemis graphiquement
                            foreach (var enemy in enemies)
                            {
                                enemy.Afficher(fenêtreDeJeu, NbPixelsParCase);
                            }

                            // Aucun collision
                            result = false;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Fonction qui gère le déplacement des rochers
        /// </summary>
        /// <param name="initialPos">Position initiale</param>
        /// <param name="carte">Carte à analyser</param>
        /// <param name="useVertical">Est-ce que le déplacement est vertical (true) ou horizontal (false) ?</param>
        /// <param name="direction">Direction du déplacement ( -1 = Haut et Gauche; 1 = Bas et Droite)</param>
        static void MoveRock(Vector2i initialPos, Objet[,] carte, bool useVertical, int direction)
        {
            bool dessinerRocher = true;

            Vector2i rocher = initialPos;

            Sprite rockTile = new Sprite(new Texture("Images/roche24.bmp"));
            RectangleShape voidTile = new RectangleShape(new Vector2f(NbPixelsParCase, NbPixelsParCase));
            voidTile.FillColor = Color.Black;

            KeyValuePair<Vector2i, int> nextÉboulement = new KeyValuePair<Vector2i, int>(new Vector2i(-1, -1), 0);

            // Si le déplacement est vertical
            if (useVertical)
            {
                // S'il y a plusieurs roches l'une sur l'autre
                int indexY = -1;

                while (carte[initialPos.Y + indexY, initialPos.X] == Objet.R)
                {
                    fallingRocks.Add(new Vector2i(initialPos.X, initialPos.Y + indexY));
                    carte[initialPos.Y + indexY, initialPos.X] = Objet.V;

                    carteModifiée = true;
                    indexY--;
                }

                // Bouger jusqu'à ce que le rocher ne puisse plus avancer
                int index = RaycastInDirection(Enemy.Direction.Bas, initialPos, carte, NbLignes);

                for (int i = 1; i <= index; i++)
                {
                    for (int j = -1; j < 2; j += 2)
                    {
                        if (carte[initialPos.Y, initialPos.X + j] == Objet.R && carte[initialPos.Y - 1, initialPos.X + j] == Objet.R)
                        {
                            nextÉboulement = new KeyValuePair<Vector2i, int>(new Vector2i(initialPos.X + j, initialPos.Y - 1), -j);
                        }
                    }

                    initialPos.Y += direction;

                    // Si le rocher rencontre le joueur
                    if (joueur.Position == initialPos)
                        joueurEstMort = true;

                    // Si le rocher rencontre un ennemi
                    for (int j = enemies.Count - 1; j >= 0; j--)
                    {
                        if (enemies[j].Position == initialPos)
                        {
                            enemies.RemoveAt(j);
                        }
                    }
                }

                for (int i = -1; i < 2; i += 2)
                {
                    if (carte[initialPos.Y, initialPos.X + i] == Objet.V && carte[initialPos.Y + 1, initialPos.X + i] == Objet.V && carte[initialPos.Y + 1, initialPos.X] == Objet.R)
                    {
                        dessinerRocher = false;

                        nextÉboulement = new KeyValuePair<Vector2i, int>(initialPos, i);
                    }
                }

                Sound rockFell = new Sound(new SoundBuffer("Sounds/rockFell.wav"));
                rockFell.Play();
            }
            // Si le déplacement est horizontal
            else
            {
                initialPos.X += direction;

                // Si le rocher rencontre un ennemi
                for (int j = enemies.Count - 1; j >= 0; j--)
                {
                    if (enemies[j].Position == initialPos)
                    {
                        enemies.RemoveAt(j);
                    }
                }

                // Si le rocher a été bougé au-dessus du vide
                if (carte[initialPos.Y + 1, initialPos.X] == Objet.V)
                {
                    carte[initialPos.Y, initialPos.X] = Objet.V;

                    // Ne pas dessiner le rocher pour éviter les doublons
                    dessinerRocher = false;

                    MoveRock(initialPos, carte, true, 1);
                }
                else if (carte[initialPos.Y + 1, initialPos.X] == Objet.R)
                {
                    if (carte[initialPos.Y + 1, initialPos.X + direction] == Objet.V && carte[initialPos.Y, initialPos.X + direction] == Objet.V)
                    {
                        dessinerRocher = false;

                        MoveRock(initialPos, carte, false, direction);
                    }
                }

                if (carte[initialPos.Y - 1, initialPos.X - 2 * direction] == Objet.R && carte[initialPos.Y, initialPos.X - 2 * direction] == Objet.R)
                {
                    nextÉboulement = new KeyValuePair<Vector2i, int>(new Vector2i(initialPos.X - 2 * direction, initialPos.Y - 1), direction);
                }
            }

            voidTile.Position = (Vector2f)rocher * NbPixelsParCase;
            rockTile.Position = (Vector2f)(initialPos * NbPixelsParCase);

            carte[rocher.Y, rocher.X] = Objet.V;

            if (dessinerRocher)
            {
                // Ceci sert si le rocher doit bouger verticalement après un mouvement horizontal pour éviter de doubler le sprite
                carte[initialPos.Y, initialPos.X] = Objet.R;

                fenêtreDeJeu.Draw(rockTile);
            }

            fenêtreDeJeu.Draw(voidTile);

            if (nextÉboulement.Value != 0)
            {
                MoveRock(nextÉboulement.Key, carte, false, nextÉboulement.Value);
            }
        }

        public static int RaycastInDirection(Enemy.Direction direction, Vector2i initialPos, Objet[,] carte, int maximum)
        {
            int distance = 0;

            do
            {
                initialPos = Enemy.AvancerDansDirection(direction, initialPos, 1);

                if (initialPos.Y >= NbLignes || initialPos.Y < 0 || initialPos.X >= NbColonnes || initialPos.X < 0)
                    break;

                if (carte[initialPos.Y, initialPos.X] == Objet.V)
                    distance++;

                if (distance >= maximum)
                    break;

            } while (carte[initialPos.Y, initialPos.X] == Objet.V);

            return distance;
        }

        public static void Éboulement(Vector2i playerNextPos, Objet[,] carte, int i)
        {
            RectangleShape voidTile = new RectangleShape(new Vector2f(NbPixelsParCase, NbPixelsParCase));
            voidTile.FillColor = Color.Black;

            voidTile.Position = (Vector2f)playerNextPos * NbPixelsParCase;
            fenêtreDeJeu.Draw(voidTile);

            // Déplacer le rocher au-dessus du joueur (La fonction se chargera d'écraser le joueur)
            MoveRock(new Vector2i(playerNextPos.X + i, playerNextPos.Y), carte, false, -i);

            int index = 1;

            // Faire descendre les rochers qui se situent sur le rocher qui déboule
            while (carte[playerNextPos.Y - index, playerNextPos.X + i] == Objet.R)
            {
                MoveRock(new Vector2i(playerNextPos.X + i, playerNextPos.Y - index), carte, true, 1);
                index++;
            }

            photoCarte = fenêtreDeJeu.Capture();
            fenêtreDeJeu.Draw(new Sprite(new Texture(photoCarte)));
        }

        #region FPS Counter
        // Fonctions utilisées pour mesurer les performances du jeu
        public static Stopwatch fpsCounter = new Stopwatch();

        static void FPSTick()
        {
            if (fpsCounter.IsRunning)
            {
                fpsCounter.Stop();

                Console.Clear();
                Console.WriteLine(1000 / (int)fpsCounter.ElapsedMilliseconds + " fps");
                fpsCounter.Reset();
            }
            else
            {
                fpsCounter.Start();
            }
        }
        #endregion
    }
}