using Cryptography;
using System;
using System.IO;

namespace LORENZ
{
    public static class Jeux
    {
        public static void TheGame(ref double coins)
        {
            Console.Clear();
            Extensions.AfficherTitre("Le jeu LORENZ", ConsoleColor.DarkBlue);
            Console.ForegroundColor = ConsoleColor.Yellow;
            int levelChoosed;
            while (true)
            {
                levelChoosed = LevelMenu();
                if (levelChoosed == -1)
                    return;
                else if (levelChoosed == default)
                {
                    Console.Clear();
                    Display.PrintMessage("Ceci n'est pas une option valide.", MessageState.Failure);
                }
                else break;
            }
            int maxRandNum = default;
            for (int i = 0; i < levelChoosed; i++)
                maxRandNum += (int)(5 * Math.Pow(10, i));
            int randomNum = new System.Random().Next(0, maxRandNum + 1);
            Console.Clear();
            Console.WriteLine($"LORENZ a choisi un chiffre entre 0 et {maxRandNum}.");
            Console.WriteLine("Essayez de le trouver !");
            Console.WriteLine("Appuyez sur ENTRÉE sans rien écrire pour quitter.");
            double winCoins;
            int tries = 0;
            int choosenNum = -1;
            while (randomNum != choosenNum)
            {
                string choosen = Console.ReadLine();
                if (choosen == "")
                    break;
                if (!int.TryParse(choosen, out choosenNum))
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Ceci n'est pas un nombre, veuillez entrer un nombre entre 0 et {maxRandNum}.");
                    Console.WriteLine("Appuyez sur ENTRÉE sans rien écrire pour quitter.");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                else if (choosenNum > maxRandNum || choosenNum < 0)
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Ce nombre n'est pas compris entre 0 et {maxRandNum}, veuillez entrer un nombre entre 0 et {maxRandNum}.");
                    Console.WriteLine("Appuyez sur ENTRÉE sans rien écrire pour quitter.");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                else
                {
                    tries += 1;
                    winCoins = Math.Ceiling((double)(maxRandNum / 2 / tries));
                    Console.Clear();
                    if (randomNum > choosenNum)
                        Console.WriteLine("C'est plus haut !");
                    else if (randomNum < choosenNum)
                        Console.WriteLine("C'est plus bas !");
                    else
                    {
                        if (tries != 1)
                            Console.WriteLine("BRAVO ! Vous avez réussi en " + tries + " coups !");
                        else
                            Console.WriteLine("BRAVO ! Vous avez réussi du premier coup !");
                        Console.WriteLine("BILAN");
                        Console.WriteLine("Sous-total :         " + coins + " Coins");
                        coins += winCoins;
                        Console.WriteLine("Montant gagné :      " + winCoins + " Coins");
                        if (tries == 1)
                        {
                            coins += winCoins * 55;
                            Console.WriteLine("Bonus du 1er coup :  " + winCoins * 55 + $" Coins ({winCoins} x 55)");
                        }
                        else if (tries == 55)
                        {
                            coins += tries * 55;
                            Console.WriteLine("Bonus persévérence : " + tries * 55 + $" Coins ({winCoins} x 55)");
                        }
                        if (winCoins == 55)
                        {
                            coins += winCoins * 55;
                            Console.WriteLine("Bonus du montant :   " + winCoins * 55 + " Coins (55 x 55)");
                        }
                        if (randomNum == 55)
                        {
                            coins += winCoins * 55;
                            Console.WriteLine("Bonus de la 55 :     " + winCoins * 55 + $" Coins ({winCoins} x 55)");
                        }
                        Console.WriteLine("-------------------------------------------------------");
                        Console.WriteLine("Nouveau solde :      " + coins + " Coins");
                        WriteCoinsIntoFile(coins);
                        Console.ReadKey(true);
                    }
                }
            }
        }

        private static int LevelMenu()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Veuillez choisir un niveau :\nAppuyez sur ESC pour annuler.");
            Console.WriteLine("[1] : DÉBUTANT");
            Console.WriteLine("[2] : INTERMÉDIAIRE");
            Console.WriteLine("[3] : AVANCÉ");
            Console.WriteLine("[4] : EXPERT");
            Console.WriteLine("[5] : MAÎTRE");
            Console.WriteLine("[6] : GRAND MAÎTRE");
            Console.WriteLine("[7] : NITROGLYCÉRINE");
            Console.WriteLine("[8] : TRINITROTOLUÈNE");
            Console.WriteLine("[9] : DOYEN DE LA 55 - TÉTRAÉTYLMÉTHANE");
            int level = (int)Console.ReadKey(true).Key;
            /* ConsoleKey.Escape = 27
             * ConsoleKey.D1 = 49
             * ConsoleKey.D9 = 57
             * ConsoleKey.NumPad1 = 97
             * ConsoleKey.NumPad9 = 105 */
            if (level >= 49 && level <= 57)
                return level - 48;
            else if (level >= 97 && level <= 105)
                return level - 96;
            else if (level == 27)
                return -1;
            else
                return default;
        }

        private static void WriteCoinsIntoFile(double coins)
        {
            string directoryPath = new FileInfo(Parametres.CoinsRecordFile).DirectoryName;
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
            Cryptographie.ChiffrerFichier(Encryption.CreateScrambledMessage(coins.ToString()), Parametres.CoinsRecordFile);
        }

        public static double ReadCoinsInfoFile()
        {
            try
            {
                string[] coinsStr = Decyphering.StripOutAndSplit(Cryptographie.DechiffrerFichier(Parametres.CoinsRecordFile));
                return double.Parse(coinsStr[0]);
            }
            catch (LORENZException)
            {
                return 0.00;
            }
            catch (CryptographyException)
            {
                return 0.00;
            }
        }
    }
}
