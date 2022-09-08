using System;
using System.IO;
using Cryptography;

namespace LORENZ
{
    static class Jeux
    {
        public static double TheGame(double Coins)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            int levelChoosed;
            while (true)
            {
                levelChoosed = LevelMenu();
                if (levelChoosed == -1)
                    return Coins;
                else if (levelChoosed == default)
                {
                    Console.Clear();
                    Display.PrintMessage("Ceci n'est pas une option valide.", MessageState.Failure);
                }
                else break;
            }
            int MaxRandNum = default;
            for (int i = 0; i < levelChoosed; i++)
                MaxRandNum += (int)(5 * Math.Pow(10, i));
            int RandomNum = new System.Random().Next(0, MaxRandNum + 1);
            Console.Clear();
            Console.WriteLine($"LORENZ a choisi un chiffre entre 0 et {MaxRandNum}.");
            Console.WriteLine("Essayez de le trouver !");
            Console.WriteLine("Appuyez sur ENTRÉE sans rien écrire pour quitter.");
            double WinCoins;
            int Try = 0;
            int ChoosenNum = -1;
            while (RandomNum != ChoosenNum)
            {
                string Choosen = Console.ReadLine();
                if (Choosen == "")
                    break;
                if (!int.TryParse(Choosen, out ChoosenNum))
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Ceci n'est pas un nombre, veuillez entrer un nombre entre 0 et {MaxRandNum}.");
                    Console.WriteLine("Appuyez sur ENTRÉE sans rien écrire pour quitter.");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                else if (ChoosenNum > MaxRandNum || ChoosenNum < 0)
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Ce nombre n'est pas compris entre 0 et {MaxRandNum}, veuillez entrer un nombre entre 0 et {MaxRandNum}.");
                    Console.WriteLine("Appuyez sur ENTRÉE sans rien écrire pour quitter.");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                else
                {
                    Try += 1;
                    WinCoins = Math.Ceiling((double)(MaxRandNum / 2 / Try));
                    Console.Clear();
                    if (RandomNum > ChoosenNum)
                        Console.WriteLine("C'est plus haut !");
                    else if (RandomNum < ChoosenNum)
                        Console.WriteLine("C'est plus bas !");
                    else
                    {
                        if (Try != 1)
                            Console.WriteLine("BRAVO ! Vous avez réussi en " + Try + " coups !");
                        else
                            Console.WriteLine("BRAVO ! Vous avez réussi du premier coup !");
                        Console.WriteLine("BILAN");
                        Console.WriteLine("Sous-total :         " + Coins + " Coins");
                        Coins += WinCoins;
                        Console.WriteLine("Montant gagné :      " + WinCoins + " Coins");
                        if (Try == 1)
                        {
                            Coins += WinCoins * 55;
                            Console.WriteLine("Bonus du 1er coup :  " + WinCoins * 55 + $" Coins ({WinCoins} x 55)");
                        }
                        else if (Try == 55)
                        {
                            Coins += Try * 55;
                            Console.WriteLine("Bonus persévérence : " + Try * 55 + $" Coins ({WinCoins} x 55)");
                        }
                        if (WinCoins == 55)
                        {
                            Coins += WinCoins * 55;
                            Console.WriteLine("Bonus du montant :   " + WinCoins * 55 + " Coins (55 x 55)");
                        }
                        if (RandomNum == 55)
                        {
                            Coins += WinCoins * 55;
                            Console.WriteLine("Bonus de la 55 :     " + WinCoins * 55 + $" Coins ({WinCoins} x 55)");
                        }
                        Console.WriteLine("-------------------------------------------------------");
                        Console.WriteLine("Nouveau solde :      " + Coins + " Coins");
                        WriteCoinsIntoFile(Coins);
                        Console.ReadKey(true);
                    }
                }
            }
            return Coins;
        }

        static int LevelMenu()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Veuillez choisir un niveau :\nAppuyez sur ESC pour annuler.");
            Console.WriteLine("[1] : DÉBUTANT");
            Console.WriteLine("[2] : INTERMÉDIAIRE");
            Console.WriteLine("[3] : AVANCÉ");
            Console.WriteLine("[4] : EXPERT");
            Console.WriteLine("[5] : MAÎTRE");
            Console.WriteLine("[6] : DIRECTEUR D'UNIVERSITÉ");
            Console.WriteLine("[7] : PREMIER MINISTRE");
            Console.WriteLine("[8] : GOUVERNEUR GÉNÉRAL");
            Console.WriteLine("[9] : DOYEN DE LA 55");
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

        static void WriteCoinsIntoFile(double coins)
        {
            string DirectoryPath = new FileInfo(Parametres.CoinsRecordFile).DirectoryName;
            if (!Directory.Exists(DirectoryPath))
                Directory.CreateDirectory(DirectoryPath);
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
