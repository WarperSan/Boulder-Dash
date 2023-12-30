using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using static Activité_Synthèse.Program;

namespace Activité_Synthèse.Classes
{
    class Enemy : Personnage
    {
        /// <summary>
        /// Comportements pouvant adopter les ennemis
        /// </summary>
        public enum EnemiesBehavior { FollowWalls, BreathFire, MoveBackAndForward, Projectile }

        /// <value>
        /// Utilisez la propriété Direction afin de conserver la direction du personnage
        /// </value>     
        public Direction DirectionActuelle = Direction.Haut;
        public bool Horaire = false;

        // Comportement que l'ennemi doit avoir
        public EnemiesBehavior behavior { get; set; }

        // Combien de passages avant que l'ennemi puisse agir ?
        public int UpdateCountMax = 0;

        // Nombre restant de passages
        public int UpdateCount = 0;

        // Projectile Section
        public int Lifespan = 1;
        public int Speed = 1;

        public string SpriteGauche = "";
        public string SpriteDroite = "";

        public int TailleProjectile = 1;

        public Enemy(string fichierImage, int x, int y, EnemiesBehavior _behavior, int updateCount) : base(fichierImage, x, y)
        {
            Position = new Vector2i(x, y);
            behavior = _behavior;
            UpdateCountMax = updateCount;
            UpdateCount = UpdateCountMax;
        }

        /// <summary>
        /// Fonction qui diffère de celle dans Personnage pour les projectiles plus grand qu'une case
        /// </summary>
        public new void Afficher(RenderWindow fenetre, float nbPixelsParCase)
        {
            if (TailleProjectile == 1)
            {
                base.Afficher(fenetre, nbPixelsParCase);
            }
            else
            {
                Vector2i tempPos = Position;

                // Quand Fygar crache vers la droite, le feu est 2 case trop loin

                if (DirectionActuelle == Direction.Gauche)
                    Position = new Vector2i(Position.X - (TailleProjectile - 1), Position.Y);

                base.Afficher(fenetre, nbPixelsParCase);

                Position = tempPos;
            }
        }

        // Représente les directions de déplacement de l'ennemi
        // L'ordre des directions a été changé pour faciliter la compréhension
        public enum Direction { Haut, Gauche, Bas, Droite };

        #region Behaviors
        /// <summary>
        /// Retourne la direction d'un ennemi tourné selon un sens donné
        /// </summary>
        /// <param name="enemy">Ennemi à tourner</param>
        /// <param name="horaire">Est-ce que la rotation est horaire (true) ou anti-horaire (false) ?</param>
        /// <returns>Nouvelle direction</returns>
        static Direction TurnBy(Enemy enemy, bool horaire, int count)
        {
            // Horaire : Haut -> Droite -> Bas -> Gauche -> Haut
            // Anti-horaire : Haut -> Gauche -> Bas -> Droite -> Haut

            Direction tempDir = enemy.DirectionActuelle;
            int index = 0;

            for (int i = 0; i < count; i++)
            {
                index = (int)tempDir;

                if (horaire)
                    index -= 1;
                else
                    index += 1;

                if (index > 3)
                {
                    index -= 4;
                }

                if (index < 0)
                {
                    index += 4;
                }

                tempDir = (Direction)index;
            }

            return tempDir;
        }

        /// <summary>
        /// Comportement selon lequel l'ennemi doit suivre les murs
        /// </summary>
        /// <param name="enemy">Ennemi qui doit suivre le comportement</param>
        /// <param name="carte">Carte à analyser</param>
        /// <returns>Nouvelle position de l'ennemi</returns>
        public static Vector2i FollowWalls(Enemy enemy, Objet[,] carte)
        {
            Vector2i initialPosition = enemy.Position;

            // Tourner la direction de 90° anti-horaire
            Direction tempDir = TurnBy(enemy, false, 1);

            // Si la case avec la direction tournée est vide
            if (!ColliderInFront(tempDir, initialPosition, carte))
            {
                // Mettre la direction à l'ennemi
                enemy.DirectionActuelle = tempDir;

                initialPosition = AvancerDansDirection(enemy.DirectionActuelle, initialPosition, 1);
            }
            else
            {
                // Si la case avec la direction originale est vide
                if (!ColliderInFront(enemy.DirectionActuelle, initialPosition, carte))
                {
                    initialPosition = AvancerDansDirection(enemy.DirectionActuelle, initialPosition, 1);
                }
                else
                {
                    // Tourner la direction de 90° horaire
                    enemy.DirectionActuelle = TurnBy(enemy, true, 1);
                }
            }

            return initialPosition;
        }

        /// <summary>
        /// Comportement selon lequel l'ennemi doit aller sur une case aléatoire
        /// </summary>
        /// <param name="enemy">Ennemi à déplacer</param>
        /// <param name="carte">Carte à analyser</param>
        /// <returns>Nouvelle position</returns>
        public static Vector2i RandomlyMove(Enemy enemy, Objet[,] carte)
        {
            Vector2i nextPos = enemy.Position;

            Direction rdmDirection = (Direction)new Random().Next(0, 4);

            bool willMove = true;

            if (ColliderInFront(rdmDirection, enemy.Position, carte))
            {
                rdmDirection = TurnBy(enemy, true, 1);

                if (ColliderInFront(rdmDirection, enemy.Position, carte))
                {
                    willMove = false;
                }
            }

            if (willMove)
            {
                if (rdmDirection == Direction.Gauche)
                    enemy.Texture = new Texture(enemy.SpriteGauche);

                if (rdmDirection == Direction.Droite)
                    enemy.Texture = new Texture(enemy.SpriteDroite);

                nextPos = AvancerDansDirection(rdmDirection, enemy.Position, 1);
            }

            return nextPos;
        }

        /// <summary>
        /// Comportement selon lequel l'ennemi doit cracher une flamme sur ses côtés (gauche ou droite)
        /// </summary>
        /// <param name="carte">Carte à analyser</param>
        public static void BreathFire(Enemy enemy, Objet[,] carte)
        {
            List<Direction> directions = new List<Direction>();

            if (!ColliderInFront(Direction.Gauche, enemy.Position, carte))
            {
                directions.Add(Direction.Gauche);
            }

            if (!ColliderInFront(Direction.Droite, enemy.Position, carte))
            {
                directions.Add(Direction.Droite);
            }

            if (directions.Count != 0)
            {
                Direction rdmDir = directions[new Random().Next(0, directions.Count)];

                Vector2i nextPos = AvancerDansDirection(rdmDir, enemy.Position, 1);

                string image;

                int distance = RaycastInDirection(rdmDir, enemy.Position, carte, 3);

                string directionString = rdmDir.ToString().ToLower();
                image = $"Images/Fygar/stage{distance}-{directionString}-fire24.bmp";
                enemy.Texture = new Texture($"Images/Fygar/fygar-{directionString}24.bmp");

                Enemy fire = new Enemy(image, nextPos.X, nextPos.Y, EnemiesBehavior.Projectile, 1);

                fire.TailleProjectile = distance;
                fire.Speed = 0;
                fire.Lifespan = enemy.UpdateCountMax;
                fire.DirectionActuelle = rdmDir;

                enemies.Add(fire);
                fire.Afficher(fenêtreDeJeu, NbPixelsParCase);

                enemy.UpdateCount = enemy.UpdateCountMax * (distance + 1);
            }
        }

        public static void MoveBackAndForward(Enemy enemy, Objet[,] carte)
        {
            if (RaycastInDirection(enemy.DirectionActuelle, enemy.Position, carte, 1) == 0)
            {
                enemy.DirectionActuelle = TurnBy(enemy, true, 2);

                enemy.Texture = new Texture($"Images/Ghastly/ghastly-{enemy.DirectionActuelle}24.bmp");
            }

            if (RaycastInDirection(enemy.DirectionActuelle, enemy.Position, carte, 1) == 1)
                enemy.Position = AvancerDansDirection(enemy.DirectionActuelle, enemy.Position, 1);
        }

        /// <summary>
        /// Test pour savoir si la case devant l'ennemi est libre (false) ou non (true)
        /// </summary>
        /// <param name="direction">Direction du mouvement</param>
        /// <param name="initialPosition">Origine du test</param>
        /// <param name="carte">Carte à analyser</param>
        /// <returns>Est-ce que l'ennemi rentre dans un mur (true) ou non (false) ?</returns>
        public static bool ColliderInFront(Direction direction, Vector2i initialPosition, Objet[,] carte)
        {
            initialPosition = AvancerDansDirection(direction, initialPosition, 1);

            bool result = carte[initialPosition.Y, initialPosition.X] != Objet.V;

            foreach (var enemy in enemies)
            {
                if (initialPosition == enemy.Position)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Calcule la position qu'aurait l'ennemi s'il se déplaçait dans la direction donnée à la vitesse donnée
        /// </summary>
        /// <param name="direction">Direction du déplacement</param>
        /// <param name="initialPosition">Position initiale</param>
        /// <param name="Speed">Vitesse (en cases) du déplacement</param>
        /// <returns></returns>
        public static Vector2i AvancerDansDirection(Direction direction, Vector2i initialPosition, int Speed)
        {
            Vector2i nextPos = initialPosition;

            switch (direction)
            {
                case Direction.Haut:
                    nextPos.Y -= Speed;
                    break;
                case Direction.Gauche:
                    nextPos.X -= Speed;
                    break;
                case Direction.Bas:
                    nextPos.Y += Speed;
                    break;
                case Direction.Droite:
                    nextPos.X += Speed;
                    break;
                default:
                    break;
            }

            return nextPos;
        }
        #endregion


    }
}
